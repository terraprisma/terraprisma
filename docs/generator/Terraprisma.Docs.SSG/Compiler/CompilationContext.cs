using HtmlAgilityPack;
using Terraprisma.Docs.SSG.Compiler.DotNet;
using Terraprisma.Docs.SSG.Compiler.Markdown;
using Terraprisma.Docs.SSG.Configuration;

namespace Terraprisma.Docs.SSG.Compiler;

/// <summary>
///     The compiler compilation context, which contains all compilers, their
///     settings, and transient data.
/// </summary>
public sealed class CompilationContext {
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

                foreach (var (fileName, compiledPage) in compiledHtml) {
                    var outputDir = ns.Root ? project.OutputDir : Path.Combine(project.OutputDir, name);
                    var path = Path.Combine(outputDir, fileName) + ".html";
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                    // TODO: Use actual title instead of fileName
                    var doc = MakeDefaultDocument(compiledPage);
                    doc.Save(path);
                }
            }
            catch {
                Console.WriteLine($"    Compilation of namespace '{name}' failed.");
                throw;
            }

            Console.WriteLine($"    Compiled namespace '{name}'.");
        }
    }

    private static HtmlDocument MakeDefaultDocument(CompiledPage page) {
        var htmlDoc = new HtmlDocument();

        var htmlNode = htmlDoc.CreateElement("html");

        // <head>
        var headNode = htmlDoc.CreateElement("head");
        var linkNode = htmlDoc.CreateElement("link");
        linkNode.SetAttributeValue("rel", "stylesheet");
        linkNode.SetAttributeValue("href", "/styles-docs.css");
        headNode.AppendChild(linkNode);
        var metaNode = htmlDoc.CreateElement("meta");
        metaNode.SetAttributeValue("name", "viewport");
        metaNode.SetAttributeValue("content", "width=device-width, initial-scale=1");
        headNode.AppendChild(metaNode);
        var titleNode = htmlDoc.CreateElement("title");
        titleNode.AppendChild(htmlDoc.CreateTextNode(page.Title));
        headNode.AppendChild(titleNode);
        htmlNode.AppendChild(headNode);

        // <body>
        var bodyNode = htmlDoc.CreateElement("body");
        var mainViewNode = htmlDoc.CreateElement("div");
        mainViewNode.SetAttributeValue("class", "main-view");
        var leftSidebarNode = htmlDoc.CreateElement("div");
        leftSidebarNode.SetAttributeValue("class", "left-sidebar");
        var leftSidebarP = htmlDoc.CreateElement("p");
        leftSidebarP.AppendChild(htmlDoc.CreateTextNode("left sidebar"));
        leftSidebarNode.AppendChild(leftSidebarP);
        mainViewNode.AppendChild(leftSidebarNode);
        mainViewNode.AppendChild(page.Html);
        var rightSidebarNode = htmlDoc.CreateElement("div");
        rightSidebarNode.SetAttributeValue("class", "right-sidebar");
        var rightSidebarP = htmlDoc.CreateElement("p");
        rightSidebarP.AppendChild(htmlDoc.CreateTextNode("right sidebar"));
        rightSidebarNode.AppendChild(rightSidebarP);
        mainViewNode.AppendChild(rightSidebarNode);
        bodyNode.AppendChild(mainViewNode);
        htmlNode.AppendChild(bodyNode);

        // Append the generated <html> element.
        htmlDoc.DocumentNode.AppendChild(htmlNode);

        // All this work just to add <!DOCTYPE html> to the start of the
        // document... maybe it's just easier to prepend later... lol
        var doctypeHtml = htmlDoc.CreateComment("<!DOCTYPE html>");
        htmlDoc.DocumentNode.InsertBefore(doctypeHtml, htmlNode);

        return htmlDoc;
    }

    public static CompilationContext MakeDefault(CompilerConfiguration config) {
        var context = new CompilationContext(config);
        context.AddCompiler("markdown", new MarkdownCompiler());
        context.AddCompiler("dotnet", new DotNetCompiler());
        return context;
    }
}
