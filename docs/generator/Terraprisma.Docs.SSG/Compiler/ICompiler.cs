using Terraprisma.Docs.SSG.Configuration;

namespace Terraprisma.Docs.SSG.Compiler {
    /// <summary>
    ///     Handles the compilation of a documentation namespace to a set of
    ///     HTML files.
    /// </summary>
    public interface ICompiler {
        /// <summary>
        ///     Compiles the given <see cref="CompilationNamespace"/>
        ///     (<paramref name="ns"/>) to a map of HTML files in the
        ///     <paramref name="context"/> of a <see cref="CompilationContext"/>
        ///     instance.
        /// </summary>
        /// <param name="context">The <see cref="CompilationContext"/>.</param>
        /// <param name="ns">The <see cref="CompilationNamespace"/>.</param>
        /// <returns>
        ///     A map of relative URL paths to the bodies of HTML documents.
        ///     <br />
        ///     The key is a lowercase relative URI (<c>example.text.file</c>)
        ///     which points to the body of an HTML document, minus the
        ///     wrapping <c>body</c> tags.
        /// </returns>
        Dictionary<string, string> Compile(CompilationContext context, CompilationNamespace ns);
    }
}
