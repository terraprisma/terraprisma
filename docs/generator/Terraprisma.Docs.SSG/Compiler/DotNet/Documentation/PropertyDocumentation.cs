using Mono.Cecil;
using Terraprisma.Docs.SSG.Compiler.DotNet.Documentation.Models;

namespace Terraprisma.Docs.SSG.Compiler.DotNet.Documentation;

public sealed class PropertyDocumentation : MemberWithSelfTypeDocumentation {
    public AccessModifierKind AccessModifier { get; set; }

    public InstanceKind InstanceKind { get; set; }

    public AbstractKind AbstractKind { get; set; }

    public SealedKind SealedKind { get; set; }

    public AccessModifierKind GetterAccessModifier { get; set; }

    public AccessModifierKind SetterAccessModifier { get; set; }

    public bool HasGetter { get; set; }

    public bool HasSetter { get; set; }

    public PropertyDocumentation(string @namespace, string name, string assemblyName, string selfType, bool selfTypeIsGeneric) : base(@namespace, name, assemblyName, selfType, selfTypeIsGeneric) { }

    public override string ToString() {
        return NormalizeName(Name);
    }

    public static PropertyDocumentation FromPropertyDefinition(PropertyDefinition propertyDefinition) {
        var propertyDoc = new PropertyDocumentation(
            @namespace: propertyDefinition.DeclaringType.Namespace,
            name: NormalizeName(propertyDefinition.Name),
            assemblyName: propertyDefinition.Module.Assembly.Name.Name,
            selfType: propertyDefinition.PropertyType.FullName,
            selfTypeIsGeneric: propertyDefinition.PropertyType.IsGenericInstance
        );

        if (propertyDefinition.GetMethod.IsPublic)
            propertyDoc.AccessModifier = AccessModifierKind.Public;
        else if (propertyDefinition.GetMethod.IsPrivate)
            propertyDoc.AccessModifier = AccessModifierKind.Private;
        else if (propertyDefinition.GetMethod.IsAssembly)
            propertyDoc.AccessModifier = AccessModifierKind.Internal;
        else if (propertyDefinition.GetMethod.IsFamily)
            propertyDoc.AccessModifier = AccessModifierKind.Protected;
        else if (propertyDefinition.GetMethod.IsFamilyOrAssembly)
            propertyDoc.AccessModifier = AccessModifierKind.ProtectedInternal;

        propertyDoc.InstanceKind = propertyDefinition.GetMethod.IsStatic ? InstanceKind.Static : InstanceKind.Instance;
        propertyDoc.AbstractKind = propertyDefinition.GetMethod.IsAbstract ? AbstractKind.Abstract : AbstractKind.NotAbstract;
        propertyDoc.SealedKind = propertyDefinition.GetMethod.IsFinal ? SealedKind.Sealed : SealedKind.Unsealed;
        if (propertyDefinition.GetMethod.IsVirtual)
            propertyDoc.AbstractKind = AbstractKind.Virtual;

        propertyDoc.HasGetter = propertyDefinition.GetMethod != null;
        propertyDoc.HasSetter = propertyDefinition.SetMethod != null;

        if (propertyDefinition.GetMethod != null) {
            if (propertyDefinition.GetMethod.IsPublic)
                propertyDoc.GetterAccessModifier = AccessModifierKind.Public;
            else if (propertyDefinition.GetMethod.IsPrivate)
                propertyDoc.GetterAccessModifier = AccessModifierKind.Private;
            else if (propertyDefinition.GetMethod.IsAssembly)
                propertyDoc.GetterAccessModifier = AccessModifierKind.Internal;
            else if (propertyDefinition.GetMethod.IsFamily)
                propertyDoc.GetterAccessModifier = AccessModifierKind.Protected;
            else if (propertyDefinition.GetMethod.IsFamilyOrAssembly)
                propertyDoc.GetterAccessModifier = AccessModifierKind.ProtectedInternal;
        }

        if (propertyDefinition.SetMethod != null) {
            if (propertyDefinition.SetMethod.IsPublic)
                propertyDoc.SetterAccessModifier = AccessModifierKind.Public;
            else if (propertyDefinition.SetMethod.IsPrivate)
                propertyDoc.SetterAccessModifier = AccessModifierKind.Private;
            else if (propertyDefinition.SetMethod.IsAssembly)
                propertyDoc.SetterAccessModifier = AccessModifierKind.Internal;
            else if (propertyDefinition.SetMethod.IsFamily)
                propertyDoc.SetterAccessModifier = AccessModifierKind.Protected;
            else if (propertyDefinition.SetMethod.IsFamilyOrAssembly)
                propertyDoc.SetterAccessModifier = AccessModifierKind.ProtectedInternal;
        }

        return propertyDoc;
    }
}
