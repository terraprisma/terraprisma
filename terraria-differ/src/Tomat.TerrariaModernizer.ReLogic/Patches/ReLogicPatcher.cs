using System;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MonoMod.Cil;
using ReLogic.Localization.IME;
using ReLogic.OS;
using ReLogic.OS.Linux;
using ReLogic.OS.OSX;
using ReLogic.OS.Windows;

namespace Tomat.TerrariaModernizer.ReLogic.Patches;

public static class ReLogicPatcher {
    public static void Patch(ModuleDefinition module) {
        PatchCurrentPlatform(module);
        // PatchFnaIme(module);
    }

    // Conditionally sets ReLogic.Platform::Current based on the OS.
    private static void PatchCurrentPlatform(ModuleDefinition module) {
        var platform = module.GetType("ReLogic.OS.Platform");
        var platformStaticCtor = platform.GetStaticConstructor();
        var il = new ILContext(platformStaticCtor);
        var c = new ILCursor(il);
        c.GotoNext(MoveType.Before, x => x.MatchNewobj(out _));
        c.Remove();
        c.EmitDelegate(() => {
            if (OperatingSystem.IsWindows())
                return (Platform)new WindowsPlatform();
            if (OperatingSystem.IsLinux())
                return new LinuxPlatform();
            if (OperatingSystem.IsMacOS())
                return new OsxPlatform();

            throw new PlatformNotSupportedException("The current platform is not supported.");
        });
    }
}
