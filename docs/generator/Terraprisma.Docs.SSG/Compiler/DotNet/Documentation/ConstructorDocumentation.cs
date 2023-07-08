using Mono.Cecil;
using Terraprisma.Docs.SSG.Compiler.DotNet.Documentation.Models;
using GenericParameter = Terraprisma.Docs.SSG.Compiler.DotNet.Documentation.Models.GenericParameter;

namespace Terraprisma.Docs.SSG.Compiler.DotNet.Documentation;

public sealed class ConstructorDocumentation : MethodDocumentation {
    public ConstructorDocumentation(string @namespace, string name, string assemblyName) : base(@namespace, name, assemblyName) { }

    public static ConstructorDocumentation FromConstructorDefinition(MethodDefinition methodDefinition, TypeDocumentation parent) {
        var ctorDoc = new ConstructorDocumentation(
            @namespace: parent.Namespace,
            name: NormalizeName(methodDefinition.FullName),
            assemblyName: methodDefinition.Module.Assembly.Name.Name
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
