using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace System.Drawing;

public partial class Graphics {
    private readonly nint handle;

    private Graphics(nint handle) {
        this.handle = handle;
    }

    public nint GetHdc() {
        return GetDC(handle);
    }

    public static Graphics FromHwnd(nint handle) {
        return new Graphics(handle);
    }

    [LibraryImport("user32.dll")]
    private static partial nint GetDC(nint hWnd);
}
