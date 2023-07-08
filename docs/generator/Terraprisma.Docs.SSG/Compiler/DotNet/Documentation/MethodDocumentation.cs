using Mono.Cecil;

namespace Terraprisma.Docs.SSG.Compiler.DotNet.Documentation;

public sealed class MethodDocumentation : MemberDocumentation {
    public MethodDocumentation(string @namespace, string name, string assemblyName) : base(@namespace, name, assemblyName) { }

    public static MethodDocumentation FromMethodDefinition(MethodDefinition methodDefinition) {
        return default!;
    }
}
