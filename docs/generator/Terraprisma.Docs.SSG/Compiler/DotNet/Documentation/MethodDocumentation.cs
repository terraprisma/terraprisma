using Mono.Cecil;
using Terraprisma.Docs.SSG.Compiler.DotNet.Documentation.Models;
using GenericParameter = Terraprisma.Docs.SSG.Compiler.DotNet.Documentation.Models.GenericParameter;

namespace Terraprisma.Docs.SSG.Compiler.DotNet.Documentation;

public class MethodDocumentation : MemberDocumentation {
    public AccessModifierKind AccessModifier { get; set; }

    public InstanceKind InstanceKind { get; set; }

    public SealedKind SealedKind { get; set; }

    public AbstractKind AbstractKind { get; set; }

    public List<GenericParameter>? GenericParameters { get; set; }

    public List<Parameter>? Parameters { get; set; }

    public string ReturnType { get; }

    public bool ReturnTypeIsGeneric { get; }

    public MethodDocumentation(string @namespace, string name, string assemblyName, string returnType, bool returnTypeIsGeneric) : base(@namespace, name, assemblyName) {
        ReturnType = returnType;
        ReturnTypeIsGeneric = returnTypeIsGeneric;
    }

    public override string ToString() {
        var name = /*Parent +*/ $"-{NormalizeName(Name)}";

        if (GenericParameters is not null)
            name += $"-{GenericParameters.Count}";

        name += '(';

        var first = true;

        if (Parameters is not null) {
            foreach (var parameter in Parameters) {
                if (!first)
                    name += '-';
                first = false;
                name += NormalizeParameterName(parameter);
            }
        }

        name += ')';
        return name;
    }

    public static MethodDocumentation FromMethodDefinition(MethodDefinition methodDefinition, TypeDocumentation parent) {
        var methodDoc = new MethodDocumentation(
            @namespace: parent.Namespace,
            name: NormalizeName(methodDefinition.Name),
            assemblyName: methodDefinition.Module.Assembly.Name.Name,
            returnType: methodDefinition.ReturnType.FullName,
            returnTypeIsGeneric: methodDefinition.ReturnType.IsGenericParameter
        );

        if (methodDefinition.IsPublic)
            methodDoc.AccessModifier = AccessModifierKind.Public;
        else if (methodDefinition.IsPrivate)
            methodDoc.AccessModifier = AccessModifierKind.Private;
        else if (methodDefinition.IsAssembly)
            methodDoc.AccessModifier = AccessModifierKind.Internal;
        else if (methodDefinition.IsFamily)
            methodDoc.AccessModifier = AccessModifierKind.Protected;
        else if (methodDefinition.IsFamilyOrAssembly)
            methodDoc.AccessModifier = AccessModifierKind.ProtectedInternal;
        else if (methodDefinition.IsFamilyAndAssembly)
            methodDoc.AccessModifier = AccessModifierKind.PrivateProtected;

        methodDoc.InstanceKind = methodDefinition.IsStatic ? InstanceKind.Static : InstanceKind.Instance;
        methodDoc.SealedKind = methodDefinition.IsFinal ? SealedKind.Sealed : SealedKind.Unsealed;
        methodDoc.AbstractKind = methodDefinition.IsAbstract ? AbstractKind.Abstract : AbstractKind.NotAbstract;
        if (methodDefinition.IsVirtual)
            methodDoc.AbstractKind = AbstractKind.Virtual;

        if (methodDefinition.HasGenericParameters) {
            methodDoc.GenericParameters = new List<GenericParameter>();

            foreach (var genericParameter in methodDefinition.GenericParameters) {
                methodDoc.GenericParameters.Add(new GenericParameter {
                    Name = genericParameter.Name,
                });
            }
        }

        if (methodDefinition.HasParameters) {
            methodDoc.Parameters = new List<Parameter>();

            foreach (var parameter in methodDefinition.Parameters) {
                methodDoc.Parameters.Add(
                    new Parameter(
                        name: parameter.Name,
                        typeOrGenericName: parameter.ParameterType.FullName,
                        isGeneric: parameter.ParameterType.IsGenericParameter
                    )
                );
            }
        }

        return methodDoc;
    }
}
