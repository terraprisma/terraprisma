using System;

namespace Terraprisma.Docs.SSG.Compile.Markdown
{
    public interface IMarkdownCompiler
    {
        public string[] FileExtensions { get => new string[] { "md" }; }
    }
}