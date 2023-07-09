using Terraprisma.Docs.SSG.Compiler.DotNet;
using Terraprisma.Docs.SSG.Compiler.Markdown;
using Terraprisma.Docs.SSG.Configuration;

namespace Terraprisma.Docs.SSG.Compiler;

/// <summary>
///     The compiler compilation context, which contains all compilers, their
///     settings, and transient data.
/// </summary>
public sealed class CompilationContext {
    // TODO: Add styles.css
    private static readonly string html_template = @"
<!DOCTYPE html>
<html>
    <head>
        <link rel=""stylesheet"" href=""/styles.css"">
        <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
        <title>{0}</title>
    </head>
    <body>
{1}
    </body>
</html>
".Trim();

    public Dictionary<string, ICompiler> Compilers { get; } = new();

    public CompilerConfiguration Config { get; }

    public CompilationContext(CompilerConfiguration config) {
        Config = config;
    }

    public void AddCompiler(string name, ICompiler compiler) {
        Compilers.Add(name, compiler);
    }

    public void CompileProject(string projectName) {
        if (!Config.Projects.TryGetValue(projectName, out var project))
            throw new InvalidOperationException($"Cannot compile unknown project '{project}'.");

        foreach (var (name, ns) in project.Namespaces) {
            Console.WriteLine($"    Compiling namespace '{name}'.");

            try {
                if (!Compilers.TryGetValue(ns.Type, out var compiler))
                    throw new InvalidOperationException($"Unknown compiler type '{ns.Type}'.");

                var compiledHtml = compiler.Compile(this, ns);
                Console.WriteLine("        Compiled to resulting files:");

                if (compiledHtml.Count == 0) {
                    Console.WriteLine("            <empty>");
                }
                else {
                    foreach (var fileName in compiledHtml.Keys)
                        Console.WriteLine($"            {fileName}");
                }

                foreach (var (fileName, html) in compiledHtml) {
                    var outputDir = ns.Root ? project.OutputDir : Path.Combine(project.OutputDir, name);
                    var path = Path.Combine(outputDir, fileName) + ".html";
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                    File.WriteAllText(path, ContextualizeHtml(fileName, html));
                }
            }
            catch {
                Console.WriteLine($"    Compilation of namespace '{name}' failed.");
                throw;
            }

            Console.WriteLine($"    Compiled namespace '{name}'.");
        }
    }

    // TODO: Provide a way to specify the title lol
    private static string ContextualizeHtml(string fileName, string html) {
        return string.Format(html_template, fileName, html);
    }

    public static CompilationContext MakeDefault(CompilerConfiguration config) {
        var context = new CompilationContext(config);
        context.AddCompiler("markdown", new MarkdownCompiler());
        context.AddCompiler("dotnet", new DotNetCompiler());
        return context;
    }
}
