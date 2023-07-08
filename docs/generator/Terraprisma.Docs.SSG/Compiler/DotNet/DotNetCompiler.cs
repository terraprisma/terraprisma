using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using Mono.Cecil;
using Terraprisma.Docs.SSG.Compiler.DotNet.Documentation;
using Terraprisma.Docs.SSG.Configuration;

namespace Terraprisma.Docs.SSG.Compiler.DotNet;

/// <summary>
///     An MSBuild-powered .NET compiler which can compile a project and analyze
///     the compiled assembly, along with a resulting summary file.
/// </summary>
public sealed class DotNetCompiler : ICompiler {
    private static readonly List<string> name_blacklist = new() {
        "-<module>", // <Module>
    };

    public Dictionary<string, string> Compile(CompilationContext context, CompilationNamespace ns) {
        if (!File.Exists(ns.Input))
            throw new FileNotFoundException($"The file {ns.Input} does not exist.");

        if (!MSBuildLocator.IsRegistered) {
            var instance = MSBuildLocator.QueryVisualStudioInstances().First();
            MSBuildLocator.RegisterInstance(instance);
        }

        var (assemblyFilePath, summaryFilePath) = CompileMsBuildProject(ns.Input);
        return GenerateDocumentationFromAssembly(assemblyFilePath, summaryFilePath);
    }

    private (string assemblyFilePath, string? summaryFilePath) CompileMsBuildProject(string projectPath) {
        var projectCollection = new ProjectCollection();
        var project = new Project(projectPath, null, null, projectCollection);

        var outputDir = project.GetPropertyValue("OutputPath");
        var outputFileName = project.GetPropertyValue("AssemblyName") + ".dll";
        var summaryFileName = project.GetPropertyValue("DocumentationFile");

        var outputPath = Path.Combine(Path.GetDirectoryName(projectPath)!, outputDir, outputFileName);
        var summaryPath = Path.Combine(Path.GetDirectoryName(projectPath)!, outputDir, summaryFileName);

        if (!project.Build())
            throw new Exception("Failed to build project.");

        if (!File.Exists(outputPath))
            throw new FileNotFoundException($"The output file '{outputPath}' does not exist.");

        if (string.IsNullOrEmpty(summaryFileName) || !File.Exists(summaryPath)) {
            Console.WriteLine($"    Warning: The summary file '{summaryPath}' does not exist.");
            summaryPath = null;
        }

        return (outputPath, summaryPath);
    }

    private static Dictionary<string, string> GenerateDocumentationFromAssembly(string assemblyFilePath, string? summaryFilePath) {
        var typeDocs = CreateDocsFromAssembly(assemblyFilePath).Where(x => !name_blacklist.Contains(x.ToString()));
        var output = new Dictionary<string, string>();
        foreach (var typeDoc in typeDocs)
            GenerateOutputsFromType(typeDoc, output);
        return output;
    }

    private static List<TypeDocumentation> CreateDocsFromAssembly(string assemblyFilePath) {
        var module = ModuleDefinition.ReadModule(assemblyFilePath);
        var typeDocs = new List<TypeDocumentation>();

        // use Types and not GetTypes as we handle nested types ourselves.
        var types = module.Types; //module.GetTypes();
        foreach (var type in types)
            typeDocs.Add(AddMemberDocsForType(type));

        return typeDocs;
    }

    private static void GenerateOutputsFromType(TypeDocumentation typeDoc, Dictionary<string, string> output) {
        if (name_blacklist.Contains(typeDoc.ToString()))
            return;

        // TODO: Just marking for now. We need a page for the type and a page
        // for each of its members.
        output.Add(typeDoc.ToString(), "");
    }

    private static TypeDocumentation AddMemberDocsForType(TypeDefinition type) {
        return TypeDocumentation.FromTypeDefinition(type);
    }
}
