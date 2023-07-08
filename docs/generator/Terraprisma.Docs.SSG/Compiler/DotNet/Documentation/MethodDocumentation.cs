using Mono.Cecil;

namespace Terraprisma.Docs.SSG.Compiler.DotNet.Documentation;

public class MethodDocumentation : MemberDocumentation {
    public TypeDocumentation Parent { get; }

    public MethodDocumentation(string @namespace, string name, string assemblyName, TypeDocumentation parent) : base(@namespace, name, assemblyName) {
        Parent = parent;
    }

    public static MethodDocumentation FromMethodDefinition(MethodDefinition methodDefinition) {
        return default!;
    }
}
