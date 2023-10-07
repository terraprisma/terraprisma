using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using MonoMod;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace Tomat.TerrariaModernizer;

/// <summary>
///     Handles modernizing the Terraria assembly and its dependencies,
///     relinking it to FNA and updating to .NET 7.0.
/// </summary>
public sealed class ModernizerModder : MonoModder {
    private static readonly string[] libs_to_remove = { "System.Core", "System", "Microsoft.Xna.Framework", "System.Windows.Forms", "Microsoft.Xna.Framework.Graphics", "Microsoft.Xna.Framework.Game", "System.Drawing" };

    private static readonly string[] private_system_libraries = { "System.Private.CoreLib" };

    private static readonly Dictionary<string, string> assembly_remap = new() {
        { "CsvHelper", "CsvHelper" },
        { "Ionic.Zip.CF", "Ionic.Zip.Reduced" },
        { "Newtonsoft.Json", "Newtonsoft.Json" },
        { "MP3Sharp", "MP3Sharp" },
        { "NVorbis", "NVorbis" },
    };

    private readonly string workspace;
    private readonly Assembly formsAssembly;

    public ModernizerModder(string workspace) {
        this.workspace = workspace;
        formsAssembly = Assembly.LoadFile(Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location)!, "System.Windows.Forms.dll"));
    }

    public override void MapDependencies() {
        foreach (var lib in libs_to_remove)
            Module.AssemblyReferences.Remove(Module.AssemblyReferences.FirstOrDefault(x => x.Name == lib));

        foreach (var (name, newName) in assembly_remap) {
            var index = Module.AssemblyReferences.IndexOf(Module.AssemblyReferences.FirstOrDefault(x => x.Name == name));

            if (index == -1)
                continue;

            Module.AssemblyReferences.RemoveAt(index);
            AddReference(newName);
        }

        AddReference("System.Runtime");
        AddReference(Assembly.GetExecutingAssembly().GetName());
        // Module.ImportReference(typeof(object));

        base.MapDependencies();
    }

    public override IMetadataTokenProvider Relinker(IMetadataTokenProvider mtp, IGenericParameterProvider context) {
        var relinkedMember = base.Relinker(mtp, context);

        if (relinkedMember is TypeReference type) {
            if (libs_to_remove.Contains(type.Scope.Name)) {
                if (type.Namespace.StartsWith("System.Windows.Forms"))
                    return Module.ImportReference(formsAssembly.GetType(type.FullName));

                return Module.ImportReference(FindType(type.FullName));
            }
        }

        return relinkedMember;
    }

    public override void PatchRefs() {
        base.PatchRefs();

        foreach (var lib in private_system_libraries)
            Module.AssemblyReferences.Remove(Module.AssemblyReferences.FirstOrDefault(x => x.Name == lib));
    }

    private void AddReference(AssemblyName name) {
        if (Module.AssemblyReferences.All(x => x.Name != name.Name))
            Module.AssemblyReferences.Add(AssemblyNameReference.Parse(name.FullName));
    }

    private void AddReference(string name) {
        var asm = Assembly.GetExecutingAssembly().GetReferencedAssemblies().FirstOrDefault(x => x.Name == name);

        if (asm is not null) {
            AddReference(asm);
            return;
        }

        var workspacePath = Path.Combine(workspace, name + ".dll");

        if (!File.Exists(workspacePath))
            throw new Exception($"Could not find assembly {name}");

        AddReference(AssemblyName.GetAssemblyName(workspacePath));
    }

    private static bool MonoCanInline(MethodBody body) {
        return body.CodeSize < 20;
    }
}
