using Terraprisma.Docs.SSG.Compiler.DotNet.Documentation.Models;

namespace Terraprisma.Docs.SSG.Compiler.DotNet.Documentation;

public abstract class MemberDocumentation {
    public string Namespace { get; }

    public string Name { get; }

    public string AssemblyName { get; }

    protected MemberDocumentation(string @namespace, string name, string assemblyName) {
        Namespace = @namespace;
        Name = name;
        AssemblyName = assemblyName;
    }

    protected static string NormalizeName(string name) {
        // Namespace.MyType -> namespace-mytype
        // Namespace.myType.MyMethod(System.Int32) -> namespace-mytype-my-method(system-int32)
        // Namespace.myType.MyMethod(System.Int32,System.String) -> namespace-mytype-my-method(system-int32-system-string)
        return name.ToLower().Replace('.', '-').Replace(',', '-').Replace('`', '-');
    }

    protected static string NormalizeParameterName(Parameter parameter) {
        return NormalizeName(parameter.TypeOrGenericName);
    }
}
