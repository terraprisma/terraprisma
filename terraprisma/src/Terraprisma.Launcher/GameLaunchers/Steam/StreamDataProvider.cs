using System;
using System.IO;
using Microsoft.Win32;

namespace Terraprisma.Launcher.GameLaunchers.Steam;

/// <summary>
///     Provides sometimes-platform-dependent data for Steam, such as paths.
/// </summary>
public static class StreamDataProvider {
    /// <summary>
    ///     Finds the platform-dependent Steam installation path.
    /// </summary>
    /// <returns>
    ///     The existing path to the system's Steam install, or
    ///     <see langword="null"/> if no path could be found or none exist.
    /// </returns>
    public static string? GetSteamInstallPath() {
        var home = Environment.GetEnvironmentVariable("HOME");

        if (OperatingSystem.IsWindows()) {
            if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) is string steamPath && Directory.Exists(steamPath))
                return steamPath;
        }
        else if (OperatingSystem.IsMacOS() && home is not null) {
            // No idea where it is on macOS.
        }
        else if (OperatingSystem.IsLinux() && home is not null) {
            var dotSteamPath = Path.Combine(home, ".steam", "steam");
            if (Directory.Exists(dotSteamPath))
                return dotSteamPath;

            var dotLocalPath = Path.Combine(home, ".local", "share", "Steam");
            if (Directory.Exists(dotLocalPath))
                return dotLocalPath;

            var dotVarPath = Path.Combine(home, ".var", "app", "com.valvesoftware.Steam", "data", "Steam");
            if (Directory.Exists(dotVarPath))
                return dotVarPath;
        }

        return null;
    }

    /*public static int? GetCurrentUserId() {
        
    }*/
}
