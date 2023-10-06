using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Mono.Cecil;
using Tomat.TerrariaModernizer.ReLogic;
using Tomat.TerrariaModernizer.ReLogic.Patches;
using Tomat.TerrariaModernizer.Terraria;
using Tomat.TerrariaModernizer.Terraria.Patches;

namespace Tomat.TerrariaModernizer;

internal static class Program {
    internal static void Main(string[] args) {
        if (args.Length != 1)
            throw new ArgumentException("Expected one argument: the path to the Terraria directory.");

        var terrariaDir = args[0];
        if (!Directory.Exists(terrariaDir))
            throw new ArgumentException("The specified Terraria directory does not exist.");

        var terrariaExe = Path.Combine(terrariaDir, "Terraria.exe");
        if (!File.Exists(terrariaExe))
            throw new ArgumentException("The specified Terraria directory does not contain Terraria.exe.");

        terrariaDir = SetUpModdableWorkspace(terrariaDir);

        PatchRailSdk(terrariaDir);
        LoadAndReproduceNewAssemblies();
        PatchReLogic(terrariaDir);
        PatchTerraria(terrariaDir);
    }

    private static string SetUpModdableWorkspace(string dir) {
        var workspace = Path.Combine(dir, "modernized");
        if (Directory.Exists(workspace))
            Directory.Delete(workspace, true);
        Directory.CreateDirectory(workspace);

        foreach (var dll in Directory.GetFiles(dir, "*.dll"))
            File.Copy(dll, Path.Combine(workspace, Path.GetFileName(dll)));

        foreach (var dll in Directory.GetFiles(dir, "*.xml"))
            File.Copy(dll, Path.Combine(workspace, Path.GetFileName(dll)));

        foreach (var dll in Directory.GetFiles(dir, "*.txt"))
            File.Copy(dll, Path.Combine(workspace, Path.GetFileName(dll)));

        File.Copy(Path.Combine(dir, "Terraria.exe"), Path.Combine(workspace, "Terraria.exe"));

        using var terrariaModule = ModuleDefinition.ReadModule(Path.Combine(dir, "Terraria.exe"));
        var resourcesToExtract = new Dictionary<string, string> {
            // { "Terraria.Libraries.CsvHelper.CsvHelper.dll", "CsvHelper.dll" },
            // { "Terraria.Libraries.DotNetZip.Ionic.Zip.CF.dll", "Ionic.Zip.CF.dll" },
            // { "Terraria.Libraries.JSON.NET.Newtonsoft.Json.dll", "Newtonsoft.Json.dll" },
            // { "Terraria.Libraries.MP3Sharp.MP3Sharp.dll", "MP3Sharp.dll" },
            // { "Terraria.Libraries.NVorbis.NVorbis.dll", "NVorbis.dll" },
            // { "Terraria.Libraries.NVorbis.System.ValueTuple.dll", "" },
            { "Terraria.Libraries.RailSDK.Windows.RailSDK.Net.dll", "RailSDK.Net.dll" },
            { "Terraria.Libraries.ReLogic.ReLogic.dll", "ReLogic.dll" },
        };

        foreach (var (resource, file) in resourcesToExtract) {
            var resourceInstance = terrariaModule.Resources.FirstOrDefault(r => r.Name == resource);
            if (resourceInstance is null)
                throw new Exception($"Could not find resource {resource} in Terraria.exe.");

            if (resourceInstance is not EmbeddedResource embeddedResource)
                throw new Exception($"Resource {resource} in Terraria.exe is not an embedded resource.");

            using var fileStream = File.Create(Path.Combine(workspace, file));
            using var resourceStream = embeddedResource.GetResourceStream();
            resourceStream.CopyTo(fileStream);
        }

        return workspace;
    }

    private static void LoadAndReproduceNewAssemblies() {
        Console.WriteLine(typeof(CsvHelper.ArrayHelper).Assembly.Location);
        Console.WriteLine(typeof(Ionic.Zip.CloseDelegate).Assembly.Location);
        Console.WriteLine(typeof(Newtonsoft.Json.JsonConvert).Assembly.Location);
        Console.WriteLine(typeof(MP3Sharp.MP3SharpException).Assembly.Location);
        Console.WriteLine(typeof(NVorbis.VorbisReader).Assembly.Location);
        Console.WriteLine(typeof(Microsoft.Xna.Framework.Vector2).Assembly.Location);
        Console.WriteLine(typeof(System.Windows.Forms.Application));
        Console.WriteLine(typeof(System.Drawing.Graphics));
    }

    private static void PatchRailSdk(string workspace) {
        var railSdk = Path.Combine(workspace, "RailSDK.Net.dll");
        var railSdkModule = AssemblyDefinition.ReadAssembly(railSdk);
        railSdkModule.MainModule.Architecture = TargetArchitecture.I386;
        railSdkModule.MainModule.Attributes = ModuleAttributes.ILOnly;
        using var ms = new MemoryStream();
        railSdkModule.Write(ms);
        railSdkModule.Dispose();
        File.WriteAllBytes(railSdk, ms.ToArray());
    }

    private static void PatchReLogic(string workspace) {
        using var modder = new ModernizerModder(workspace) {
            InputPath = Path.Combine(workspace, "ReLogic.dll"),
            OutputPath = Path.Combine(workspace, "PATCHED_ReLogic.dll"),
            MissingDependencyThrow = false,
        };
        modder.Read();
        PatchNetVersion(modder.Module);
        modder.ReadMod(typeof(ReLogic.MonoModRules).Assembly.Location);
        modder.MapDependencies();
        ReLogicPatcher.Patch(modder.Module);
        modder.AutoPatch();
        modder.Write();
    }

    private static void PatchTerraria(string workspace) {
        using var modder = new ModernizerModder(workspace) {
            InputPath = Path.Combine(workspace, "Terraria.exe"),
            OutputPath = Path.Combine(workspace, "PATCHED_Terraria.exe"),
            MissingDependencyThrow = false,
        };
        modder.Read();
        PatchNetVersion(modder.Module);
        modder.ReadMod(typeof(Terraria.MonoModRules).Assembly.Location);
        modder.MapDependencies();
        TerrariaPatcher.Patch(modder.Module);
        modder.AutoPatch();
        modder.Write();
    }

    private static void PatchNetVersion(ModuleDefinition module) {
        module.RuntimeVersion = Assembly.GetExecutingAssembly().ImageRuntimeVersion;
        module.Attributes &= ~(ModuleAttributes.Required32Bit | ModuleAttributes.Preferred32Bit);
        var tfxAttr = Assembly.GetExecutingAssembly().GetCustomAttribute<TargetFrameworkAttribute>();
        var moduleAttr = module.Assembly.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == typeof(TargetFrameworkAttribute).FullName);

        if (moduleAttr is not null)
            module.Assembly.CustomAttributes.Remove(moduleAttr);

        module.Assembly.CustomAttributes.Add(new CustomAttribute(module.ImportReference(typeof(TargetFrameworkAttribute).GetConstructor(new[] { typeof(string) }))) {
            ConstructorArguments = {
                new CustomAttributeArgument(module.ImportReference(typeof(string)), tfxAttr!.FrameworkName),
            },
        });

        var dbgAttr = Assembly.GetExecutingAssembly().GetCustomAttribute<DebuggableAttribute>();
        var debuggable = module.Assembly.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == typeof(DebuggableAttribute).FullName);

        if (debuggable is not null)
            module.Assembly.CustomAttributes.Remove(debuggable);

        module.Assembly.CustomAttributes.Add(new CustomAttribute(module.ImportReference(typeof(DebuggableAttribute).GetConstructor(new[] { typeof(DebuggableAttribute.DebuggingModes) }))) {
            ConstructorArguments = {
                new CustomAttributeArgument(module.ImportReference(typeof(DebuggableAttribute.DebuggingModes)), dbgAttr!.DebuggingFlags),
            },
        });

        module.Assembly.CustomAttributes.Add(new CustomAttribute(module.ImportReference(typeof(InternalsVisibleToAttribute).GetConstructor(new[] { typeof(string) }))) {
            ConstructorArguments = {
                new CustomAttributeArgument(module.ImportReference(typeof(string)), "Tomat.TerrariaModernizer.ReLogic"),
            },
        });
        module.Assembly.CustomAttributes.Add(new CustomAttribute(module.ImportReference(typeof(InternalsVisibleToAttribute).GetConstructor(new[] { typeof(string) }))) {
            ConstructorArguments = {
                new CustomAttributeArgument(module.ImportReference(typeof(string)), "Tomat.TerrariaModernizer.Terraria"),
            },
        });
    }
}
