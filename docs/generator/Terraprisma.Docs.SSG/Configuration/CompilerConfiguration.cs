using Newtonsoft.Json;

namespace Terraprisma.Docs.SSG.Configuration; 

public sealed class CompilerConfiguration {
    [JsonProperty("projects")]
    public Dictionary<string, CompilationProject> Projects { get; } = new();
}
