using Mono.Cecil;
using Terraprisma.Docs.SSG.Compiler.DotNet.Documentation.Models;

namespace Terraprisma.Docs.SSG.Compiler.DotNet.Documentation;

public sealed class EventDocumentation : MemberWithSelfTypeDocumentation {
    public AccessModifierKind AccessModifier { get; set; }

    public InstanceKind InstanceKind { get; set; }

    public AbstractKind AbstractKind { get; set; }

    public SealedKind SealedKind { get; set; }

    public EventDocumentation(string @namespace, string name, string assemblyName, string selfType, bool selfTypeIsGeneric) : base(@namespace, name, assemblyName, selfType, selfTypeIsGeneric) { }

    public override string ToString() {
        return NormalizeName(Name);
    }

    public static EventDocumentation FromEventDefinition(EventDefinition eventDefinition) {
        // TODO: Check if AddMethod should be used here and if IsGenericInstance
        // is correct (as opposed to IsGenericParameter?).

        var eventDoc = new EventDocumentation(
            @namespace: eventDefinition.DeclaringType.Namespace,
            name: NormalizeName(eventDefinition.Name),
            assemblyName: eventDefinition.Module.Assembly.Name.Name,
            selfType: eventDefinition.EventType.FullName,
            selfTypeIsGeneric: eventDefinition.EventType.IsGenericInstance
        );

        if (eventDefinition.AddMethod.IsPublic)
            eventDoc.AccessModifier = AccessModifierKind.Public;
        else if (eventDefinition.AddMethod.IsPrivate)
            eventDoc.AccessModifier = AccessModifierKind.Private;
        else if (eventDefinition.AddMethod.IsAssembly)
            eventDoc.AccessModifier = AccessModifierKind.Internal;
        else if (eventDefinition.AddMethod.IsFamily)
            eventDoc.AccessModifier = AccessModifierKind.Protected;
        else if (eventDefinition.AddMethod.IsFamilyOrAssembly)
            eventDoc.AccessModifier = AccessModifierKind.ProtectedInternal;

        eventDoc.InstanceKind = eventDefinition.AddMethod.IsStatic ? InstanceKind.Static : InstanceKind.Instance;
        eventDoc.AbstractKind = eventDefinition.AddMethod.IsAbstract ? AbstractKind.Abstract : AbstractKind.NotAbstract;
        eventDoc.SealedKind = eventDefinition.AddMethod.IsFinal ? SealedKind.Sealed : SealedKind.Unsealed;
        if (eventDefinition.AddMethod.IsVirtual)
            eventDoc.AbstractKind = AbstractKind.Virtual;
        return default!;
    }
}
