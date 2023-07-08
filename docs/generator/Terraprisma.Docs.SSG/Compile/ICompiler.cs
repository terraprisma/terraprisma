using System;

namespace Terraprisma.Docs.SSG.Compile
{
    public interface ICompiler
    {
        public string[] FileExtensions { get; }
    }
}