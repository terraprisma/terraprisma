using Newtonsoft.Json;

namespace Terraprisma.Docs.SSG.Configuration; 

public sealed class CompilationProject {
    [JsonProperty("namespaces")]
    public Dictionary<string, CompilationNamespace> Namespaces { get; set; } = new();

    [JsonProperty("outputDir")]
    public string OutputDir { get; set; } = string.Empty;

    [JsonProperty("clearOutput")]
    public bool ClearOutput { get; set; }
}
