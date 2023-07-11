using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace Terraprisma.Nightglow.Commands;

/// <summary>
///     The base command, which all commands inherit from. Contains some
///     universal options.
/// </summary>
public abstract class BaseCommand : ICommand {
    /// <summary>
    ///     Whether to suppress message boxes and select default prompt options.
    ///     Designed to ignore user input in environments where the user is not
    ///     there.
    /// </summary>
    [CommandOption("quiet", 'q', Description = "Whether to suppress message boxes and select default prompt options.")]
    public bool Quiet { get; set; }

    async ValueTask ICommand.ExecuteAsync(IConsole console) {
        await ExecuteAsync(console);
    }

    protected abstract ValueTask ExecuteAsync(IConsole console);
}
