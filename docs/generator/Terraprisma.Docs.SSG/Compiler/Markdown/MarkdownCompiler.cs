using Terraprisma.Docs.SSG.Configuration;

namespace Terraprisma.Docs.SSG.Compiler.Markdown;

/// <summary>
///     A Markdig-powered Markdown compiler which compiles a directory of
///     Markdown files.
/// </summary>
public sealed class MarkdownCompiler : ICompiler {
    public Dictionary<string, string> Compile(CompilationContext context, CompilationNamespace ns) {
        if (!Directory.Exists(ns.Input))
            throw new DirectoryNotFoundException($"The directory {ns.Input} does not exist.");

        return new Dictionary<string, string>();
    }
}
