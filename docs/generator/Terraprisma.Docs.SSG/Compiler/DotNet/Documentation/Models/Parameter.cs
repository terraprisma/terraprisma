namespace Terraprisma.Docs.SSG.Compiler.DotNet.Documentation.Models;

public sealed class Parameter {
    public string Name { get; }

    public string TypeOrGenericName { get; }

    public bool IsGeneric { get; }

    public Parameter(string name, string typeOrGenericName, bool isGeneric) {
        Name = name;
        TypeOrGenericName = typeOrGenericName;
        IsGeneric = isGeneric;
    }
}
