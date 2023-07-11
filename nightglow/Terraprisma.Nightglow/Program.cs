using System.Threading.Tasks;
using CliFx;

namespace Terraprisma.Nightglow;

internal static class Program {
    internal static async Task<int> Main(string[] args) {
        return await new CliApplicationBuilder().AddCommandsFromThisAssembly().Build().RunAsync(args);
    }
}
