using Newtonsoft.Json;

namespace Terraprisma.Docs.SSG.Configuration;

public sealed class CompilationNamespace {
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("root")]
    public bool Root { get; set; }

    [JsonProperty("input")]
    public string Input { get; set; } = string.Empty;

    [JsonProperty("options")]
    public Dictionary<string, object> Options { get; set; } = new ();
}
