using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ArcaneWorld.Generators;

[Generator]
public class AutoSerializeGenerator : IIncrementalGenerator {

    private const string IAutoSerializeTypeName = "global::ArcaneWorld.Capacity.IAutoSerialize";

    private const string SaveFieldAttributeName = "global::ArcaneWorld.Attribute.SaveField";

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsClassDeclaration(node),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx)
            )
            .Where(static m => m is not null);

        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(
            compilationAndClasses,
            static (spc, source) => Execute(source.Left, source.Right, spc)
        );
    }

    private static bool IsClassDeclaration(SyntaxNode node) {
        return node is ClassDeclarationSyntax classDecl &&
               classDecl.BaseList != null;
    }

    private static ClassInfo? GetSemanticTargetForGeneration(GeneratorSyntaxContext context) {
        if (context.Node is not ClassDeclarationSyntax classDeclaration) {
            return null;
        }

        var semanticModel = context.SemanticModel;
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
        if (classSymbol == null) {
            return null;
        }

        // 检查是否实现了 IAutoSerialize 接口
        bool implementsIAutoSerialize = classSymbol.AllInterfaces
            .Any(i => i.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Equals(IAutoSerializeTypeName));

        if (!implementsIAutoSerialize) {
            return null;
        }

        // 检查基类是否也实现了 IAutoSerialize
        bool baseImplementsIAutoSerialize = false;
        if (classSymbol.BaseType != null && classSymbol.BaseType.SpecialType != SpecialType.System_Object) {
            baseImplementsIAutoSerialize = classSymbol.BaseType.AllInterfaces
                .Any(i => i.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Equals(IAutoSerializeTypeName));
        }

        // 收集泛型类型参数信息
        var typeParameters = new List<TypeParameterInfo>();
        if (classSymbol.IsGenericType) {
            foreach (var typeParam in classSymbol.TypeParameters) {
                typeParameters.Add(
                    new TypeParameterInfo(
                        typeParam.Name,
                        typeParam.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    )
                );
            }
        }

        // 只收集当前类中标记了 SaveField 属性的字段和属性
        var saveFields = new List<MemberInfo>();

        // 检查字段
        foreach (var field in classSymbol.GetMembers().OfType<IFieldSymbol>()) {
            if (HasSaveFieldAttribute(field)) {
                saveFields.Add(
                    new MemberInfo(
                        field.Name,
                        field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        MemberType.Field,
                        field.Type
                    )
                );
            }
        }

        // 检查属性
        foreach (var property in classSymbol.GetMembers().OfType<IPropertySymbol>()) {
            if (HasSaveFieldAttribute(property)) {
                saveFields.Add(
                    new MemberInfo(
                        property.Name,
                        property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        MemberType.Property,
                        property.Type
                    )
                );
            }
        }

        return new ClassInfo(
            classSymbol.Name,
            classSymbol.ContainingNamespace.ToDisplayString(),
            saveFields.ToImmutableArray(),
            classDeclaration,
            baseImplementsIAutoSerialize,
            typeParameters.ToImmutableArray()
        );
    }

    private static bool HasSaveFieldAttribute(ISymbol symbol) {
        return symbol.GetAttributes()
            .Any(attr => attr.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == SaveFieldAttributeName);
    }

    private static bool ImplementsISerialize(ITypeSymbol typeSymbol) {
        return typeSymbol.AllInterfaces
            .Any(
                i => i.IsGenericType &&
                     i.ConstructedFrom.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::ArcaneWorld.Capacity.ISerialize<J>"
            );
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassInfo?> classes, SourceProductionContext context) {
        if (classes.IsDefaultOrEmpty)
            return;

        foreach (var classInfo in classes) {
            if (classInfo == null)
                continue;

            var compilationUnit = GenerateCompilationUnit(classInfo);
            var sourceText = SourceText.From(compilationUnit.NormalizeWhitespace().ToFullString(), Encoding.UTF8);
            context.AddSource($"{classInfo.Name}.AutoSerialize.g.cs", sourceText);
        }
    }

    private static CompilationUnitSyntax GenerateCompilationUnit(ClassInfo classInfo) {
        var usings = new List<UsingDirectiveSyntax>();

        // 添加原始类文件中的 using 语句
        if (classInfo.OriginalClass.SyntaxTree.GetRoot() is CompilationUnitSyntax originalCompilationUnit) {
            usings.AddRange(originalCompilationUnit.Usings);
        }

        var namespaceDeclaration = NamespaceDeclaration(
                ParseName(classInfo.Namespace)
            )
            .AddMembers(GeneratePartialClass(classInfo));

        return CompilationUnit()
            .WithUsings(List(usings.Distinct().ToArray()))
            .WithMembers(SingletonList<MemberDeclarationSyntax>(namespaceDeclaration))
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

    private static ClassDeclarationSyntax GeneratePartialClass(ClassInfo classInfo) {
        var classDeclaration = ClassDeclaration(classInfo.Name)
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.PartialKeyword)
                )
            );

        // 如果有泛型参数，添加类型参数列表
        if (classInfo.TypeParameters.Length > 0) {
            var typeParameterList = TypeParameterList(
                SeparatedList(
                    classInfo.TypeParameters.Select(
                        tp =>
                            TypeParameter(Identifier(tp.Name))
                    )
                )
            );
            classDeclaration = classDeclaration.WithTypeParameterList(typeParameterList);
        }

        var members = new List<MemberDeclarationSyntax>();

        // 只在最基类生成 serialize/deserialize 方法
        if (!classInfo.BaseImplementsIAutoSerialize) {
            members.Add(GenerateSerializeMethod(classInfo));
            members.Add(GenerateDeserializeMethod(classInfo));
        }

        // 所有类都生成 _serialize/_deserialize 方法
        members.Add(GeneratePrivateSerializeMethod(classInfo));
        members.Add(GeneratePrivateDeserializeMethod(classInfo));

        // 只在最顶层基类生成分部方法声明
        if (!classInfo.BaseImplementsIAutoSerialize) {
            members.AddRange(GeneratePartialMethodDeclarations());
        }

        return classDeclaration.WithMembers(List(members));
    }

    private static MethodDeclarationSyntax GenerateSerializeMethod(ClassInfo classInfo) {
        var statements = new List<StatementSyntax>();

        // 使用 using 语句获取写锁
        statements.Add(
            UsingStatement(
                null,
                InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression(),
                            IdentifierName("lockForWrite")
                        )
                    )
                    .WithArgumentList(ArgumentList()),
                Block(
                    // 只在最顶层基类调用 onBeforeSerialize 回调
                    !classInfo.BaseImplementsIAutoSerialize
                        ? ExpressionStatement(
                            InvocationExpression(
                                    IdentifierName("onBeforeSerialize")
                                )
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList(
                                            Argument(IdentifierName("jsonSerializerOptions"))
                                        )
                                    )
                                )
                        )
                        : (StatementSyntax)EmptyStatement(),
                    LocalDeclarationStatement(
                        VariableDeclaration(
                                IdentifierName("var")
                            )
                            .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator(
                                            Identifier("result")
                                        )
                                        .WithInitializer(
                                            EqualsValueClause(
                                                InvocationExpression(
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            ThisExpression(),
                                                            IdentifierName("_serialize")
                                                        )
                                                    )
                                                    .WithArgumentList(
                                                        ArgumentList(
                                                            SingletonSeparatedList(
                                                                Argument(IdentifierName("jsonSerializerOptions"))
                                                            )
                                                        )
                                                    )
                                            )
                                        )
                                )
                            )
                    ),
                    // 只在最顶层基类调用 onAfterSerialize 回调
                    !classInfo.BaseImplementsIAutoSerialize
                        ? ExpressionStatement(
                            InvocationExpression(
                                    IdentifierName("onAfterSerialize")
                                )
                                .WithArgumentList(
                                    ArgumentList(
                                        SeparatedList<ArgumentSyntax>(
                                            new[] {
                                                Argument(IdentifierName("result")),
                                                Argument(IdentifierName("jsonSerializerOptions"))
                                            }
                                        )
                                    )
                                )
                        )
                        : (StatementSyntax)EmptyStatement(),
                    ReturnStatement(IdentifierName("result"))
                )
            )
        );

        return MethodDeclaration(
                QualifiedName(
                    QualifiedName(
                        QualifiedName(
                            QualifiedName(
                                IdentifierName("System"),
                                IdentifierName("Text")
                            ),
                            IdentifierName("Json")
                        ),
                        IdentifierName("Nodes")
                    ),
                    IdentifierName("JsonObject")
                ),
                Identifier("serialize")
            )
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PublicKeyword),
                    Token(
                        classInfo.BaseImplementsIAutoSerialize
                            ? SyntaxKind.OverrideKeyword
                            : SyntaxKind.VirtualKeyword
                    )
                )
            )
            .WithParameterList(
                ParameterList(
                    SingletonSeparatedList(
                        Parameter(
                                Identifier("jsonSerializerOptions")
                            )
                            .WithType(
                                QualifiedName(
                                    QualifiedName(
                                        QualifiedName(
                                            IdentifierName("System"),
                                            IdentifierName("Text")
                                        ),
                                        IdentifierName("Json")
                                    ),
                                    IdentifierName("JsonSerializerOptions")
                                )
                            )
                    )
                )
            )
            .WithBody(
                Block(statements)
            );
    }

    private static MethodDeclarationSyntax GenerateDeserializeMethod(ClassInfo classInfo) {
        var statements = new List<StatementSyntax>();

        // 使用 using 语句获取写锁
        statements.Add(
            UsingStatement(
                null,
                InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression(),
                            IdentifierName("lockForWrite")
                        )
                    )
                    .WithArgumentList(ArgumentList()),
                Block(
                    // 只在最顶层基类调用 onBeforeDeserialize 回调
                    !classInfo.BaseImplementsIAutoSerialize
                        ? ExpressionStatement(
                            InvocationExpression(
                                    IdentifierName("onBeforeDeserialize")
                                )
                                .WithArgumentList(
                                    ArgumentList(
                                        SeparatedList<ArgumentSyntax>(
                                            new[] {
                                                Argument(IdentifierName("data")),
                                                Argument(IdentifierName("jsonSerializerOptions"))
                                            }
                                        )
                                    )
                                )
                        )
                        : (StatementSyntax)EmptyStatement(),
                    ExpressionStatement(
                        InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression(),
                                    IdentifierName("_deserialize")
                                )
                            )
                            .WithArgumentList(
                                ArgumentList(
                                    SeparatedList<ArgumentSyntax>(
                                        new[] {
                                            Argument(IdentifierName("data")),
                                            Argument(IdentifierName("jsonSerializerOptions"))
                                        }
                                    )
                                )
                            )
                    ),
                    // 只在最顶层基类调用 onAfterDeserialize 回调
                    !classInfo.BaseImplementsIAutoSerialize
                        ? ExpressionStatement(
                            InvocationExpression(
                                    IdentifierName("onAfterDeserialize")
                                )
                                .WithArgumentList(
                                    ArgumentList(
                                        SeparatedList<ArgumentSyntax>(
                                            new[] {
                                                Argument(IdentifierName("data")),
                                                Argument(IdentifierName("jsonSerializerOptions"))
                                            }
                                        )
                                    )
                                )
                        )
                        : (StatementSyntax)EmptyStatement()
                )
            )
        );

        return MethodDeclaration(
                PredefinedType(Token(SyntaxKind.VoidKeyword)),
                Identifier("deserialize")
            )
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PublicKeyword),
                    Token(
                        classInfo.BaseImplementsIAutoSerialize
                            ? SyntaxKind.OverrideKeyword
                            : SyntaxKind.VirtualKeyword
                    )
                )
            )
            .WithParameterList(
                ParameterList(
                    SeparatedList<ParameterSyntax>(
                        new[] {
                            Parameter(
                                    Identifier("data")
                                )
                                .WithType(
                                    QualifiedName(
                                        QualifiedName(
                                            QualifiedName(
                                                QualifiedName(
                                                    IdentifierName("System"),
                                                    IdentifierName("Text")
                                                ),
                                                IdentifierName("Json")
                                            ),
                                            IdentifierName("Nodes")
                                        ),
                                        IdentifierName("JsonObject")
                                    )
                                ),
                            Parameter(
                                    Identifier("jsonSerializerOptions")
                                )
                                .WithType(
                                    QualifiedName(
                                        QualifiedName(
                                            QualifiedName(
                                                IdentifierName("System"),
                                                IdentifierName("Text")
                                            ),
                                            IdentifierName("Json")
                                        ),
                                        IdentifierName("JsonSerializerOptions")
                                    )
                                )
                        }
                    )
                )
            )
            .WithBody(
                Block(statements)
            );
    }

    private static MethodDeclarationSyntax GeneratePrivateSerializeMethod(ClassInfo classInfo) {
        var statements = new List<StatementSyntax>();

        // 如果基类也实现了 IAutoSerialize，先调用 base._serialize
        if (classInfo.BaseImplementsIAutoSerialize) {
            statements.Add(
                LocalDeclarationStatement(
                    VariableDeclaration(
                            IdentifierName("var")
                        )
                        .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator(
                                        Identifier("jObject")
                                    )
                                    .WithInitializer(
                                        EqualsValueClause(
                                            InvocationExpression(
                                                    MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        BaseExpression(),
                                                        IdentifierName("_serialize")
                                                    )
                                                )
                                                .WithArgumentList(
                                                    ArgumentList(
                                                        SingletonSeparatedList(
                                                            Argument(IdentifierName("jsonSerializerOptions"))
                                                        )
                                                    )
                                                )
                                        )
                                    )
                            )
                        )
                )
            );
        }
        else {
            statements.Add(
                LocalDeclarationStatement(
                    VariableDeclaration(
                            IdentifierName("var")
                        )
                        .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator(
                                        Identifier("jObject")
                                    )
                                    .WithInitializer(
                                        EqualsValueClause(
                                            ObjectCreationExpression(
                                                    QualifiedName(
                                                        QualifiedName(
                                                            QualifiedName(
                                                                QualifiedName(
                                                                    IdentifierName("System"),
                                                                    IdentifierName("Text")
                                                                ),
                                                                IdentifierName("Json")
                                                            ),
                                                            IdentifierName("Nodes")
                                                        ),
                                                        IdentifierName("JsonObject")
                                                    )
                                                )
                                                .WithArgumentList(ArgumentList())
                                        )
                                    )
                            )
                        )
                )
            );
        }

        // 为每个 SaveField 字段/属性添加序列化语句
        foreach (var member in classInfo.SaveFields) {
            // 检查字段类型是否实现了 ISerialize
            if (ImplementsISerialize(member.TypeSymbol)) {
                // 调用字段的 serialize 方法
                statements.Add(
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            ElementAccessExpression(
                                    IdentifierName("jObject")
                                )
                                .WithArgumentList(
                                    BracketedArgumentList(
                                        SingletonSeparatedList(
                                            Argument(
                                                LiteralExpression(
                                                    SyntaxKind.StringLiteralExpression,
                                                    Literal(member.Name)
                                                )
                                            )
                                        )
                                    )
                                ),
                            InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName(member.Name),
                                        IdentifierName("serialize")
                                    )
                                )
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList(
                                            Argument(IdentifierName("jsonSerializerOptions"))
                                        )
                                    )
                                )
                        )
                    )
                );
            }
            else {
                // 使用 JsonSerializer.SerializeToNode
                statements.Add(
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            ElementAccessExpression(
                                    IdentifierName("jObject")
                                )
                                .WithArgumentList(
                                    BracketedArgumentList(
                                        SingletonSeparatedList(
                                            Argument(
                                                LiteralExpression(
                                                    SyntaxKind.StringLiteralExpression,
                                                    Literal(member.Name)
                                                )
                                            )
                                        )
                                    )
                                ),
                            InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        QualifiedName(
                                            QualifiedName(
                                                QualifiedName(
                                                    IdentifierName("System"),
                                                    IdentifierName("Text")
                                                ),
                                                IdentifierName("Json")
                                            ),
                                            IdentifierName("JsonSerializer")
                                        ),
                                        IdentifierName("SerializeToNode")
                                    )
                                )
                                .WithArgumentList(
                                    ArgumentList(
                                        SeparatedList<ArgumentSyntax>(
                                            new[] {
                                                Argument(IdentifierName(member.Name)),
                                                Argument(IdentifierName("jsonSerializerOptions"))
                                            }
                                        )
                                    )
                                )
                        )
                    )
                );
            }
        }


        statements.Add(
            ReturnStatement(
                IdentifierName("jObject")
            )
        );

        return MethodDeclaration(
                QualifiedName(
                    QualifiedName(
                        QualifiedName(
                            QualifiedName(
                                IdentifierName("System"),
                                IdentifierName("Text")
                            ),
                            IdentifierName("Json")
                        ),
                        IdentifierName("Nodes")
                    ),
                    IdentifierName("JsonObject")
                ),
                Identifier("_serialize")
            )
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.ProtectedKeyword),
                    Token(
                        classInfo.BaseImplementsIAutoSerialize
                            ? SyntaxKind.OverrideKeyword
                            : SyntaxKind.VirtualKeyword
                    )
                )
            )
            .WithParameterList(
                ParameterList(
                    SingletonSeparatedList(
                        Parameter(
                                Identifier("jsonSerializerOptions")
                            )
                            .WithType(
                                QualifiedName(
                                    QualifiedName(
                                        QualifiedName(
                                            IdentifierName("System"),
                                            IdentifierName("Text")
                                        ),
                                        IdentifierName("Json")
                                    ),
                                    IdentifierName("JsonSerializerOptions")
                                )
                            )
                    )
                )
            )
            .WithBody(
                Block(statements)
            );
    }

    private static MethodDeclarationSyntax GeneratePrivateDeserializeMethod(ClassInfo classInfo) {
        var statements = new List<StatementSyntax>();

        // 如果基类也实现了 IAutoSerialize，先调用 base._deserialize
        if (classInfo.BaseImplementsIAutoSerialize) {
            statements.Add(
                ExpressionStatement(
                    InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                BaseExpression(),
                                IdentifierName("_deserialize")
                            )
                        )
                        .WithArgumentList(
                            ArgumentList(
                                SeparatedList<ArgumentSyntax>(
                                    new[] {
                                        Argument(IdentifierName("data")),
                                        Argument(IdentifierName("jsonSerializerOptions"))
                                    }
                                )
                            )
                        )
                )
            );
        }

        foreach (var member in classInfo.SaveFields) {
            var variableName = $"{member.Name}Node";

            statements.Add(
                IfStatement(
                    InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("data"),
                                IdentifierName("TryGetPropertyValue")
                            )
                        )
                        .WithArgumentList(
                            ArgumentList(
                                SeparatedList<ArgumentSyntax>(
                                    new[] {
                                        Argument(
                                            LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                Literal(member.Name)
                                            )
                                        ),
                                        Argument(
                                                DeclarationExpression(
                                                    IdentifierName("var"),
                                                    SingleVariableDesignation(
                                                        Identifier(variableName)
                                                    )
                                                )
                                            )
                                            .WithRefKindKeyword(Token(SyntaxKind.OutKeyword))
                                    }
                                )
                            )
                        ),
                    Block(
                        // 检查字段类型是否实现了 ISerialize
                        ImplementsISerialize(member.TypeSymbol)
                            ?
                            // 调用字段的 deserialize 方法，使用 is 模式匹配进行类型检查
                            IfStatement(
                                IsPatternExpression(
                                    IdentifierName(variableName),
                                    DeclarationPattern(
                                        QualifiedName(
                                            QualifiedName(
                                                QualifiedName(
                                                    QualifiedName(
                                                        IdentifierName("System"),
                                                        IdentifierName("Text")
                                                    ),
                                                    IdentifierName("Json")
                                                ),
                                                IdentifierName("Nodes")
                                            ),
                                            IdentifierName("JsonObject")
                                        ),
                                        SingleVariableDesignation(
                                            Identifier($"{variableName}Obj")
                                        )
                                    )
                                ),
                                Block(
                                    ExpressionStatement(
                                        InvocationExpression(
                                                MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    IdentifierName(member.Name),
                                                    IdentifierName("deserialize")
                                                )
                                            )
                                            .WithArgumentList(
                                                ArgumentList(
                                                    SeparatedList<ArgumentSyntax>(
                                                        new[] {
                                                            Argument(IdentifierName($"{variableName}Obj")),
                                                            Argument(IdentifierName("jsonSerializerOptions"))
                                                        }
                                                    )
                                                )
                                            )
                                    )
                                )
                            )
                            :
                            // 使用 JsonSerializer.Deserialize
                            ExpressionStatement(
                                AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    IdentifierName(member.Name),
                                    PostfixUnaryExpression(
                                        SyntaxKind.SuppressNullableWarningExpression,
                                        InvocationExpression(
                                                MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    QualifiedName(
                                                        QualifiedName(
                                                            QualifiedName(
                                                                IdentifierName("System"),
                                                                IdentifierName("Text")
                                                            ),
                                                            IdentifierName("Json")
                                                        ),
                                                        IdentifierName("JsonSerializer")
                                                    ),
                                                    GenericName(
                                                            Identifier("Deserialize")
                                                        )
                                                        .WithTypeArgumentList(
                                                            TypeArgumentList(
                                                                SingletonSeparatedList<TypeSyntax>(
                                                                    ParseTypeName(member.TypeName)
                                                                )
                                                            )
                                                        )
                                                )
                                            )
                                            .WithArgumentList(
                                                ArgumentList(
                                                    SeparatedList<ArgumentSyntax>(
                                                        new[] {
                                                            Argument(IdentifierName(variableName)),
                                                            Argument(IdentifierName("jsonSerializerOptions"))
                                                        }
                                                    )
                                                )
                                            )
                                    )
                                )
                            )
                    )
                )
            );
        }


        return MethodDeclaration(
                PredefinedType(Token(SyntaxKind.VoidKeyword)),
                Identifier("_deserialize")
            )
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.ProtectedKeyword),
                    Token(
                        classInfo.BaseImplementsIAutoSerialize
                            ? SyntaxKind.OverrideKeyword
                            : SyntaxKind.VirtualKeyword
                    )
                )
            )
            .WithParameterList(
                ParameterList(
                    SeparatedList<ParameterSyntax>(
                        new[] {
                            Parameter(
                                    Identifier("data")
                                )
                                .WithType(
                                    QualifiedName(
                                        QualifiedName(
                                            QualifiedName(
                                                QualifiedName(
                                                    IdentifierName("System"),
                                                    IdentifierName("Text")
                                                ),
                                                IdentifierName("Json")
                                            ),
                                            IdentifierName("Nodes")
                                        ),
                                        IdentifierName("JsonObject")
                                    )
                                ),
                            Parameter(
                                    Identifier("jsonSerializerOptions")
                                )
                                .WithType(
                                    QualifiedName(
                                        QualifiedName(
                                            QualifiedName(
                                                IdentifierName("System"),
                                                IdentifierName("Text")
                                            ),
                                            IdentifierName("Json")
                                        ),
                                        IdentifierName("JsonSerializerOptions")
                                    )
                                )
                        }
                    )
                )
            )
            .WithBody(
                Block(statements)
            );
    }

    private static IEnumerable<MethodDeclarationSyntax> GeneratePartialMethodDeclarations() {
        // 生成分部方法声明
        var onBeforeSerialize = MethodDeclaration(
                PredefinedType(Token(SyntaxKind.VoidKeyword)),
                Identifier("onBeforeSerialize")
            )
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.ProtectedKeyword),
                    Token(SyntaxKind.PartialKeyword)
                )
            )
            .WithParameterList(
                ParameterList(
                    SingletonSeparatedList(
                        Parameter(
                                Identifier("jsonSerializerOptions")
                            )
                            .WithType(
                                QualifiedName(
                                    QualifiedName(
                                        QualifiedName(
                                            IdentifierName("System"),
                                            IdentifierName("Text")
                                        ),
                                        IdentifierName("Json")
                                    ),
                                    IdentifierName("JsonSerializerOptions")
                                )
                            )
                    )
                )
            )
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        var onAfterSerialize = MethodDeclaration(
                PredefinedType(Token(SyntaxKind.VoidKeyword)),
                Identifier("onAfterSerialize")
            )
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.ProtectedKeyword),
                    Token(SyntaxKind.PartialKeyword)
                )
            )
            .WithParameterList(
                ParameterList(
                    SeparatedList<ParameterSyntax>(
                        new[] {
                            Parameter(
                                    Identifier("jObject")
                                )
                                .WithType(
                                    QualifiedName(
                                        QualifiedName(
                                            QualifiedName(
                                                QualifiedName(
                                                    IdentifierName("System"),
                                                    IdentifierName("Text")
                                                ),
                                                IdentifierName("Json")
                                            ),
                                            IdentifierName("Nodes")
                                        ),
                                        IdentifierName("JsonObject")
                                    )
                                ),
                            Parameter(
                                    Identifier("jsonSerializerOptions")
                                )
                                .WithType(
                                    QualifiedName(
                                        QualifiedName(
                                            QualifiedName(
                                                IdentifierName("System"),
                                                IdentifierName("Text")
                                            ),
                                            IdentifierName("Json")
                                        ),
                                        IdentifierName("JsonSerializerOptions")
                                    )
                                )
                        }
                    )
                )
            )
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        var onBeforeDeserialize = MethodDeclaration(
                PredefinedType(Token(SyntaxKind.VoidKeyword)),
                Identifier("onBeforeDeserialize")
            )
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.ProtectedKeyword),
                    Token(SyntaxKind.PartialKeyword)
                )
            )
            .WithParameterList(
                ParameterList(
                    SeparatedList<ParameterSyntax>(
                        new[] {
                            Parameter(
                                    Identifier("data")
                                )
                                .WithType(
                                    QualifiedName(
                                        QualifiedName(
                                            QualifiedName(
                                                QualifiedName(
                                                    IdentifierName("System"),
                                                    IdentifierName("Text")
                                                ),
                                                IdentifierName("Json")
                                            ),
                                            IdentifierName("Nodes")
                                        ),
                                        IdentifierName("JsonObject")
                                    )
                                ),
                            Parameter(
                                    Identifier("jsonSerializerOptions")
                                )
                                .WithType(
                                    QualifiedName(
                                        QualifiedName(
                                            QualifiedName(
                                                IdentifierName("System"),
                                                IdentifierName("Text")
                                            ),
                                            IdentifierName("Json")
                                        ),
                                        IdentifierName("JsonSerializerOptions")
                                    )
                                )
                        }
                    )
                )
            )
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        var onAfterDeserialize = MethodDeclaration(
                PredefinedType(Token(SyntaxKind.VoidKeyword)),
                Identifier("onAfterDeserialize")
            )
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.ProtectedKeyword),
                    Token(SyntaxKind.PartialKeyword)
                )
            )
            .WithParameterList(
                ParameterList(
                    SeparatedList<ParameterSyntax>(
                        new[] {
                            Parameter(
                                    Identifier("data")
                                )
                                .WithType(
                                    QualifiedName(
                                        QualifiedName(
                                            QualifiedName(
                                                QualifiedName(
                                                    IdentifierName("System"),
                                                    IdentifierName("Text")
                                                ),
                                                IdentifierName("Json")
                                            ),
                                            IdentifierName("Nodes")
                                        ),
                                        IdentifierName("JsonObject")
                                    )
                                ),
                            Parameter(
                                    Identifier("jsonSerializerOptions")
                                )
                                .WithType(
                                    QualifiedName(
                                        QualifiedName(
                                            QualifiedName(
                                                IdentifierName("System"),
                                                IdentifierName("Text")
                                            ),
                                            IdentifierName("Json")
                                        ),
                                        IdentifierName("JsonSerializerOptions")
                                    )
                                )
                        }
                    )
                )
            )
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        return new[] { onBeforeSerialize, onAfterSerialize, onBeforeDeserialize, onAfterDeserialize };
    }

    private record ClassInfo
    (
        string Name,
        string Namespace,
        ImmutableArray<MemberInfo> SaveFields,
        ClassDeclarationSyntax OriginalClass,
        bool BaseImplementsIAutoSerialize,
        ImmutableArray<TypeParameterInfo> TypeParameters
    );

    private record MemberInfo(string Name, string TypeName, MemberType MemberType, ITypeSymbol TypeSymbol);

    private record TypeParameterInfo(string Name, string FullName);

    private enum MemberType {

        Field, Property

    }

}
