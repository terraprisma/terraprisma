using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using GLib;
using Terraprisma.Nightglow.UI;
using Application = Gtk.Application;

namespace Terraprisma.Nightglow.Commands;

/// <summary>
///     The main (default) command, which launches into the Nightglow GUI.
/// </summary>
[Command]
public sealed class MainCommand : BaseCommand {
    protected override ValueTask ExecuteAsync(IConsole console) {
        Application.Init();

        var app = new Application("dev.tomat.terraprisma.nightglow", ApplicationFlags.None);
        app.Register(Cancellable.Current);

        var win = new MainWindow("Nightglow");
        app.AddWindow(win);

        win.Show();
        Application.Run();

        return default;
    }
}
