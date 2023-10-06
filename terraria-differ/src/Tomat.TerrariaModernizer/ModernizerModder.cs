using System.Reflection;
using Mono.Cecil;
using MonoMod;
using MethodBody = Mono.Cecil.Cil.MethodBody;
using MethodImplAttributes = Mono.Cecil.MethodImplAttributes;

namespace Tomat.TerrariaModernizer;

/// <summary>
///     Handles modernizing the Terraria assembly and its dependencies,
///     relinking it to FNA and updating to .NET 7.0.
/// </summary>
public sealed class ModernizerModder : MonoModder {
    private static readonly string[] libs_to_remove = { "mscorlib", "System.Core", "System", "Microsoft.Xna.Framework", "System.Windows.Forms", "Microsoft.Xna.Framework.Graphics", "Microsoft.Xna.Framework.Game", "System.Drawing" };

    private static readonly Dictionary<string, string> assembly_remap = new() {
        { "CsvHelper", "CsvHelper" },
        { "Ionic.Zip.CF", "Ionic.Zip.Reduced" },
        { "Newtonsoft.Json", "Newtonsoft.Json" },
        { "MP3Sharp", "MP3Sharp" },
        { "NVorbis", "NVorbis" },
    };

    private readonly string workspace;

    public ModernizerModder(string workspace) {
        this.workspace = workspace;
    }

    public override void MapDependencies() {
        foreach (var lib in libs_to_remove)
            Module.AssemblyReferences.Remove(Module.AssemblyReferences.FirstOrDefault(x => x.Name == lib));

        foreach (var (name, newName) in assembly_remap) {
            var index = Module.AssemblyReferences.IndexOf(Module.AssemblyReferences.FirstOrDefault(x => x.Name == name));

            if (index != -1) {
                Module.AssemblyReferences.RemoveAt(index);
                AddReference(newName);
            }
        }

        AddReference("System.Runtime");
        AddReference(Assembly.GetExecutingAssembly().GetName());

        base.MapDependencies();
    }

    /*public override void MapDependencies(ModuleDefinition main) {
        // TODO: Use an updated version of Ionic.Zip to avoid this reference.
        if (!libs_to_remove.Contains(main.Name[..^".dll".Length]))
            base.MapDependencies(main);
    }*/

    public override IMetadataTokenProvider Relinker(IMetadataTokenProvider mtp, IGenericParameterProvider context) {
        var relinkedMember = base.Relinker(mtp, context);

        if (relinkedMember is TypeReference type && libs_to_remove.Contains(type.Scope.Name))
            return Module.ImportReference(FindType(type.FullName));

        return relinkedMember;
    }

    public override void PatchRefs(ModuleDefinition mod) {
        base.PatchRefs(mod);

        // for (var i = 0; i < mod.AssemblyReferences.Count; i++) {
        //     if (libs_to_remove.Contains(mod.AssemblyReferences[i].Name))
        //         mod.AssemblyReferences.RemoveAt(i--);
        // }

        foreach (var lib in libs_to_remove)
            Module.AssemblyReferences.Remove(Module.AssemblyReferences.FirstOrDefault(x => x.Name == lib));

        foreach (var (name, newName) in assembly_remap) {
            var index = Module.AssemblyReferences.IndexOf(Module.AssemblyReferences.FirstOrDefault(x => x.Name == name));

            if (index != -1) {
                Module.AssemblyReferences.RemoveAt(index);
                AddReference(newName);
            }
        }
    }

    public override void PatchRefsInMethod(MethodDefinition method) {
        base.PatchRefsInMethod(method);

        // The conditions for inlining in Mono are sane and known, let's use
        // that.
        if (!method.ImplAttributes.HasFlag(MethodImplAttributes.AggressiveInlining) || !MonoCanInline(method.Body))
            return;

        method.ImplAttributes |= MethodImplAttributes.AggressiveInlining;
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
