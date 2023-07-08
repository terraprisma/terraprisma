using Spectre.Console.Cli;
using Terraprisma.Ssg.Compile;

namespace Terraprisma.Ssg
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var app = new CommandApp();
            app.Configure(config =>
            {
                config.SetApplicationName("terraprisma-ssg");
                config.AddCommand<GenerateCommand>("generate");
            });

            return app.Run(args);
        }
    }
}