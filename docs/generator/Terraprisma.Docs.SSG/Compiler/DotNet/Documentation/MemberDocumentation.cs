namespace Terraprisma.Docs.SSG.Compiler.DotNet.Documentation;

public abstract class MemberDocumentation {
    protected static string NormalizeName(string name) {
        // Namespace.MyType -> namespace-mytype
        // Namespace.myType.MyMethod(System.Int32) -> namespace-mytype-my-method(system-int32)
        // Namespace.myType.MyMethod(System.Int32,System.String) -> namespace-mytype-my-method(system-int32-system-string)
        return name.ToLower().Replace('.', '-').Replace(',', '-');
    }
}
