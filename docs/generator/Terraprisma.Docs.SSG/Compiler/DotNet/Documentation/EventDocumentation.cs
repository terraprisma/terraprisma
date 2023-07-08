using Mono.Cecil;

namespace Terraprisma.Docs.SSG.Compiler.DotNet.Documentation;

public sealed class EventDocumentation : MemberDocumentation {
    public EventDocumentation(string @namespace, string name, string assemblyName) : base(@namespace, name, assemblyName) { }

    public static EventDocumentation FromEventDefinition(EventDefinition eventDefinition) {
        return default!;
    }
}
