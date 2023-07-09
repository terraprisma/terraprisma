using Mono.Cecil;
using Terraprisma.Docs.SSG.Compiler.DotNet.Documentation.Models;

namespace Terraprisma.Docs.SSG.Compiler.DotNet.Documentation;

public sealed class FieldDocumentation : MemberWithSelfTypeDocumentation {
    public AccessModifierKind AccessModifier { get; set; }

    public InstanceKind InstanceKind { get; set; }

    public FieldDocumentation(string @namespace, string name, string assemblyName, string selfType, bool selfTypeIsGeneric) : base(@namespace, name, assemblyName, selfType, selfTypeIsGeneric) { }

    public override string ToString() {
        return NormalizeName(Name);
    }

    public static FieldDocumentation FromFieldDefinition(FieldDefinition fieldDefinition) {
        var fieldDoc = new FieldDocumentation(
            @namespace: fieldDefinition.DeclaringType.Namespace,
            name: NormalizeName(fieldDefinition.Name),
            assemblyName: fieldDefinition.Module.Assembly.Name.Name,
            selfType: fieldDefinition.FieldType.FullName,
            selfTypeIsGeneric: fieldDefinition.FieldType.IsGenericInstance
        );
        
        if (fieldDefinition.IsPublic)
            fieldDoc.AccessModifier = AccessModifierKind.Public;
        else if (fieldDefinition.IsPrivate)
            fieldDoc.AccessModifier = AccessModifierKind.Private;
        else if (fieldDefinition.IsAssembly)
            fieldDoc.AccessModifier = AccessModifierKind.Internal;
        else if (fieldDefinition.IsFamily)
            fieldDoc.AccessModifier = AccessModifierKind.Protected;
        else if (fieldDefinition.IsFamilyOrAssembly)
            fieldDoc.AccessModifier = AccessModifierKind.ProtectedInternal;
        
        fieldDoc.InstanceKind = fieldDefinition.IsStatic ? InstanceKind.Static : InstanceKind.Instance;
        return fieldDoc;
    }
}
