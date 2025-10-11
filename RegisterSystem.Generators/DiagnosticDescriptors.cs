using Microsoft.CodeAnalysis;

namespace RegisterSystem.Generator;

public class DiagnosticDescriptors {

    public static readonly DiagnosticDescriptor notPartial = new DiagnosticDescriptor(
        "RSG001",
        "Class must be declared as partial",
        "Class '{0}' must be declared as partial to enable code generation",
        "RegisterSystemGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor missingSetupMethod = new DiagnosticDescriptor(
        "RSG002",
        "Missing override setup method",
        "Class '{0}' must contain an override method named 'setup' to enable code generation",
        "RegisterSystemGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor emptySetupMethodBody = new DiagnosticDescriptor(
        "RSG003",
        "Setup method body cannot be empty",
        "The override 'setup' method in class '{0}' cannot have an empty body",
        "RegisterSystemGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor fieldTypeMismatch = new DiagnosticDescriptor(
        "RSG004",
        "Field type does not match expected type",
        "Field '{0}' in class '{1}' has type '{2}' but expected type '{3}', The field must be a public static property with get and private set accessors",
        "RegisterSystemGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor wrongAssignmentType = new DiagnosticDescriptor(
        "RSG005",
        "Assignment type does not match generic constraint",
        "Assignment to '{0}' in class '{1}' uses type '{2}' which does not inherit from the generic constraint '{3}'",
        "RegisterSystemGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

}