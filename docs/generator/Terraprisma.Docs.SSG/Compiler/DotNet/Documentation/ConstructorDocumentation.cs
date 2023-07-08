using Mono.Cecil;
using Terraprisma.Docs.SSG.Compiler.DotNet.Documentation.Models;
using GenericParameter = Terraprisma.Docs.SSG.Compiler.DotNet.Documentation.Models.GenericParameter;

namespace Terraprisma.Docs.SSG.Compiler.DotNet.Documentation;

public sealed class ConstructorDocumentation : MethodDocumentation {
    public AccessModifierKind AccessModifier { get; set; }

    public List<GenericParameter>? GenericParameters { get; set; }

    public List<Parameter>? Parameters { get; set; }

    public ConstructorDocumentation(string @namespace, string name, string assemblyName, TypeDocumentation parent) : base(@namespace, name, assemblyName, parent) { }

    public override string ToString() {
        var name = /*Parent +*/ "-ctor";

        if (GenericParameters is not null)
            name += $"-{GenericParameters.Count}";

        name += '(';

        var first = true;

        if (Parameters is not null) {
            if (!first)
                name += '-';
            first = true;
            foreach (var parameter in Parameters)
                name += NormalizeParameterName(parameter);
        }

        name += ')';
        return name;
    }

    public static ConstructorDocumentation FromConstructorDefinition(MethodDefinition methodDefinition, TypeDocumentation parent) {
        var ctorDoc = new ConstructorDocumentation(
            @namespace: parent.Namespace,
            name: NormalizeName(methodDefinition.FullName),
            assemblyName: methodDefinition.Module.Assembly.Name.Name,
            parent: parent
        );

        // Only some are possible with constructors but who cares?
        if (methodDefinition.IsPublic)
            ctorDoc.AccessModifier = AccessModifierKind.Public;
        else if (methodDefinition.IsPrivate)
            ctorDoc.AccessModifier = AccessModifierKind.Private;
        else if (methodDefinition.IsAssembly)
            ctorDoc.AccessModifier = AccessModifierKind.Internal;
        else if (methodDefinition.IsFamily)
            ctorDoc.AccessModifier = AccessModifierKind.Protected;
        else if (methodDefinition.IsFamilyOrAssembly)
            ctorDoc.AccessModifier = AccessModifierKind.ProtectedInternal;
        else if (methodDefinition.IsFamilyAndAssembly)
            ctorDoc.AccessModifier = AccessModifierKind.PrivateProtected;

        if (methodDefinition.HasGenericParameters) {
            ctorDoc.GenericParameters = new List<GenericParameter>();

            foreach (var genericParameter in methodDefinition.GenericParameters) {
                ctorDoc.GenericParameters.Add(new GenericParameter {
                    Name = genericParameter.Name,
                });
            }
        }

        if (methodDefinition.HasParameters) {
            ctorDoc.Parameters = new List<Parameter>();

            foreach (var parameter in methodDefinition.Parameters) {
                ctorDoc.Parameters.Add(
                    new Parameter(
                        name: parameter.Name,
                        typeOrGenericName: parameter.ParameterType.FullName,
                        isGeneric: parameter.ParameterType.IsGenericParameter
                    )
                );
            }
        }

        return ctorDoc;
    }
}
