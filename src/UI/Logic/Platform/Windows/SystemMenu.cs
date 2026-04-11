using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.Platform.Windows;

public static class SystemMenu
{
    #region Win32 Constants

    private const uint WM_SYSCOMMAND = 0x0112;
    private const uint WM_MEASUREITEM = 0x002C;
    private const uint WM_DRAWITEM = 0x002B;

    private const uint MF_STRING = 0x00000000;
    private const uint MF_SEPARATOR = 0x00000800;
    private const uint MF_GRAYED = 0x00000001;
    private const uint MF_OWNERDRAW = 0x00000100;

    private const uint TPM_LEFTALIGN = 0x0000;
    private const uint TPM_RETURNCMD = 0x0100;
    private const uint TPM_LEFTBUTTON = 0x0000;

    private const uint SC_RESTORE = 0xF120;
    private const uint SC_MINIMIZE = 0xF020;
    private const uint SC_MAXIMIZE = 0xF030;
    private const uint SC_CLOSE = 0xF060;
    private const uint SC_MOVE = 0xF010;
    private const uint SC_SIZE = 0xF000;

    private const int ODS_SELECTED = 0x0001;
    private const int ODS_GRAYED = 0x0004;

    private const int DT_LEFT = 0x00000000;
    private const int DT_SINGLELINE = 0x00000020;
    private const int DT_VCENTER = 0x00000004;

    #endregion

    #region Win32 Structs

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X, Y; }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }

    [StructLayout(LayoutKind.Sequential)]
    private struct MEASUREITEMSTRUCT
    {
        public uint CtlType, CtlID;
        public uint itemID;
        public uint itemWidth, itemHeight;
        public IntPtr itemData;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DRAWITEMSTRUCT
    {
        public uint CtlType, CtlID;
        public uint itemID;
        public uint itemAction, itemState;
        public IntPtr hwndItem, hDC;
        public RECT rcItem;
        public IntPtr itemData;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WNDCLASSEX
    {
        public uint cbSize, style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra, cbWndExtra;
        public IntPtr hInstance, hIcon, hCursor, hbrBackground;
        public IntPtr lpszMenuName, lpszClassName, hIconSm;
    }

    #endregion

    #region Win32 Imports

    [DllImport("user32.dll")] private static extern bool GetCursorPos(out POINT p);
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr w, IntPtr l);
    [DllImport("user32.dll")] private static extern IntPtr CreatePopupMenu();
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool AppendMenu(IntPtr hMenu, uint uFlags, IntPtr uIDNewItem, string lpNewItem);
    [DllImport("user32.dll")] private static extern bool DestroyMenu(IntPtr hMenu);
    [DllImport("user32.dll")] private static extern uint TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int n, IntPtr hWnd, IntPtr r);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr CreateWindowEx(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int x, int y, int w, int h, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);
    [DllImport("user32.dll")] private static extern bool DestroyWindow(IntPtr hWnd);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern bool UnregisterClass(string lpClassName, IntPtr hInstance);
    [DllImport("user32.dll")] private static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr w, IntPtr l);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr GetModuleHandle(string? lpModuleName);
    [DllImport("gdi32.dll")] private static extern IntPtr CreateSolidBrush(uint crColor);
    [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr hObject);
    [DllImport("gdi32.dll")] private static extern int SetBkMode(IntPtr hdc, int mode);
    [DllImport("gdi32.dll")] private static extern uint SetTextColor(IntPtr hdc, uint color);
    [DllImport("gdi32.dll")] private static extern uint SetBkColor(IntPtr hdc, uint color);
    [DllImport("gdi32.dll")] private static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);
    [DllImport("gdi32.dll")] private static extern IntPtr GetStockObject(int fnObject);
    [DllImport("user32.dll")] private static extern bool FillRect(IntPtr hdc, ref RECT r, IntPtr hbr);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int DrawText(IntPtr hdc, string s, int n, ref RECT r, uint fmt);

    #endregion

    #region Theme

    private struct MenuTheme
    {
        public uint Background;   // COLORREF (0x00BBGGRR)
        public uint Highlight;
        public uint TextNormal;
        public uint TextDisabled;
        public uint Separator;
    }

    private static MenuTheme DarkTheme => new()
    {
        Background = ToCOLORREF(0x2D2D2D),
        Highlight = ToCOLORREF(0x4A4A4A),
        TextNormal = ToCOLORREF(0xE8E8E8),
        TextDisabled = ToCOLORREF(0x707070),
        Separator = ToCOLORREF(0x505050),
    };

    private static MenuTheme LightTheme => new()
    {
        Background = ToCOLORREF(0xF5F5F5),
        Highlight = ToCOLORREF(0xD0E4F7),
        TextNormal = ToCOLORREF(0x1A1A1A),
        TextDisabled = ToCOLORREF(0xAAAAAA),
        Separator = ToCOLORREF(0xD0D0D0),
    };

    // RGB hex → Win32 COLORREF (BBGGRR byte order)
    private static uint ToCOLORREF(uint rgb) =>
        ((rgb & 0xFF) << 16) | (rgb & 0xFF00) | ((rgb >> 16) & 0xFF);

    private static bool IsDarkMode(Window window)
    {
        var variant = window.ActualThemeVariant;
        return variant == Avalonia.Styling.ThemeVariant.Dark;
    }

    #endregion

    #region Menu Items

    private record MenuItem(string Label, uint Command, bool Enabled, bool IsSeparator = false);

    private static MenuItem[] BuildItems(Window window)
    {
        bool maximized = window.WindowState == WindowState.Maximized;
        bool minimized = window.WindowState == WindowState.Minimized;
        bool normal = window.WindowState == WindowState.Normal;

        return
        [
            new("Restore",   SC_RESTORE,  maximized || minimized),
            new("Move",      SC_MOVE,     normal),
            new("Size",      SC_SIZE,     normal),
            new("Minimize",  SC_MINIMIZE, !minimized),
            new("Maximize",  SC_MAXIMIZE, !maximized),
            new("",          0,           false, IsSeparator: true),
            new("Close (Alt+F4)", SC_CLOSE, true),
        ];
    }

    #endregion

    // Shared state during a single Show() call — safe because we're single-threaded on UIThread
    private static MenuTheme _theme;
    private static MenuItem[] _items = [];

    public static void Show(Window window)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;

        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => Show(window));
            return;
        }

        var platformHandle = window.TryGetPlatformHandle();
        if (platformHandle == null) return;

        var hwnd = platformHandle.Handle;
        _theme = IsDarkMode(window) ? DarkTheme : LightTheme;
        _items = BuildItems(window);

        GetCursorPos(out var pt);
        SetForegroundWindow(hwnd);

        Dispatcher.UIThread.Post(() =>
        {
            var ownerHwnd = CreateOwnerWindow(hwnd);
            if (ownerHwnd == IntPtr.Zero) return;

            var menu = BuildOwnerDrawMenu();
            if (menu == IntPtr.Zero) { DestroyOwnerWindow(ownerHwnd); return; }

            try
            {
                var cmd = TrackPopupMenu(menu, TPM_LEFTALIGN | TPM_RETURNCMD | TPM_LEFTBUTTON,
                    pt.X, pt.Y, 0, ownerHwnd, IntPtr.Zero);

                if (cmd != 0)
                    PostMessage(hwnd, WM_SYSCOMMAND, (IntPtr)cmd, IntPtr.Zero);
            }
            finally
            {
                DestroyMenu(menu);
                DestroyOwnerWindow(ownerHwnd);
            }
        }, DispatcherPriority.Background);
    }

    #region Owner-draw window

    private static readonly string OwnerClassName = "SE_MenuOwner_" + Guid.NewGuid().ToString("N");
    private static IntPtr _ownerHwnd;
    private static ushort _ownerClassAtom;

    private static IntPtr CreateOwnerWindow(IntPtr parent)
    {
        var hInstance = GetModuleHandle(null);
        var wndProc = Marshal.GetFunctionPointerForDelegate((WndProcDelegate)OwnerWndProc);

        var wc = new WNDCLASSEX
        {
            cbSize = (uint)Marshal.SizeOf<WNDCLASSEX>(),
            lpfnWndProc = wndProc,
            hInstance = hInstance,
            lpszClassName = Marshal.StringToHGlobalUni(OwnerClassName),
        };

        _ownerClassAtom = RegisterClassEx(ref wc);
        Marshal.FreeHGlobal(wc.lpszClassName);

        _ownerHwnd = CreateWindowEx(0, OwnerClassName, "", 0, 0, 0, 0, 0, parent, IntPtr.Zero, hInstance, IntPtr.Zero);
        return _ownerHwnd;
    }

    private static void DestroyOwnerWindow(IntPtr hwnd)
    {
        DestroyWindow(hwnd);
        UnregisterClass(OwnerClassName, GetModuleHandle(null));
    }

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    private static IntPtr OwnerWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_MEASUREITEM)
        {
            var mis = Marshal.PtrToStructure<MEASUREITEMSTRUCT>(lParam);
            var item = _items[mis.itemID];
            mis.itemHeight = item.IsSeparator ? 8u : 24u;
            mis.itemWidth = 180u;
            Marshal.StructureToPtr(mis, lParam, false);
            return (IntPtr)1;
        }

        if (msg == WM_DRAWITEM)
        {
            var dis = Marshal.PtrToStructure<DRAWITEMSTRUCT>(lParam);
            DrawMenuItem(dis);
            return (IntPtr)1;
        }

        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    #endregion

    #region Drawing

    private static void DrawMenuItem(DRAWITEMSTRUCT dis)
    {
        var item = _items[dis.itemID];
        bool selected = (dis.itemState & ODS_SELECTED) != 0;
        bool grayed = !item.Enabled;
        var rc = dis.rcItem;

        // Background
        var bgColor = selected && !grayed ? _theme.Highlight : _theme.Background;
        var hbr = CreateSolidBrush(bgColor);
        FillRect(dis.hDC, ref rc, hbr);
        DeleteObject(hbr);

        if (item.IsSeparator)
        {
            // Draw a thin horizontal line in the centre of the separator rect
            var midY = (rc.Top + rc.Bottom) / 2;
            var sepRc = new RECT { Left = rc.Left + 8, Top = midY, Right = rc.Right - 8, Bottom = midY + 1 };
            var sepBrush = CreateSolidBrush(_theme.Separator);
            FillRect(dis.hDC, ref sepRc, sepBrush);
            DeleteObject(sepBrush);
            return;
        }

        // Text
        var textColor = grayed ? _theme.TextDisabled : _theme.TextNormal;
        SetBkMode(dis.hDC, 1); // TRANSPARENT
        SetTextColor(dis.hDC, textColor);

        var textRc = new RECT { Left = rc.Left + 28, Top = rc.Top, Right = rc.Right - 8, Bottom = rc.Bottom };
        DrawText(dis.hDC, item.Label, -1, ref textRc, DT_LEFT | DT_SINGLELINE | DT_VCENTER);
    }

    #endregion

    #region Build menu

    private static IntPtr BuildOwnerDrawMenu()
    {
        var hMenu = CreatePopupMenu();
        if (hMenu == IntPtr.Zero) return IntPtr.Zero;

        for (uint i = 0; i < (uint)_items.Length; i++)
        {
            var item = _items[i];
            uint flags = MF_OWNERDRAW;
            if (!item.Enabled) flags |= MF_GRAYED;
            if (item.IsSeparator) flags |= MF_SEPARATOR;

            // Use the array index as the item ID so WM_MEASUREITEM / WM_DRAWITEM can look it up
            AppendMenu(hMenu, flags, (IntPtr)i, item.Label);
        }

        return hMenu;
    }

    #endregion
}