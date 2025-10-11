using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RegisterSystem.Generator;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RegisterSetupMethodCodeFixProvider)), Shared]
public class RegisterSetupMethodCodeFixProvider : CodeFixProvider {

    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.missingSetupMethod.Id);

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var diagnostic = context.Diagnostics.FirstOrDefault(d => FixableDiagnosticIds.Contains(d.Id));
        if (diagnostic == null) return;

        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // 查找包含诊断位置的类声明
        var classDeclaration = root?.FindToken(diagnosticSpan.Start)
            .Parent?.AncestorsAndSelf()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();

        if (classDeclaration == null) return;

        // 注册代码修复
        var action = CodeAction.Create(
            title: "添加 override setup 方法",
            createChangedDocument: c => AddSetupMethod(context.Document, classDeclaration, c),
            equivalenceKey: "AddSetupMethod");

        context.RegisterCodeFix(action, diagnostic);
    }

    private static async Task<Document> AddSetupMethod(Document document, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken) {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null) return document;

        // 创建 setup 方法
        var setupMethod = MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), "setup")
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.OverrideKeyword)
                )
            )
            .WithBody(
                Block(
                    ExpressionStatement(
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                BaseExpression(),
                                IdentifierName("setup")
                            )
                        )
                    )
                )
            )
            .WithLeadingTrivia(
                TriviaList(
                    EndOfLine("\n"),
                    Whitespace("    ")
                )
            )
            .WithTrailingTrivia(
                TriviaList(
                    EndOfLine("\n")
                )
            );

        // 找到插入位置（在类的最后一个成员之后，或者在类的开始处）
        ClassDeclarationSyntax newClassDeclaration;
        if (classDeclaration.Members.Any()) {
            // 在最后一个成员之后插入
            newClassDeclaration = classDeclaration.AddMembers(setupMethod);
        } else {
            // 如果类中没有成员，直接添加
            newClassDeclaration = classDeclaration.AddMembers(setupMethod);
        }

        // 替换旧的类声明
        var newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);
        return document.WithSyntaxRoot(newRoot);
    }
}
