namespace Terraprisma.Docs.SSG.Compiler.DotNet.Documentation;

public abstract class MemberWithSelfTypeDocumentation : MemberDocumentation {
    public string SelfType { get; }

    public bool SelfTypeIsGeneric { get; }

    protected MemberWithSelfTypeDocumentation(string @namespace, string name, string assemblyName, string selfType, bool selfTypeIsGeneric) : base(@namespace, name, assemblyName) {
        SelfType = selfType;
        SelfTypeIsGeneric = selfTypeIsGeneric;
    }
}
