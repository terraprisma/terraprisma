using System;
using Gtk;

namespace Terraprisma.Nightglow.UI; 

internal sealed class MainWindow : Window {
    public MainWindow(IntPtr raw) : base(raw) { }

    public MainWindow(WindowType type) : base(type) { }

    public MainWindow(string title) : base(title) { }
}
