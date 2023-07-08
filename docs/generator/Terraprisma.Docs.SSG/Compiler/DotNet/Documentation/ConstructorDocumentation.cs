using Mono.Cecil;

namespace Terraprisma.Docs.SSG.Compiler.DotNet.Documentation;

public sealed class ConstructorDocumentation : MemberDocumentation {
    public TypeDocumentation Parent { get; }

    public ConstructorDocumentation(string @namespace, string name, string assemblyName, TypeDocumentation parent) : base(@namespace, name, assemblyName) {
        Parent = parent;
    }

    public static ConstructorDocumentation FromConstructorDefinition(MethodDefinition methodDefinition, TypeDocumentation parent) {
        
        return default!;
    }
}
