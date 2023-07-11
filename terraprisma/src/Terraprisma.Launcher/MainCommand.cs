using System.IO;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Terraprisma.Launcher.GameLaunchers.Steam;

namespace Terraprisma.Launcher;

/// <summary>
///     The main command of the launcher.
/// </summary>
[Command]
public class MainCommand : ICommand {
    /// <summary>
    ///     The directory of the launcher, where patches should be installed to.
    /// </summary>
    [CommandParameter(0, Description = "The directory of the launcher, where patches should be installed to.")]
    public string? LauncherDirectory { get; set; }

    /// <summary>
    ///     The directory of the Terraria installation, assumed to be the
    ///     directory above the launcher directory if not specified.
    /// </summary>
    [CommandOption("terraria-path", 't', Description = "The directory of the Terraria installation, assumed to be the directory above the launcher directory if not specified", IsRequired = false)]
    public string? TerrariaPath { get; set; } = null;

    async ValueTask ICommand.ExecuteAsync(IConsole console) {
        await console.Output.WriteLineAsync($"Terraprisma.Launcher v{typeof(Program).Assembly.GetName().Version}");

        LauncherDirectory ??= Path.GetDirectoryName(typeof(Program).Assembly.Location) ?? Directory.GetCurrentDirectory();
        TerrariaPath ??= Path.GetFullPath(Path.Combine(LauncherDirectory, ".."));

        await console.Output.WriteLineAsync($"Using launcher directory: {LauncherDirectory}");
        await console.Output.WriteLineAsync($"Using Terraria directory: {TerrariaPath}");

        var steamPath = StreamDataProvider.GetSteamInstallPath();
        if (steamPath is null)
            return;

        var userdataPath = Path.Combine(steamPath, "userdata");
        if (!Directory.Exists(userdataPath))
            return;

        // TODO: Detect current user instead of this.
        foreach (var user in Directory.GetDirectories(userdataPath)) {
            var shortcutsPath = Path.Combine(user, "config", "shortcuts.vdf");
            if (!File.Exists(shortcutsPath))
                continue;

            await console.Output.WriteLineAsync($"Found shortcuts file for user {user}");
            var shortcuts = VDFParser.VDFParser.Parse(shortcutsPath);
            _ = shortcuts;
        }
    }
}
