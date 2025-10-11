using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static RegisterSystem.Generator.Util;

namespace RegisterSystem.Generator;

[Generator]
public class RegisterManageGenerator : IIncrementalGenerator {

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        RegisterSourceOutput(context);
    }

    protected virtual void RegisterSourceOutput(IncrementalGeneratorInitializationContext context) {
        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(
                context.SyntaxProvider
                    .CreateSyntaxProvider(
                        (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax,
                        (syntaxContext, _) => syntaxContext
                    )
                    .Collect()
            ),
            (sourceContext, compilationAndClasses) => {
                ImmutableArray<GeneratorSyntaxContext> generatorSyntaxContexts = compilationAndClasses.Right;

                foreach (GeneratorSyntaxContext generatorSyntaxContext in generatorSyntaxContexts) {
                    ProcessClass(sourceContext, generatorSyntaxContext);
                }
            }
        );
    }

    protected virtual void ProcessClass(SourceProductionContext sourceContext, GeneratorSyntaxContext generatorSyntaxContext) {
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

        if (setUpMethodDeclarationSyntax is null) {
            sourceContext.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.missingSetupMethod,
                    classDeclarationSyntax.GetLocation(),
                    classSymbol.Name
                )
            );
            return;
        }

        if (setUpMethodDeclarationSyntax.Body is null) {
            sourceContext.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.emptySetupMethodBody,
                    setUpMethodDeclarationSyntax.GetLocation(),
                    classSymbol.Name
                )
            );
            return;
        }

        GenerateCode(
            sourceContext,
            classSymbol,
            classDeclarationSyntax,
            setUpMethodDeclarationSyntax,
            semanticModel,
            constraintType
        );
    }

    protected virtual bool ShouldProcessClass(INamedTypeSymbol classSymbol) {
        return InheritsFromType(classSymbol, RegisterManageTypeName);
    }

    protected virtual ITypeSymbol? GetConstraintType(INamedTypeSymbol classSymbol) {
        return GetRegisterManageGenericConstraint(classSymbol);
    }

    protected virtual MethodDeclarationSyntax? FindSetupMethod(ClassDeclarationSyntax classDeclarationSyntax) {
        return classDeclarationSyntax.Members
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(method => method.Identifier.ValueText == SetUpMethodName && method.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)));
    }

    protected virtual string GetGeneratedMethodName() {
        return "getDefaultRegisterItem";
    }

    protected virtual string GetGeneratedMethodReturnType() {
        return "System.Collections.Generic.IEnumerable<(RegisterBasics registerBasics, string name)>";
    }

    protected virtual void GenerateCode
    (
        SourceProductionContext sourceContext,
        INamedTypeSymbol classSymbol,
        ClassDeclarationSyntax classDeclarationSyntax,
        MethodDeclarationSyntax setUpMethodDeclarationSyntax,
        SemanticModel semanticModel,
        ITypeSymbol? constraintType
    ) {
        List<FieldDefinition> fieldDefinition = GetFieldDefinitions(sourceContext, classSymbol, setUpMethodDeclarationSyntax, semanticModel, constraintType);

        // 验证现有字段并设置生成标识
        List<FieldDefinition> validFields = FilterExistingFields(sourceContext, classSymbol, classDeclarationSyntax, fieldDefinition);

        // 生成代码
        CompilationUnitSyntax compilationUnit = GenerateCompilationUnit(classSymbol, classDeclarationSyntax, validFields);

        SourceText sourceText = SourceText.From(compilationUnit.NormalizeWhitespace().ToFullString(), Encoding.UTF8);
        sourceContext.AddSource($"{classSymbol.Name}.g.cs", sourceText);
    }

    protected virtual List<FieldDefinition> GetFieldDefinitions(SourceProductionContext sourceContext, INamedTypeSymbol classSymbol, MethodDeclarationSyntax setUpMethodDeclarationSyntax, SemanticModel semanticModel, ITypeSymbol? constraintType) {
        return setUpMethodDeclarationSyntax.Body!.DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
            .Where(assignment => assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            .Select(
                assignment => {
                    // 分支1：处理 new 表达式赋值
                    if (assignment.Right is ObjectCreationExpressionSyntax newExpression) {
                        return ProcessNewExpressionAssignment(assignment, newExpression, sourceContext, classSymbol, semanticModel, constraintType);
                    }
                    
                    // 分支2：处理非 new 表达式赋值（如方法调用等）
                    return ProcessNonNewExpressionAssignment(assignment, sourceContext, classSymbol, semanticModel, constraintType);
                }
            )
            .Where(t => t != null && !string.IsNullOrEmpty(t.Name) && !string.IsNullOrEmpty(t.Type))
            .OfType<FieldDefinition>()
            .ToList();
    }

    /// <summary>
    /// 处理 new 表达式赋值
    /// </summary>
    protected virtual FieldDefinition? ProcessNewExpressionAssignment(AssignmentExpressionSyntax assignment, ObjectCreationExpressionSyntax newExpression, SourceProductionContext sourceContext, INamedTypeSymbol classSymbol, SemanticModel semanticModel, ITypeSymbol? constraintType) {
        // 获取右侧表达式的类型信息
        TypeInfo typeInfo = ModelExtensions.GetTypeInfo(semanticModel, newExpression);
        if (typeInfo.Type is not INamedTypeSymbol rightTypeSymbol) {
            return null;
        }

        // 检查是否符合类型约束
        if (!IsValidAssignmentType(
                assignment,
                semanticModel,
                constraintType,
                sourceContext,
                classSymbol,
                rightTypeSymbol
            )) {
            return null;
        }

        if (!IsFieldOrPropertyAccess(assignment.Left, assignment.Right, semanticModel, out (string name, string type) info)) {
            return null;
        }

        // 确保字段信息有效
        if (string.IsNullOrEmpty(info.name) || string.IsNullOrEmpty(info.type)) {
            return null;
        }

        // 获取赋值语句前的注释
        string comment = GetLeadingComments(assignment) ?? string.Empty;

        // 检查是否在条件语句中
        bool isNullable = IsWithinConditionalStatement(assignment);

        return new FieldDefinition(info.name, info.type, comment, isNullable, true);
    }

    /// <summary>
    /// 处理非 new 表达式赋值（如方法调用等）
    /// </summary>
    protected virtual FieldDefinition? ProcessNonNewExpressionAssignment(AssignmentExpressionSyntax assignment, SourceProductionContext sourceContext, INamedTypeSymbol classSymbol, SemanticModel semanticModel, ITypeSymbol? constraintType) {
        // 检查是否在对象初始化器中（排除这种情况）
        if (IsInsideObjectInitializer(assignment)) {
            return null;
        }

        // 获取左侧字段或属性名称
        if (assignment.Left is not IdentifierNameSyntax leftIdentifier) {
            return null;
        }

        string memberName = leftIdentifier.Identifier.ValueText;

        // 获取右侧表达式的类型信息
        TypeInfo rightTypeInfo = ModelExtensions.GetTypeInfo(semanticModel, assignment.Right);
        if (rightTypeInfo.Type is not INamedTypeSymbol rightTypeSymbol) {
            return null;
        }

        // 检查类中是否存在与左侧同名且类型匹配的属性/字段
        if (!HasMatchingProperty(classSymbol, memberName, rightTypeSymbol)) {
            return null;
        }

        // 检查右侧类型是否符合约束条件
        if (!IsValidAssignmentType(
                assignment,
                semanticModel,
                constraintType,
                sourceContext,
                classSymbol,
                rightTypeSymbol
            )) {
            return null;
        }

        // 获取赋值语句前的注释
        string comment = GetLeadingComments(assignment) ?? string.Empty;

        // 检查是否在条件语句中
        bool isNullable = IsWithinConditionalStatement(assignment);

        return new FieldDefinition(memberName, rightTypeSymbol.Name, comment, isNullable, false);
    }

    protected virtual bool IsValidAssignmentType
    (
        AssignmentExpressionSyntax assignment,
        SemanticModel semanticModel,
        ITypeSymbol? constraintType,
        SourceProductionContext sourceContext,
        INamedTypeSymbol classSymbol,
        INamedTypeSymbol rightTypeSymbol
    ) {
        if (constraintType != null) {
            // 有泛型约束时，检查是否继承自约束类型
            if (!IsNewExpressionOfConstraintType(assignment.Right, semanticModel, constraintType)) {
                // 报告类型不匹配错误
                if (assignment.Left is IdentifierNameSyntax leftIdentifier) {
                    sourceContext.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.wrongAssignmentType,
                            assignment.GetLocation(),
                            leftIdentifier.Identifier.ValueText,
                            classSymbol.Name,
                            rightTypeSymbol.Name,
                            constraintType.Name
                        )
                    );
                }
                return false;
            }
        }
        else {
            // 没有泛型约束时，检查是否继承自 RegisterBasics
            if (!InheritsFromType(rightTypeSymbol, RegisterBasicsTypeName)) {
                if (assignment.Left is IdentifierNameSyntax leftIdentifier) {
                    sourceContext.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.wrongAssignmentType,
                            assignment.GetLocation(),
                            leftIdentifier.Identifier.ValueText,
                            classSymbol.Name,
                            rightTypeSymbol.Name,
                            "RegisterBasics"
                        )
                    );
                }
                return false;
            }
        }
        return true;
    }

    protected virtual List<FieldDefinition> FilterExistingFields(SourceProductionContext sourceContext, INamedTypeSymbol classSymbol, ClassDeclarationSyntax classDeclarationSyntax, List<FieldDefinition> fieldDefinition) {
        List<FieldDefinition> validFields = new List<FieldDefinition>();

        foreach (FieldDefinition field in fieldDefinition) {
            if (HasExistingMember(classSymbol, field.Name)) {
                // 验证现有字段是否符合要求
                if (!ValidateExistingMember(classSymbol, field.Name, field.Type, out string actualType, out string errorReason)) {
                    // 获取字段在源代码中的位置
                    Location? fieldLocation = classDeclarationSyntax.Members
                        .FirstOrDefault(
                            m => m is PropertyDeclarationSyntax prop && prop.Identifier.ValueText == field.Name ||
                                 m is FieldDeclarationSyntax fieldDecl && fieldDecl.Declaration.Variables.Any(v => v.Identifier.ValueText == field.Name)
                        )
                        ?.GetLocation() ?? classDeclarationSyntax.GetLocation();

                    sourceContext.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.fieldTypeMismatch,
                            fieldLocation,
                            field.Name,
                            classSymbol.Name,
                            actualType,
                            field.Type
                        )
                    );
                }
                else {
                    // 字段已存在且验证通过，不需要生成属性，但要包含在返回方法中
                    validFields.Add(field with { NeedsGeneration = false });
                }
            }
            else {
                // 字段不存在，需要生成属性，也要包含在返回方法中
                validFields.Add(field);
            }
        }

        return validFields;
    }

    protected virtual CompilationUnitSyntax GenerateCompilationUnit(INamedTypeSymbol classSymbol, ClassDeclarationSyntax classDeclarationSyntax, List<FieldDefinition> validFields) {
        return CompilationUnit()
            .WithUsings(List(GetUsingDirectives(classDeclarationSyntax).ToArray()))
            .WithMembers(
                List<MemberDeclarationSyntax>(
                    NamespaceDeclaration(IdentifierName(classSymbol.ContainingNamespace.ToDisplayString()))
                        .AddMembers(
                            ClassDeclaration(classSymbol.Name)
                                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword)))
                                .WithMembers(List<MemberDeclarationSyntax>())
                                .AddMembers(GenerateProperties(validFields.Where(f => f.NeedsGeneration).ToList()))
                                .AddMembers(GenerateMethod(validFields))
                        )
                )
            )
            .WithLeadingTrivia(
                TriviaList(
                    Trivia(
                        NullableDirectiveTrivia(
                            Token(SyntaxKind.EnableKeyword),
                            true
                        )
                    ),
                    EndOfLine("\n")
                )
            );
    }

    protected virtual MemberDeclarationSyntax[] GenerateProperties(List<FieldDefinition> filteredFieldDefinition) {
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
                                Token(SyntaxKind.PublicKeyword),
                                Token(SyntaxKind.StaticKeyword)
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

    protected virtual MethodDeclarationSyntax GenerateMethod(List<FieldDefinition> filteredFieldDefinition) {
        return MethodDeclaration(
                ParseTypeName(GetGeneratedMethodReturnType()),
                Identifier(GetGeneratedMethodName())
            )
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.OverrideKeyword)
                )
            )
            .WithParameterList(ParameterList())
            .AddBodyStatements(GenerateBaseCall())
            .AddBodyStatements(GenerateReturnStatements(filteredFieldDefinition));
    }

    protected virtual StatementSyntax GenerateBaseCall() {
        return ForEachStatement(
            IdentifierName("var"),
            Identifier("item"),
            InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        BaseExpression(),
                        IdentifierName(GetGeneratedMethodName())
                    )
                )
                .WithArgumentList(ArgumentList()),
            Block(
                YieldStatement(SyntaxKind.YieldReturnStatement)
                    .WithExpression(IdentifierName("item"))
            )
        );
    }

    protected virtual StatementSyntax[] GenerateReturnStatements(List<FieldDefinition> filteredFieldDefinition) {
        return filteredFieldDefinition.Select(
                f =>
                    IfStatement(
                        BinaryExpression(
                            SyntaxKind.NotEqualsExpression,
                            IdentifierName(f.Name),
                            LiteralExpression(SyntaxKind.NullLiteralExpression)
                        ),
                        Block(
                            YieldStatement(SyntaxKind.YieldReturnStatement)
                                .WithExpression(GenerateReturnExpression(f))
                        )
                    )
            )
            .ToArray<StatementSyntax>();
    }

    protected virtual ExpressionSyntax GenerateReturnExpression(FieldDefinition field) {
        return TupleExpression(
            SeparatedList<ArgumentSyntax>(
                new[] {
                    Argument(IdentifierName(field.Name)),
                    Argument(
                        LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            Literal(field.Name)
                        )
                    )
                }
            )
        );
    }

}
