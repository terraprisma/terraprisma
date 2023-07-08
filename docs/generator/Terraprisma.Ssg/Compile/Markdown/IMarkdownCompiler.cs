using System;

namespace Terraprisma.Ssg.Compile.Markdown
{
    public interface IMarkdownCompiler
    {
        public string[] FileExtensions { get => new string[] { "md" }; }
    }
}