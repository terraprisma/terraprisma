using Mono.Cecil;

namespace Terraprisma.Docs.SSG.Compiler.DotNet.Documentation;

public sealed class PropertyDocumentation : MemberDocumentation {
    public PropertyDocumentation(string @namespace, string name, string assemblyName) : base(@namespace, name, assemblyName) { }

    public static PropertyDocumentation FromPropertyDefinition(PropertyDefinition propertyDefinition) {
        return default!;
    }
}
