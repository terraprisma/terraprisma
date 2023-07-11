using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace Terraprisma.Nightglow.Commands;

/// <summary>
///     The main (default) command, which launches into the Nightglow GUI.
/// </summary>
[Command]
public sealed class MainCommand : BaseCommand {
    protected override ValueTask ExecuteAsync(IConsole console) {
        throw new System.NotImplementedException();
    }
}
