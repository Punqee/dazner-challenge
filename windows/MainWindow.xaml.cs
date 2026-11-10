using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DAZNer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int WH_MOUSE_LL = 14;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_SIZING = 0x0214;
        private const int WM_ENTERSIZEMOVE = 0x0231;
        private const int WM_EXITSIZEMOVE = 0x0232;
        private const int WMSZ_LEFT = 1;
        private const int WMSZ_RIGHT = 2;
        private const int WMSZ_TOP = 3;
        private const int WMSZ_TOPLEFT = 4;
        private const int WMSZ_TOPRIGHT = 5;
        private const int WMSZ_BOTTOM = 6;
        private const int WMSZ_BOTTOMLEFT = 7;
        private const int WMSZ_BOTTOMRIGHT = 8;
        private const int VK_MENU = 0x12;
        private const uint GA_ROOT = 2;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(POINT Point);

        [DllImport("user32.dll")]
        private static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern uint GetDoubleClickTime();

        [DllImport("user32.dll")]
        private static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

        private IntPtr _windowHandle = IntPtr.Zero;
        private IntPtr _hookId = IntPtr.Zero;
        private LowLevelMouseProc? _hookProc;

        private int _paneCount = 1;
        private bool _isDragging = false;
        private bool _isResizing = false;
        private bool _webViewDisabledForResize = false;
        private POINT _dragStartCursor;
        private RECT _dragStartWindowRect;
        private long _lastClickTime = 0;
        private POINT _lastClickPoint;

        private double AspectRatio => (16.0 * _paneCount) / 9.0;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            _windowHandle = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(_windowHandle)?.AddHook(WndProc);

            _hookProc = HookCallback;
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            _hookId = SetWindowsHookEx(WH_MOUSE_LL, _hookProc, GetModuleHandle(curModule?.ModuleName), 0);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }
            base.OnClosed(e);
        }

        private void SetWebViewEnabled(bool enabled)
        {
            bool shouldDisable = !enabled;
            if (_webViewDisabledForResize == shouldDisable)
            {
                return;
            }

            IntPtr wvHandle = webView.Handle;
            if (wvHandle != IntPtr.Zero)
            {
                EnableWindow(wvHandle, enabled);
                _webViewDisabledForResize = shouldDisable;
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_ENTERSIZEMOVE)
            {
                _isResizing = true;
            }
            else if (msg == WM_EXITSIZEMOVE)
            {
                _isResizing = false;
                SetWebViewEnabled(true);
            }
            else if (msg == WM_SIZING)
            {
                int edge = wParam.ToInt32();
                RECT rect = Marshal.PtrToStructure<RECT>(lParam);
                double ratio = AspectRatio;
                int minWidth = 320 * _paneCount;
                int minHeight = (int)Math.Round(minWidth / ratio);

                int width = rect.right - rect.left;
                int height = rect.bottom - rect.top;

                switch (edge)
                {
                    case WMSZ_LEFT:
                    case WMSZ_RIGHT:
                        if (width < minWidth)
                        {
                            if (edge == WMSZ_LEFT)
                            {
                                rect.left = rect.right - minWidth;
                            }
                            else
                            {
                                rect.right = rect.left + minWidth;
                            }
                            width = minWidth;
                        }
                        rect.bottom = rect.top + (int)Math.Round(width / ratio);
                        break;

                    case WMSZ_TOP:
                    case WMSZ_BOTTOM:
                        if (height < minHeight)
                        {
                            if (edge == WMSZ_TOP)
                            {
                                rect.top = rect.bottom - minHeight;
                            }
                            else
                            {
                                rect.bottom = rect.top + minHeight;
                            }
                            height = minHeight;
                        }
                        rect.right = rect.left + (int)Math.Round(height * ratio);
                        break;

                    case WMSZ_TOPLEFT:
                    case WMSZ_TOPRIGHT:
                        if (width < minWidth)
                        {
                            if (edge == WMSZ_TOPLEFT)
                            {
                                rect.left = rect.right - minWidth;
                            }
                            else
                            {
                                rect.right = rect.left + minWidth;
                            }
                            width = minWidth;
                        }
                        rect.top = rect.bottom - (int)Math.Round(width / ratio);
                        break;

                    case WMSZ_BOTTOMLEFT:
                    case WMSZ_BOTTOMRIGHT:
                        if (width < minWidth)
                        {
                            if (edge == WMSZ_BOTTOMLEFT)
                            {
                                rect.left = rect.right - minWidth;
                            }
                            else
                            {
                                rect.right = rect.left + minWidth;
                            }
                            width = minWidth;
                        }
                        rect.bottom = rect.top + (int)Math.Round(width / ratio);
                        break;
                }

                Marshal.StructureToPtr(rect, lParam, false);
                handled = true;
                return (IntPtr)1;
            }

            return IntPtr.Zero;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && _windowHandle != IntPtr.Zero)
            {
                int msg = wParam.ToInt32();

                if (_isDragging)
                {
                    if (msg == WM_MOUSEMOVE)
                    {
                        var moveStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                        int dx = moveStruct.pt.x - _dragStartCursor.x;
                        int dy = moveStruct.pt.y - _dragStartCursor.y;
                        SetWindowPos(
                            _windowHandle,
                            IntPtr.Zero,
                            _dragStartWindowRect.left + dx,
                            _dragStartWindowRect.top + dy,
                            0,
                            0,
                            SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
                    }
                    else if (msg == WM_LBUTTONUP)
                    {
                        _isDragging = false;
                        return (IntPtr)1;
                    }
                }
                else if (msg == WM_MOUSEMOVE && !_isResizing)
                {
                    var moveStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                    bool onBorder = false;

                    if (WindowState == WindowState.Normal && GetWindowRect(_windowHandle, out RECT winRect))
                    {
                        int relX = moveStruct.pt.x - winRect.left;
                        int relY = moveStruct.pt.y - winRect.top;
                        int winW = winRect.right - winRect.left;
                        int winH = winRect.bottom - winRect.top;

                        if (relX >= 0 && relX < winW && relY >= 0 && relY < winH)
                        {
                            double dpiScale = GetDpiForWindow(_windowHandle) / 96.0;
                            if (dpiScale <= 0)
                            {
                                dpiScale = 1.0;
                            }

                            int topResize = (int)Math.Round(4 * dpiScale);
                            int sideResize = (int)Math.Round(6 * dpiScale);

                            if (relX < sideResize || relX >= winW - sideResize ||
                                relY < topResize || relY >= winH - sideResize)
                            {
                                bool isAltDown = (GetAsyncKeyState(VK_MENU) & 0x8000) != 0;
                                if (!isAltDown)
                                {
                                    onBorder = true;
                                }
                            }
                        }
                    }

                    SetWebViewEnabled(!onBorder);
                }
                else if (msg == WM_LBUTTONDOWN)
                {
                    var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                    IntPtr hwndAtPt = WindowFromPoint(hookStruct.pt);

                    if (hwndAtPt != IntPtr.Zero && GetAncestor(hwndAtPt, GA_ROOT) == _windowHandle)
                    {
                        var pt = hookStruct.pt;
                        ScreenToClient(_windowHandle, ref pt);
                        GetClientRect(_windowHandle, out RECT clientRect);

                        if (pt.x >= 0 && pt.x < clientRect.right && pt.y >= 0 && pt.y < clientRect.bottom)
                        {
                            double dpiScale = GetDpiForWindow(_windowHandle) / 96.0;
                            if (dpiScale <= 0)
                            {
                                dpiScale = 1.0;
                            }

                            int topResize = (int)Math.Round(4 * dpiScale);
                            int sideResize = (int)Math.Round(6 * dpiScale);
                            int stripBottom = (int)Math.Round(18 * dpiScale);

                            bool inTopStrip = pt.y >= topResize && pt.y < stripBottom &&
                                              pt.x >= sideResize && pt.x < (clientRect.right - sideResize);
                            bool isAltDown = (GetAsyncKeyState(VK_MENU) & 0x8000) != 0;

                            if (inTopStrip)
                            {
                                SetWebViewEnabled(true);
                                long now = Environment.TickCount64;
                                uint doubleClickTime = GetDoubleClickTime();
                                bool isDoubleClick = (now - _lastClickTime <= doubleClickTime) &&
                                                     Math.Abs(pt.x - _lastClickPoint.x) <= 6 &&
                                                     Math.Abs(pt.y - _lastClickPoint.y) <= 6;
                                _lastClickTime = now;
                                _lastClickPoint = pt;

                                if (isDoubleClick)
                                {
                                    _lastClickTime = 0;
                                    _isDragging = false;
                                    Dispatcher.InvokeAsync(() =>
                                    {
                                        WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
                                    });
                                    return (IntPtr)1;
                                }

                                SetForegroundWindow(_windowHandle);
                                if (WindowState != WindowState.Maximized && GetWindowRect(_windowHandle, out _dragStartWindowRect))
                                {
                                    _dragStartCursor = hookStruct.pt;
                                    _isDragging = true;
                                    return (IntPtr)1;
                                }
                            }
                            else if (isAltDown)
                            {
                                SetWebViewEnabled(true);
                                SetForegroundWindow(_windowHandle);
                                if (WindowState != WindowState.Maximized && GetWindowRect(_windowHandle, out _dragStartWindowRect))
                                {
                                    _dragStartCursor = hookStruct.pt;
                                    _isDragging = true;
                                    return (IntPtr)1;
                                }
                            }
                        }
                    }
                }
            }
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }
    }
}
