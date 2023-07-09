using Terraprisma.Docs.SSG.Configuration;

namespace Terraprisma.Docs.SSG.Compiler.Markdown;

/// <summary>
///     A Markdig-powered Markdown compiler which compiles a directory of
///     Markdown files.
/// </summary>
public sealed class MarkdownCompiler : ICompiler {
    public Dictionary<string, CompiledPage> Compile(CompilationContext context, CompilationNamespace ns) {
        if (!Directory.Exists(ns.Input))
            throw new DirectoryNotFoundException($"The directory {ns.Input} does not exist.");

        var files = Directory.GetFiles(ns.Input, "*.md", SearchOption.AllDirectories);

        var output = new Dictionary<string, CompiledPage>();

        // TODO: Some way to augment with custom handling of links and stuff.
        foreach (var file in files) {
            var path = Path.GetRelativePath(ns.Input, file);

            // GetFileNameWithoutExtension deletes the parent directory.
            path = Path.ChangeExtension(path, "");

            // Remove the last character, which is a dot (ChangeExtension
            // doesn't remove it).
            path = path[..^1];

            // Normalize path separators.
            path = path.Replace('\\', '/');

            // TODO: Handle titles with frontmatter.
            output.Add(path, CompiledPage.FromRawHtml("TITLES ARE A TODO", Markdig.Markdown.ToHtml(File.ReadAllText(file))));
        }

        return output;
    }
}
