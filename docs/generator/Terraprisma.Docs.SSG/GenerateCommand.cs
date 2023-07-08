using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Newtonsoft.Json;
using Terraprisma.Docs.SSG.Compiler;
using Terraprisma.Docs.SSG.Configuration;

namespace Terraprisma.Docs.SSG {
    [Command("generate", Description = "Generates a static site from inputs")]
    public sealed class GenerateCommand : ICommand {
        [CommandParameter(0, Description = "The path to the configuration file")]
        public string ConfigFile { get; set; } = string.Empty;

        public async ValueTask ExecuteAsync(IConsole console) {
            if (!File.Exists(ConfigFile))
                throw new FileNotFoundException("The specified configuration file does not exist.", ConfigFile);

            var config = JsonConvert.DeserializeObject<CompilerConfiguration>(await File.ReadAllTextAsync(ConfigFile));
            if (config is null)
                throw new InvalidOperationException("The specified configuration file is invalid.");

            var configDir = Path.GetDirectoryName(ConfigFile);
            if (!string.IsNullOrEmpty(configDir))
                Directory.SetCurrentDirectory(Path.IsPathRooted(configDir) ? configDir : Path.Combine(Directory.GetCurrentDirectory(), configDir));

            var context = CompilationContext.MakeDefault(config);
            await console.Output.WriteLineAsync($"Compiling {config.Projects.Count} project(s)...");

            var hasFailures = false;

            foreach (var (projectName, project) in context.Config.Projects) {
                await console.Output.WriteLineAsync($"Compiling project '{projectName}'...");

                await console.Output.WriteLineAsync($"Using output directory '{project.OutputDir}'.");

                if (project.ClearOutput && Directory.Exists(project.OutputDir)) {
                    await console.Output.WriteLineAsync("Clearing (deleting) output directory...");
                    Directory.Delete(project.OutputDir, true);
                }

                Directory.CreateDirectory(project.OutputDir);

                try {
                    context.CompileProject(projectName);
                    await console.Output.WriteLineAsync($"Successfully compiled project '{projectName}'.");
                }
                catch (Exception e) {
                    await console.Output.WriteLineAsync($"Failed to compile project '{projectName}'.");
                    await console.Output.WriteLineAsync(e.ToString());
                    hasFailures = true;
                }
            }

            if (hasFailures)
                throw new Exception("One or more projects failed to compile.");
        }
    }
}
