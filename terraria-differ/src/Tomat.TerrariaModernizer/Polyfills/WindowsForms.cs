using System.Runtime.InteropServices;

namespace System.Windows.Forms;

public static class Clipboard {
    public static void SetText(string text) {
        throw new NotImplementedException();
    }

    public static string GetText() {
        throw new NotImplementedException();
    }
}

public interface IMessageFilter {
    bool PreFilterMessage(ref Message m);
}

[StructLayout(LayoutKind.Sequential)]
public struct Message {
    public nint HWnd;
    public int Msg;
    public nint WParam;
    public nint LParam;
    public nint Result;

    public static Message Create(nint hWnd, int msg, nint wParam, nint lParam) {
        return new Message {
            HWnd = hWnd,
            Msg = msg,
            WParam = wParam,
            LParam = lParam,
            Result = nint.Zero,
        };
    }
}

public static class Application {
    public static void AddMessageFilter(IDisposable _) {
        // no-op
    }
}
