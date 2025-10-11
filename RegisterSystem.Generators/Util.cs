using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RegisterSystem.Generator;

/// <summary>
/// 字段定义信息
/// </summary>
public record FieldDefinition(string Name, string Type, string Comment, bool IsNullable, bool NeedsGeneration);

public class Util {

    public const string RegisterManageTypeName = "RegisterSystem.RegisterManage";

    public const string RegisterBasicsTypeName = "RegisterSystem.RegisterBasics";

    public const string SetUpMethodName = "setup";

    /// <summary>
    /// 检查类型是否继承自指定基类
    /// </summary>
    public static bool InheritsFromType(INamedTypeSymbol typeSymbol, string baseTypeName) {
        INamedTypeSymbol? currentType = typeSymbol.BaseType;
        while (currentType != null) {
            string fullName = currentType.ToDisplayString();
            if (fullName == baseTypeName) {
                return true;
            }
            currentType = currentType.BaseType;
        }
        return false;
    }
    
    
    /// <summary>
    /// 检查类是否声明为 partial
    /// </summary>
    public static bool IsPartialClass(ClassDeclarationSyntax classDeclaration) {
        return classDeclaration.Modifiers
            .Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword));
    }

    /// <summary>
    /// 获取类继承的 RegisterManage<T> 中的泛型参数 T
    /// </summary>
    public static ITypeSymbol? GetRegisterManageGenericConstraint(INamedTypeSymbol classSymbol) {
        INamedTypeSymbol? currentType = classSymbol.BaseType;
        while (currentType != null) {
            // 检查是否为泛型类型且未绑定名称匹配 RegisterManage
            if (currentType.IsGenericType && 
                currentType.OriginalDefinition.ToDisplayString() == "RegisterSystem.RegisterManage<T>") {
                // 返回第一个泛型参数 T
                return currentType.TypeArguments.FirstOrDefault();
            }
            currentType = currentType.BaseType;
        }
        return null;
    }

    /// <summary>
    /// 检查表达式是否为创建继承自指定约束类型的 new 表达式
    /// </summary>
    public static bool IsNewExpressionOfConstraintType(SyntaxNode expression, SemanticModel semanticModel, ITypeSymbol? constraintType) {
        if (expression is not ObjectCreationExpressionSyntax newExpression) {
            return false;
        }

        TypeInfo typeInfo = ModelExtensions.GetTypeInfo(semanticModel, newExpression);
        if (typeInfo.Type is not INamedTypeSymbol typeSymbol) {
            return false;
        }

        // 如果没有约束类型，回退到检查是否继承自 RegisterBasics
        if (constraintType == null) {
            return InheritsFromType(typeSymbol, RegisterBasicsTypeName);
        }

        // 检查是否继承自约束类型
        return InheritsFromTypeSymbol(typeSymbol, constraintType);
    }

    /// <summary>
    /// 检查类型是否继承自指定的类型符号或与指定类型相同
    /// </summary>
    public static bool InheritsFromTypeSymbol(INamedTypeSymbol typeSymbol, ITypeSymbol targetType) {
        // 首先检查是否为同一类型
        if (SymbolEqualityComparer.Default.Equals(typeSymbol, targetType)) {
            return true;
        }

        // 然后检查继承关系
        INamedTypeSymbol? currentType = typeSymbol.BaseType;
        while (currentType != null) {
            if (SymbolEqualityComparer.Default.Equals(currentType, targetType)) {
                return true;
            }
            currentType = currentType.BaseType;
        }
        return false;
    }

    /// <summary>
    /// 检查表达式是否为字段或属性访问，并从右侧表达式推断类型
    /// </summary>
    public static bool IsFieldOrPropertyAccess(SyntaxNode leftExpression, SyntaxNode rightExpression, SemanticModel semanticModel, out (string name, string type) fieldInfo) {
        fieldInfo = default;

        // 处理简单标识符 (this.field 或 field)
        if (leftExpression is IdentifierNameSyntax identifierName) {
            string fieldName = identifierName.Identifier.ValueText;
            
            // 尝试从语义模型获取类型信息
            SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(identifierName);
            if (symbolInfo.Symbol is IFieldSymbol fieldSymbol) {
                fieldInfo = (fieldSymbol.Name, fieldSymbol.Type.Name);
                return true;
            }
            if (symbolInfo.Symbol is IPropertySymbol propertySymbol) {
                fieldInfo = (propertySymbol.Name, propertySymbol.Type.Name);
                return true;
            }
            
            // 如果语义模型找不到（字段尚未声明），从右侧表达式推断类型
            if (rightExpression is ObjectCreationExpressionSyntax newExpression) {
                TypeInfo typeInfo = semanticModel.GetTypeInfo(newExpression);
                if (typeInfo.Type is INamedTypeSymbol typeSymbol) {
                    fieldInfo = (fieldName, typeSymbol.Name);
                    return true;
                }
            }
        }

        // 处理成员访问 (this.field)
        if (leftExpression is MemberAccessExpressionSyntax memberAccess) {
            SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(memberAccess);
            if (symbolInfo.Symbol is IFieldSymbol fieldSymbol) {
                fieldInfo = (fieldSymbol.Name, fieldSymbol.Type.Name);
                return true;
            }
            if (symbolInfo.Symbol is IPropertySymbol propertySymbol) {
                fieldInfo = (propertySymbol.Name, propertySymbol.Type.Name);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 获取目标类文件的所有 using 语句
    /// </summary>
    public static IEnumerable<UsingDirectiveSyntax> GetUsingDirectives(ClassDeclarationSyntax classDeclaration) {
        CompilationUnitSyntax? compilationUnit = classDeclaration.SyntaxTree.GetRoot() as CompilationUnitSyntax;
        return compilationUnit?.Usings ?? [];
    }

    public static SyntaxList<TNode> List<TNode>(params TNode[] nodes) where TNode : SyntaxNode {
        return new SyntaxList<TNode>(nodes);
    }

    /// <summary>
    /// 检查类中是否已经存在指定名称的字段或属性
    /// </summary>
    public static bool HasExistingMember(INamedTypeSymbol classSymbol, string memberName) {
        // 检查静态字段
        bool hasField = classSymbol.GetMembers(memberName)
            .OfType<IFieldSymbol>()
            .Any(f => f.IsStatic);

        // 检查静态属性
        bool hasProperty = classSymbol.GetMembers(memberName)
            .OfType<IPropertySymbol>()
            .Any(p => p.IsStatic);

        return hasField || hasProperty;
    }

    /// <summary>
    /// 获取语法节点前的注释文本，特别处理以 $ 开头的注释
    /// </summary>
    public static string GetLeadingComments(SyntaxNode node) {
        var leadingTrivia = node.GetLeadingTrivia();
        var comments = new List<string>();
        
        foreach (var trivia in leadingTrivia) {
            if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)) {
                // 单行注释，移除 // 前缀并保留内容
                string commentText = trivia.ToString().TrimStart('/').Trim();
                if (!string.IsNullOrEmpty(commentText)) {
                    // 检查是否以 $ 开头，如果是则只保留 $ 后面的内容（不加注释符号）
                    if (commentText.StartsWith("$") && commentText.Length > 1) {
                        // 去掉 $ 符号，保留后面的内容，不添加 // 前缀
                        string specialComment = commentText.Substring(1).Trim();
                        if (!string.IsNullOrEmpty(specialComment)) {
                            comments.Add(specialComment);
                        }
                    } else {
                        // 普通注释直接复制
                        comments.Add("// " + commentText);
                    }
                }
            }
            else if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)) {
                // 多行注释，保留原格式
                comments.Add(trivia.ToString());
            }
            else if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                     trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)) {
                // XML 文档注释
                comments.Add(trivia.ToString());
            }
        }
        
        return string.Join("\n", comments);
    }

    /// <summary>
    /// 检查赋值语句是否被条件语句包围（如 if 语句）
    /// </summary>
    public static bool IsWithinConditionalStatement(SyntaxNode assignmentNode) {
        SyntaxNode? current = assignmentNode.Parent;
        
        while (current != null) {
            // 检查是否在 if 语句中
            if (current is IfStatementSyntax) {
                return true;
            }
            
            // 检查是否在条件表达式中 (condition ? true : false)
            if (current is ConditionalExpressionSyntax) {
                return true;
            }
            
            // 检查是否在 switch 语句中
            if (current is SwitchStatementSyntax || current is SwitchExpressionSyntax) {
                return true;
            }
            
            // 如果到达了方法体的顶层，停止查找
            if (current is MethodDeclarationSyntax) {
                break;
            }
            
            current = current.Parent;
        }
        
        return false;
    }

    /// <summary>
    /// 检查赋值语句是否在对象初始化器中
    /// </summary>
    public static bool IsInsideObjectInitializer(SyntaxNode assignmentNode) {
        SyntaxNode? current = assignmentNode.Parent;
        
        while (current != null) {
            // 如果遇到 lambda 表达式或匿名方法，视为独立作用域，停止向上查找
            if (current is LambdaExpressionSyntax || 
                current is AnonymousMethodExpressionSyntax ||
                current is LocalFunctionStatementSyntax) {
                return false;
            }
            
            // 检查是否在初始化器表达式中
            if (current is InitializerExpressionSyntax) {
                return true;
            }
            
            // 如果到达了语句级别，说明不在初始化器中
            if (current is StatementSyntax) {
                return false;
            }
            
            current = current.Parent;
        }
        
        return false;
    }

    /// <summary>
    /// 检查类中是否存在与指定名称和类型匹配的属性或字段
    /// </summary>
    public static bool HasMatchingProperty(INamedTypeSymbol classSymbol, string memberName, ITypeSymbol expectedType) {
        // 检查属性
        var property = classSymbol.GetMembers(memberName)
            .OfType<IPropertySymbol>()
            .FirstOrDefault();
            
        if (property != null) {
            // 检查类型是否兼容（考虑可空类型）
            return AreTypesCompatible(property.Type, expectedType);
        }

        // 检查字段
        var field = classSymbol.GetMembers(memberName)
            .OfType<IFieldSymbol>()
            .FirstOrDefault();
            
        if (field != null) {
            return AreTypesCompatible(field.Type, expectedType);
        }

        return false;
    }

    /// <summary>
    /// 检查两个类型是否兼容（考虑可空类型）
    /// </summary>
    public static bool AreTypesCompatible(ITypeSymbol propertyType, ITypeSymbol valueType) {
        // 完全相同的类型
        if (SymbolEqualityComparer.Default.Equals(propertyType, valueType)) {
            return true;
        }

        // 如果属性类型是可空的，检查去掉可空修饰符后是否匹配
        if (propertyType is INamedTypeSymbol namedPropertyType && 
            namedPropertyType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T) {
            var underlyingType = namedPropertyType.TypeArguments.FirstOrDefault();
            if (underlyingType != null && SymbolEqualityComparer.Default.Equals(underlyingType, valueType)) {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 验证现有字段是否符合生成字段的要求
    /// </summary>
    public static bool ValidateExistingMember(INamedTypeSymbol classSymbol, string memberName, string expectedType, out string actualType, out string errorReason) {
        actualType = "";
        errorReason = "";

        // 检查静态属性
        IPropertySymbol? property = classSymbol.GetMembers(memberName)
            .OfType<IPropertySymbol>()
            .FirstOrDefault(p => p.IsStatic);

        if (property != null) {
            actualType = property.Type.Name;
            
            // 检查访问修饰符
            if (property.DeclaredAccessibility != Accessibility.Public) {
                errorReason = "Property must be public";
                return false;
            }

            // 检查是否为静态
            if (!property.IsStatic) {
                errorReason = "Property must be static";
                return false;
            }

            // 检查类型
            if (property.Type.Name != expectedType) {
                errorReason = $"Type mismatch: expected {expectedType}, got {property.Type.Name}";
                return false;
            }

            // 检查访问器
            if (property.GetMethod == null || property.GetMethod.DeclaredAccessibility != Accessibility.Public) {
                errorReason = "Property must have a public get accessor";
                return false;
            }

            if (property.SetMethod == null || property.SetMethod.DeclaredAccessibility != Accessibility.Private) {
                errorReason = "Property must have a private set accessor";
                return false;
            }

            return true;
        }

        // 检查静态字段（不符合要求）
        IFieldSymbol? field = classSymbol.GetMembers(memberName)
            .OfType<IFieldSymbol>()
            .FirstOrDefault(f => f.IsStatic);

        if (field != null) {
            actualType = field.Type.Name;
            errorReason = "Must be a property, not a field";
            return false;
        }

        // 成员不存在
        return true;
    }


}
