namespace Terraprisma.Patcher;

/// <summary>
///     A patch step, which may perform
/// </summary>
public interface IPatchStep {
    /// <summary>
    ///     The human-readable name of the patch step.
    /// </summary>
    string StepName { get; }

    /// <summary>
    ///     Applies the patch step.
    /// </summary>
    /// <param name="patchDirectory">
    ///     The directory of the patch, where the patch step should write files
    ///     to.
    /// </param>
    /// <param name="terrariaDirectory">
    ///     The directory of the Terraria installation, where the patch step
    ///     should read files from.
    /// </param>
    /// <returns>
    ///     The reason for failure, or <see langword="null"/> if the patch step
    ///     succeeded.
    /// </returns>
    string? Apply(string patchDirectory, string terrariaDirectory);
}
