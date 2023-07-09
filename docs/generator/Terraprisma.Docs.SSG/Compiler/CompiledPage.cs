using HtmlAgilityPack;

namespace Terraprisma.Docs.SSG.Compiler;

public sealed record CompiledPage(string Title, HtmlNode Html) {
    public static readonly CompiledPage EMPTY = new("", HtmlNode.CreateNode("<main></main>"));

    public static CompiledPage FromRawHtml(string title, string html) {
        return new CompiledPage(title, HtmlNode.CreateNode($"<main>{html}</main>"));
    }
}
