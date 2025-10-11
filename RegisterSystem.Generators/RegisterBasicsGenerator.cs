using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static RegisterSystem.Generator.Util;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RegisterSystem.Generator;

[Generator]
public class RegisterBasicsGenerator : RegisterManageGenerator {

    protected override bool ShouldProcessClass(INamedTypeSymbol classSymbol) {
        return InheritsFromType(classSymbol, RegisterBasicsTypeName);
    }

    protected override ITypeSymbol? GetConstraintType(INamedTypeSymbol classSymbol) {
        // RegisterBasics 没有泛型约束，直接返回 RegisterBasics 类型
        return null;
    }

    protected override MethodDeclarationSyntax? FindSetupMethod(ClassDeclarationSyntax classDeclarationSyntax) {
        // RegisterBasics 不强制要求 setup 方法，可以查找任何 setup 方法（override 或非 override）
        var setupMethod = classDeclarationSyntax.Members
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(method => method.Identifier.ValueText == SetUpMethodName);
        
        // 如果没有找到 setup 方法，创建一个虚拟的空方法体用于后续处理
        if (setupMethod == null) {
            return MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier(SetUpMethodName))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)))
                .WithBody(Block()); // 空方法体
        }
        
        return setupMethod;
    }

    protected override void ProcessClass(SourceProductionContext sourceContext, GeneratorSyntaxContext generatorSyntaxContext) {
        SemanticModel semanticModel = generatorSyntaxContext.SemanticModel;

        if (generatorSyntaxContext.Node is not ClassDeclarationSyntax classDeclarationSyntax) {
            return;
        }

        INamedTypeSymbol? classSymbol = ModelExtensions.GetDeclaredSymbol(semanticModel, classDeclarationSyntax) as INamedTypeSymbol;

        if (classSymbol is null) {
            return;
        }

        // 先检查是否应该处理这个类，再检查是否为 partial
        if (!ShouldProcessClass(classSymbol)) {
            return;
        }

        if (!IsPartialClass(classDeclarationSyntax)) {
            sourceContext.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.notPartial,
                    classDeclarationSyntax.GetLocation(),
                    classSymbol.Name
                )
            );
            return;
        }

        // 获取泛型约束类型
        ITypeSymbol? constraintType = GetConstraintType(classSymbol);

        MethodDeclarationSyntax? setUpMethodDeclarationSyntax = FindSetupMethod(classDeclarationSyntax);

        // 对于 RegisterBasics，setup 方法不是必需的
        if (setUpMethodDeclarationSyntax is null || setUpMethodDeclarationSyntax.Body is null) {
            // 如果没有 setup 方法或方法体为空，创建一个空的字段定义列表
            GenerateCode(sourceContext, classSymbol, classDeclarationSyntax, 
                MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier(SetUpMethodName))
                    .WithBody(Block()), 
                semanticModel, constraintType);
            return;
        }

        GenerateCode(sourceContext, classSymbol, classDeclarationSyntax, setUpMethodDeclarationSyntax, semanticModel, constraintType);
    }

    protected override string GetGeneratedMethodName() {
        return "getAdditionalRegister";
    }

    protected override string GetGeneratedMethodReturnType() {
        return "System.Collections.Generic.IEnumerable<(RegisterBasics son, string name)>";
    }

    protected override MemberDeclarationSyntax[] GenerateProperties(List<FieldDefinition> filteredFieldDefinition) {
        return filteredFieldDefinition.Select(
                f => {
                    // 根据 IsNullable 决定类型名称
                    string typeName = f.IsNullable ? f.Type + "?" : f.Type;
                    
                    var property = PropertyDeclaration(
                            IdentifierName(typeName),
                            Identifier(f.Name)
                        )
                        .WithModifiers(
                            TokenList(
                                Token(SyntaxKind.PublicKeyword)
                                // 注意：这里没有 StaticKeyword，所以生成的是实例属性
                            )
                        )
                        .WithAccessorList(
                            AccessorList(
                                List(
                                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                                    AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                        .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
                                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                                )
                            )
                        )
                        .WithInitializer(
                            EqualsValueClause(
                                PostfixUnaryExpression(
                                    SyntaxKind.SuppressNullableWarningExpression,
                                    LiteralExpression(SyntaxKind.NullLiteralExpression)
                                )
                            )
                        )
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

                    // 如果有注释，添加前导注释
                    if (!string.IsNullOrEmpty(f.Comment)) {
                        var commentTrivia = f.Comment.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None)
                            .SelectMany(
                                line => new[] {
                                    Comment(line),
                                    EndOfLine("\n")
                                }
                            )
                            .ToArray();

                        property = property.WithLeadingTrivia(commentTrivia);
                    }

                    return property;
                }
            )
            .ToArray<MemberDeclarationSyntax>();
    }
    
    /*protected override StatementSyntax[] GenerateReturnStatements(List<(string name, string type, string comment)> filteredFieldDefinition) {
        return filteredFieldDefinition.Select(
                f => Block(
                    // 如果 name 为空，则设置 name
                    IfStatement(
                        BinaryExpression(
                            SyntaxKind.EqualsExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(f.name),
                                IdentifierName("name")
                            ),
                            LiteralExpression(SyntaxKind.NullLiteralExpression)
                        ),
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName(f.name),
                                    IdentifierName("name")
                                ),
                                ObjectCreationExpression(IdentifierName("ResourceLocation"))
                                    .WithArgumentList(
                                        ArgumentList(
                                            SeparatedList<ArgumentSyntax>(
                                                new[] {
                                                    Argument(
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName("name"),
                                                            IdentifierName("domain")
                                                        )
                                                    ),
                                                    Argument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal($"{f.name}")
                                                        )
                                                    )
                                                }
                                            )
                                        )
                                    )
                            )
                        )
                    ),
                    // 空值检查并返回
                    IfStatement(
                        BinaryExpression(
                            SyntaxKind.NotEqualsExpression,
                            IdentifierName(f.name),
                            LiteralExpression(SyntaxKind.NullLiteralExpression)
                        ),
                        YieldStatement(SyntaxKind.YieldReturnStatement)
                            .WithExpression(IdentifierName(f.name))
                    )
                )
            )
            .ToArray<StatementSyntax>();
    }*/

}
