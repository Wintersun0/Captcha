using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CaptchaTest
{
    internal class MessageBoxHelper
    {
        private static IWin32Window _owner;
        private static HookProc _hookProc;
        private static IntPtr _hHook;

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, IWin32Window owner)
        {
            _owner = owner;

            _hookProc = new HookProc(MessageBoxHookProc);
            _hHook = SetWindowsHookEx(WH_CBT, _hookProc, IntPtr.Zero, GetCurrentThreadId());

            return MessageBox.Show(owner, text, caption, buttons, icon);
        }

        private static IntPtr MessageBoxHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode == HCBT_ACTIVATE)
            {
                CenterMessageBox(wParam);
                UnhookWindowsHookEx(_hHook);
            }
            return CallNextHookEx(_hHook, nCode, wParam, lParam);
        }

        private static void CenterMessageBox(IntPtr hWnd)
        {
            RECT rectOwner;
            GetWindowRect(_owner.Handle, out rectOwner);

            RECT rectMessageBox;
            GetWindowRect(hWnd, out rectMessageBox);

            int width = rectMessageBox.Right - rectMessageBox.Left;
            int height = rectMessageBox.Bottom - rectMessageBox.Top;

            int newLeft = rectOwner.Left + (rectOwner.Right - rectOwner.Left - width) / 2;
            int newTop = rectOwner.Top + (rectOwner.Bottom - rectOwner.Top - height) / 2;

            SetWindowPos(hWnd, (IntPtr)0, newLeft, newTop, 0, 0, SWP_NOZORDER | SWP_NOSIZE);
        }

        private const int WH_CBT = 5;
        private const int HCBT_ACTIVATE = 5;

        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOSIZE = 0x0001;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}