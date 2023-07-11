using System.Collections.Generic;

namespace Terraprisma.Patcher;

/// <summary>
///     Provides an arbitrary amount of game patches.
/// </summary>
public interface IPatchProvider {
    /// <summary>
    ///     The unique name of the patch provider.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    ///     The channel of the patch provider.
    /// </summary>
    string ProviderChannel { get; }
    
    /// <summary>
    ///     A collection of patches provided by this patch provider.
    /// </summary>
    List<IGamePatch> Patches { get; }
}
