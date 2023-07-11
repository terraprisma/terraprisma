using Terraprisma.Patcher;

namespace Terraprisma.Patches.NetCore;

/// <summary>
///     Updates a vanilla Terraria installation to use .NET (Core) instead of
///     .NET Framework.
/// </summary>
public sealed class NetCorePatch : IGamePatch {
    public string PatchName => "Terraria .NET (Core)";

    public Version PatchVersion => new(1, 0, 0);

    public IEnumerable<IPatchStep> PatchSteps {
        get {
            yield break;
        }
    }
}
