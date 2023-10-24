using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Terraria;

partial class WindowsLaunch
{
	private static void Main(string[] args)
	{
		Environment.SetEnvironmentVariable("FNA_WORKAROUND_WINDOW_RESIZABLE", "1");
		ResolveDependencies();
		Program.LaunchGame(args, monoArgs: true /*!OperatingSystem.IsWindows()*/);
	}

	internal static void InitializeWithParsedArguments(Dictionary<string, string> args)
	{
		bool dedServ = args.ContainsKey("-server");
		if (dedServ)
		{
			_handleRoutine = ConsoleCtrlCheck;
			SetConsoleCtrlHandler(_handleRoutine, add: true);
		}
	}

	private static void ResolveDependencies()
	{
		string platform = OperatingSystem.IsWindows()
			? "win"
			: OperatingSystem.IsMacOS()
				? "osx"
				: OperatingSystem.IsLinux()
					? "linux"
					: throw new PlatformNotSupportedException();

		// TODO: Support arm64/aarch64...
		string arch = Environment.Is64BitProcess ? "x64" : "x86";
		string path = Path.Combine("Libraries", "Natives", platform + '-' + arch);
		foreach (string file in Directory.GetFiles(path))
			NativeLibrary.Load(file);

		string managedPath = Path.Combine(path, "Managed");
		List<Assembly> dependentAssemblies = [];
		foreach (string file in Directory.GetFiles(managedPath))
			dependentAssemblies.Add(Assembly.LoadFrom(file));

		AppDomain.CurrentDomain.AssemblyResolve += (_, args) => {
			foreach (Assembly assembly in dependentAssemblies)
				if (assembly.FullName == args.Name)
					return assembly;

			return null;
		};
	}
}
