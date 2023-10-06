namespace Tomat.TerrariaDiffer;

public sealed class DepotDiffNode {
    public string DepotName { get; }

    public string WorkspaceName { get; }

    public string RelativePathToExecutable { get; }

    public DepotDiffNode[] Children { get; }

    public DepotDiffNode(string depotName, string workspaceName, string relativePathToExecutable, params DepotDiffNode[] children) {
        DepotName = depotName;
        WorkspaceName = workspaceName;
        RelativePathToExecutable = relativePathToExecutable;
        Children = children;
    }
}
