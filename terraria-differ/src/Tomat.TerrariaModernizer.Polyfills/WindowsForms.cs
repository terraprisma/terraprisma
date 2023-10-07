/*using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System.Windows.Forms;

public static partial class Clipboard {
    // https://github.com/tModLoader/tModLoader/blob/1.4.4/patches/TerrariaNetCore/ReLogic/OS/Windows/NativeClipboard.cs
    private const uint cf_unicodetext = 13U;

    [LibraryImport("User32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsClipboardFormatAvailable(uint format);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool OpenClipboard(nint hWndNewOwner);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseClipboard();

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetClipboardData(uint uFormat, nint data);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial nint GetClipboardData(uint uFormat);

    [LibraryImport("Kernel32.dll", SetLastError = true)]
    private static partial nint GlobalLock(nint hMem);

    [LibraryImport("Kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GlobalUnlock(nint hMem);

    [LibraryImport("Kernel32.dll", SetLastError = true)]
    private static partial int GlobalSize(nint hMem);

    public static void SetText(string text) {
        OpenClipboard(nint.Zero);

        var ptr = Marshal.StringToHGlobalUni(text);

        SetClipboardData(13, ptr);
        CloseClipboard();
    }

    public static string GetText() {
        TryGetText(out var text);
        return text!;
    }

    public static bool TryGetText(out string? text) {
        text = null;

        if (!IsClipboardFormatAvailable(cf_unicodetext)) {
            return false;
        }

        try {
            if (!OpenClipboard(nint.Zero)) {
                return false;
            }

            var handle = GetClipboardData(cf_unicodetext);

            if (handle == nint.Zero) {
                return false;
            }

            var pointer = nint.Zero;

            try {
                pointer = GlobalLock(handle);

                if (pointer == nint.Zero) {
                    return false;
                }

                var size = GlobalSize(handle);
                var buff = new byte[size];

                Marshal.Copy(pointer, buff, 0, size);

                text = Encoding.Unicode.GetString(buff).TrimEnd('\0');

                return !string.IsNullOrEmpty(text);
            }
            finally {
                if (pointer != nint.Zero) {
                    GlobalUnlock(handle);
                }
            }
        }
        finally {
            CloseClipboard();
        }
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

    public static void RemoveMessageFilter(IMessageFilter _) {
        // no-op
    }
}
*/