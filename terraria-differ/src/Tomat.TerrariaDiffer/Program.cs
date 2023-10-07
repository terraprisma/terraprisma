using System;
using System.IO;
using System.Reflection;
using DotnetPatcher.Decompile;
using DotnetPatcher.Diff;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;

namespace Tomat.TerrariaDiffer;

internal static class Program {
    private static readonly Game game = new(105600);
    private static readonly Manifest terraria_release = new(105601, "TerrariaRelease");
    private static readonly Manifest terraria_linux = new(105602, "TerrariaLinux");
    private static readonly Manifest terraria_mac = new(105603, "TerrariaMac");
    private const string file_exclusion_regex = "^.*(?<!\\.xnb)(?<!\\.xwb)(?<!\\.xsb)(?<!\\.xgs)(?<!\\.bat)(?<!\\.txt)(?<!\\.xml)(?<!\\.msi)$";

    private static readonly DepotDiffNode decompilation_configuration = new(
        "TerrariaRelease",
        "TerrariaClientWindows",
        "Terraria.exe",
        new DepotDiffNode(
            "TerrariaLinux",
            "TerrariaClientLinux",
            "Terraria.exe"
        ),
        new DepotDiffNode(
            "TerrariaMac",
            "TerrariaClientMac",
            "Terraria.app/Contents/Resources/Terraria.exe"
        ),
        new DepotDiffNode(
            "TerrariaRelease",
            "TerrariaServerWindows",
            "TerrariaServer.exe",
            new DepotDiffNode(
                "TerrariaLinux",
                "TerrariaServerLinux",
                "TerrariaServer.exe"
            ),
            new DepotDiffNode(
                "TerrariaMac",
                "TerrariaServerMac",
                "Terraria.app/Contents/Resources/TerrariaServer.exe"
            )
        )
    );

    internal static void Main(string[] args) {
        if (Environment.GetEnvironmentVariable("SKIP_DOWNLOAD") != "1") {
            var username = Console.ReadLine()!;
            var password = Console.ReadLine()!;

            File.WriteAllText("filelist.txt", "regex:" + file_exclusion_regex);
            var depotDownloaderAsm = typeof(DepotDownloader.PlatformUtilities).Assembly;
            DownloadManifest(depotDownloaderAsm, username, password, terraria_release);
            DownloadManifest(depotDownloaderAsm, username, password, terraria_linux);
            DownloadManifest(depotDownloaderAsm, username, password, terraria_mac);
        }

        DecompileAndDiff(decompilation_configuration);
    }

    private static void NullifyInstance(Type type) {
        var instanceField = type.GetField("Instance", BindingFlags.Static | BindingFlags.Public);
        instanceField!.SetValue(null, null);
    }

    private static void DownloadManifest(Assembly depotDownloaderAssembly, string username, string password, Manifest manifest) {
        NullifyInstance(depotDownloaderAssembly.GetType("DepotDownloader.AccountSettingsStore")!);
        NullifyInstance(depotDownloaderAssembly.GetType("DepotDownloader.DepotConfigStore")!);

        var appId = game.AppId;
        var depot = manifest.DepotId;

        if (Directory.Exists(manifest.Name))
            Directory.Delete(manifest.Name, true);

        depotDownloaderAssembly.EntryPoint!.Invoke(
            null,
            new object[] {
                new[] {
                    "-app",
                    appId.ToString(),
                    "-depot",
                    depot.ToString(),
                    "-filelist",
                    "filelist.txt",
                    "-username",
                    username,
                    "-password",
                    password,
                    "-dir",
                    manifest.Name,
                    //"-remember-password",
                },
            }
        );
    }

    private static void DecompileAndDiff(DepotDiffNode node, DepotDiffNode? parent = null) {
        const string decompilation_dir = "decompiled";
        const string patches_dir = "patches";
        var dirName = Path.Combine(decompilation_dir, node.WorkspaceName);

        if (Environment.GetEnvironmentVariable("SKIP_DECOMPILATION") != "1") {
            Console.WriteLine($"Decompiling {node.WorkspaceName}...");

            if (Directory.Exists(dirName))
                Directory.Delete(dirName, true);
            Directory.CreateDirectory(dirName);

            var formatting = FormattingOptionsFactory.CreateKRStyle();
            formatting.IndentationString = "    ";
            var decompiler = new Decompiler(
                Path.Combine(node.DepotName, node.RelativePathToExecutable),
                dirName,
                new DecompilerSettings {
                    CSharpFormattingOptions = FormattingOptionsFactory.CreateKRStyle()
                }
            );
            decompiler.Decompile(new[] { "ReLogic", /*"LogitechLedEnginesWrapper",*/ "RailSDK.Net", "SteelSeriesEngineWrapper" });
        }

        foreach (var child in node.Children)
            DecompileAndDiff(child, node);

        if (parent is null)
            return;

        if (Environment.GetEnvironmentVariable("SKIP_DIFFING") == "1")
            return;

        Console.WriteLine($"Diffing {node.WorkspaceName}...");

        var patchDirName = Path.Combine(patches_dir, node.WorkspaceName);
        if (Directory.Exists(patchDirName))
            Directory.Delete(patchDirName, true);
        Directory.CreateDirectory(patchDirName);

        var differ = new Differ(Path.Combine(decompilation_dir, parent.WorkspaceName), patchDirName, dirName);
        differ.Diff();
    }
}
