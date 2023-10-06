using System.Reflection;

namespace Tomat.TerrariaDiffer;

internal static class Program {
    private static readonly Game game = new(105600);
    private static readonly Manifest terraria_release = new(105601, "TerrariaRelease");
    private static readonly Manifest terraria_linux = new(105602, "TerrariaLinux");
    private static readonly Manifest terraria_mac = new(105603, "TerrariaMac");
    private const string file_exclusion_regex = "^.*(?<!\\.xnb)(?<!\\.xwb)(?<!\\.xsb)(?<!\\.xgs)(?<!\\.bat)(?<!\\.txt)(?<!\\.xml)(?<!\\.msi)$";

    internal static void Main(string[] args) {
        var username = Console.ReadLine()!;
        var password = Console.ReadLine()!;

        File.WriteAllText("filelist.txt", "regex:" + file_exclusion_regex);

        var depotDownloaderAsm = typeof(DepotDownloader.PlatformUtilities).Assembly;
        DownloadManifest(depotDownloaderAsm, username, password, terraria_release);
        DownloadManifest(depotDownloaderAsm, username, password, terraria_linux);
        DownloadManifest(depotDownloaderAsm, username, password, terraria_mac);
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
}
