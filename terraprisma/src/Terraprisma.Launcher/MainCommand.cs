using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace Terraprisma.Launcher; 

/// <summary>
///     The main command (no name), which handles launching into the Nightglow
///     GUI.
/// </summary>
[Command]
public class MainCommand : ICommand {
    public ValueTask ExecuteAsync(IConsole console) {
        throw new System.NotImplementedException();
    }
}
