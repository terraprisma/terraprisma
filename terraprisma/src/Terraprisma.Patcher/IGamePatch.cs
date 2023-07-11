using System;
using System.Collections.Generic;

namespace Terraprisma.Patcher;

/// <summary>
///     Represents a game patch, which may have an arbitrary amount of patch
///     steps.
/// </summary>
public interface IGamePatch {
    /// <summary>
    ///     The human-readable name of the patch.
    /// </summary>
    string PatchName { get; }

    /// <summary>
    ///     The version of the patch.
    /// </summary>
    Version PatchVersion { get; }

    /// <summary>
    ///     An ordered collection of patch steps to apply in succession.
    /// </summary>
    IEnumerable<IPatchStep> PatchSteps { get; }
}
