using Terraprisma.Patcher;

namespace Terraprisma.Patches.NetCore;

public sealed class NetCorePatchProvider : IPatchProvider {
    public string ProviderName => "terraprisma.patches.netcore";

    public string ProviderChannel => "stable";

    public List<IGamePatch> Patches { get; } = new();
}
