using System;

namespace Terraprisma.Ssg.Compile
{
    public interface ICompiler
    {
        public string[] FileExtensions { get; }
    }
}