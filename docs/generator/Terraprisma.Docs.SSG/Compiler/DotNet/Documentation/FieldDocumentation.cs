using Mono.Cecil;

namespace Terraprisma.Docs.SSG.Compiler.DotNet.Documentation;

public sealed class FieldDocumentation : MemberDocumentation {
    public FieldDocumentation(string @namespace, string name, string assemblyName) : base(@namespace, name, assemblyName) { }

    public static FieldDocumentation FromFieldDefinition(FieldDefinition fieldDefinition) {
        return default!;
    }
}
