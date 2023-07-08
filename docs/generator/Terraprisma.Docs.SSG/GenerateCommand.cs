using System.ComponentModel;
using Spectre.Console.Cli;

namespace Terraprisma.Docs.SSG
{
    [Description("Generate a static site from inputs.")]
    public sealed class GenerateCommand : Command<GenerateCommand.Settings>
    {    
        public sealed class Settings : CommandSettings
        {
        }

        public override int Execute(CommandContext? context, Settings? settings)
        {
            return 0;
        }
    }
}