using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Web.WebView2.Wpf;

namespace DAZNer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int WH_MOUSE_LL = 14;
        private const int WM_MOVE = 0x0003;
        private const int WM_SIZE = 0x0005;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_SIZING = 0x0214;
        private const int WM_ENTERSIZEMOVE = 0x0231;
        private const int WM_EXITSIZEMOVE = 0x0232;
        private const int WM_DPICHANGED = 0x02E0;
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
        private const uint MONITOR_DEFAULTTONEAREST = 2;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const string HomeUrl = "https://www.dazn.com/";

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

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
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

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        private IntPtr _windowHandle = IntPtr.Zero;
        private IntPtr _hookId = IntPtr.Zero;
        private LowLevelMouseProc? _hookProc;
        private WebView2? _secondWebView;

        private int _paneCount = 1;
        private int _focusedPane = 0;
        private bool _isDragging = false;
        private bool _isResizing = false;
        private bool _isNormalState = true;
        private bool _webViewDisabledForResize = false;
        private RECT _cachedWindowRect;
        private int _cachedTopResize = 4;
        private int _cachedSideResize = 6;
        private int _cachedStripBottom = 18;
        private POINT _dragStartCursor;
        private RECT _dragStartWindowRect;
        private long _lastClickTime = 0;
        private POINT _lastClickPoint;

        private double AspectRatio => (16.0 * _paneCount) / 9.0;

        public MainWindow()
        {
            InitializeComponent();
            ConfigureWebView(webView, 0);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            _windowHandle = new WindowInteropHelper(this).Handle;
            UpdateCachedWindowMetrics();
            HwndSource.FromHwnd(_windowHandle)?.AddHook(WndProc);

            _hookProc = HookCallback;
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            _hookId = SetWindowsHookEx(WH_MOUSE_LL, _hookProc, GetModuleHandle(curModule?.ModuleName), 0);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            _isNormalState = WindowState == WindowState.Normal;
            UpdateCachedWindowMetrics();
        }

        private void UpdateCachedWindowMetrics()
        {
            if (_windowHandle == IntPtr.Zero)
            {
                return;
            }

            GetWindowRect(_windowHandle, out _cachedWindowRect);
            double dpiScale = GetDpiForWindow(_windowHandle) / 96.0;
            if (dpiScale <= 0)
            {
                dpiScale = 1.0;
            }

            _cachedTopResize = (int)Math.Round(4 * dpiScale);
            _cachedSideResize = (int)Math.Round(6 * dpiScale);
            _cachedStripBottom = (int)Math.Round(18 * dpiScale);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }

            if (_secondWebView != null)
            {
                _secondWebView.Dispose();
                _secondWebView = null;
            }

            webView.Dispose();
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
            }

            if (_secondWebView != null)
            {
                IntPtr secondHandle = _secondWebView.Handle;
                if (secondHandle != IntPtr.Zero)
                {
                    EnableWindow(secondHandle, enabled);
                }
            }

            _webViewDisabledForResize = shouldDisable;
        }

        private WebView2? GetPane(int index)
        {
            if (index == 0)
            {
                return webView;
            }
            if (index == 1)
            {
                return _secondWebView;
            }
            return null;
        }

        private void ConfigureWebView(WebView2 pane, int index)
        {
            pane.GotFocus += (_, _) =>
            {
                if (_focusedPane != index)
                {
                    FocusPane(index);
                }
            };

            void AttachCoreEvents()
            {
                if (pane.CoreWebView2 == null)
                {
                    return;
                }

                pane.CoreWebView2.NewWindowRequested += (_, args) =>
                {
                    args.Handled = true;
                    if (!string.IsNullOrWhiteSpace(args.Uri))
                    {
                        pane.CoreWebView2.Navigate(args.Uri);
                    }
                };

                SyncAudio();
            }

            if (pane.CoreWebView2 != null)
            {
                AttachCoreEvents();
            }
            else
            {
                pane.CoreWebView2InitializationCompleted += (_, args) =>
                {
                    if (args.IsSuccess)
                    {
                        AttachCoreEvents();
                    }
                };
            }
        }

        public void FocusPane(int index)
        {
            if (index < 0 || index >= _paneCount)
            {
                index = 0;
            }

            _focusedPane = index;
            GetPane(index)?.Focus();
            SyncAudio();
        }

        public void NextPane()
        {
            if (_paneCount > 1)
            {
                FocusPane((_focusedPane + 1) % _paneCount);
            }
        }

        private void SyncAudio()
        {
            if (_paneCount <= 1)
            {
                if (webView.CoreWebView2 != null)
                {
                    webView.CoreWebView2.IsMuted = false;
                }
                return;
            }

            for (int i = 0; i < _paneCount; i++)
            {
                var pane = GetPane(i);
                if (pane?.CoreWebView2 != null)
                {
                    pane.CoreWebView2.IsMuted = (i != _focusedPane);
                }
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_MOVE || msg == WM_SIZE || msg == WM_DPICHANGED)
            {
                UpdateCachedWindowMetrics();
            }
            else if (msg == WM_ENTERSIZEMOVE)
            {
                _isResizing = true;
            }
            else if (msg == WM_EXITSIZEMOVE)
            {
                _isResizing = false;
                UpdateCachedWindowMetrics();
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
                    bool onBorder = false;

                    if (_isNormalState)
                    {
                        var moveStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                        int relX = moveStruct.pt.x - _cachedWindowRect.left;
                        int relY = moveStruct.pt.y - _cachedWindowRect.top;
                        int winW = _cachedWindowRect.right - _cachedWindowRect.left;
                        int winH = _cachedWindowRect.bottom - _cachedWindowRect.top;

                        if (relX >= 0 && relX < winW && relY >= 0 && relY < winH)
                        {
                            if (relX < _cachedSideResize || relX >= winW - _cachedSideResize ||
                                relY < _cachedTopResize || relY >= winH - _cachedSideResize)
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
                            bool inTopStrip = pt.y >= _cachedTopResize && pt.y < _cachedStripBottom &&
                                              pt.x >= _cachedSideResize && pt.x < (clientRect.right - _cachedSideResize);
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
                            else if (_paneCount == 2)
                            {
                                int halfWidth = clientRect.right / 2;
                                int clickedPane = (pt.x >= halfWidth + 4) ? 1 : (pt.x < halfWidth - 4 ? 0 : _focusedPane);
                                if (clickedPane != _focusedPane)
                                {
                                    Dispatcher.InvokeAsync(() => FocusPane(clickedPane));
                                }
                            }
                        }
                    }
                }
            }
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        public void FillAndCenter()
        {
            if (_windowHandle == IntPtr.Zero)
            {
                return;
            }

            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }

            IntPtr hMonitor = MonitorFromWindow(_windowHandle, MONITOR_DEFAULTTONEAREST);
            if (hMonitor == IntPtr.Zero)
            {
                return;
            }

            var monitorInfo = new MONITORINFO
            {
                cbSize = Marshal.SizeOf<MONITORINFO>()
            };

            if (!GetMonitorInfo(hMonitor, ref monitorInfo))
            {
                return;
            }

            RECT work = monitorInfo.rcWork;
            int workWidth = work.right - work.left;
            int workHeight = work.bottom - work.top;
            if (workWidth <= 0 || workHeight <= 0)
            {
                return;
            }

            double ratio = AspectRatio;
            int targetWidth = (int)Math.Round(Math.Min((double)workWidth, workHeight * ratio));
            int targetHeight = (int)Math.Round(targetWidth / ratio);
            if (targetHeight > workHeight)
            {
                targetHeight = workHeight;
                targetWidth = (int)Math.Round(targetHeight * ratio);
            }

            int targetX = work.left + (workWidth - targetWidth) / 2;
            int targetY = work.top + (workHeight - targetHeight) / 2;

            SetWindowPos(
                _windowHandle,
                IntPtr.Zero,
                targetX,
                targetY,
                targetWidth,
                targetHeight,
                SWP_NOZORDER | SWP_NOACTIVATE);
        }

        public void ToggleAlwaysOnTop()
        {
            Topmost = !Topmost;
        }

        public void SetPaneCount(int count)
        {
            if (count == _paneCount || (count != 1 && count != 2))
            {
                return;
            }

            _paneCount = count;
            ResizeWindowForPanes(count);

            if (count == 2)
            {
                var pane1 = new WebView2
                {
                    DefaultBackgroundColor = System.Drawing.Color.Black,
                    Source = new Uri(HomeUrl)
                };
                ConfigureWebView(pane1, 1);
                Grid.SetColumn(pane1, 2);
                panesGrid.Children.Add(pane1);
                _secondWebView = pane1;

                colGap.Width = new GridLength(8);
                colPane1.Width = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                colGap.Width = new GridLength(0);
                colPane1.Width = new GridLength(0);

                if (_secondWebView != null)
                {
                    var paneToDispose = _secondWebView;
                    _secondWebView = null;
                    try
                    {
                        paneToDispose.CoreWebView2?.Stop();
                    }
                    catch
                    {
                    }
                    panesGrid.Children.Remove(paneToDispose);
                    paneToDispose.Dispose();
                }
            }

            if (_focusedPane >= count)
            {
                FocusPane(0);
            }
            else
            {
                SyncAudio();
            }
        }

        private void ResizeWindowForPanes(int count)
        {
            if (_windowHandle == IntPtr.Zero)
            {
                return;
            }

            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }

            if (!GetWindowRect(_windowHandle, out RECT oldRect))
            {
                return;
            }

            int oldWidth = oldRect.right - oldRect.left;
            int maxW = oldWidth * 2;
            RECT work = oldRect;
            bool hasMonitorWork = false;

            IntPtr hMonitor = MonitorFromWindow(_windowHandle, MONITOR_DEFAULTTONEAREST);
            if (hMonitor != IntPtr.Zero)
            {
                var monitorInfo = new MONITORINFO
                {
                    cbSize = Marshal.SizeOf<MONITORINFO>()
                };
                if (GetMonitorInfo(hMonitor, ref monitorInfo))
                {
                    work = monitorInfo.rcWork;
                    maxW = work.right - work.left;
                    hasMonitorWork = true;
                }
            }

            double ratio = AspectRatio;
            double scaledW = oldWidth * (count == 2 ? 2.0 : 0.5);
            int targetWidth = (int)Math.Round(Math.Min(Math.Max(scaledW, 640.0), maxW));
            int targetHeight = (int)Math.Round(targetWidth / ratio);

            int targetX = oldRect.left;
            int targetY = oldRect.top;

            if (hasMonitorWork)
            {
                if (targetX + targetWidth > work.right)
                {
                    targetX = Math.Max(work.left, work.right - targetWidth);
                }
                if (targetY + targetHeight > work.bottom)
                {
                    targetY = Math.Max(work.top, work.bottom - targetHeight);
                }
            }

            SetWindowPos(
                _windowHandle,
                IntPtr.Zero,
                targetX,
                targetY,
                targetWidth,
                targetHeight,
                SWP_NOZORDER | SWP_NOACTIVATE);
            UpdateCachedWindowMetrics();
        }

        public void OpenUrlDialog()
        {
            var activePane = GetPane(_focusedPane);
            if (activePane == null)
            {
                return;
            }

            string currentUrl = activePane.CoreWebView2?.Source ?? activePane.Source?.AbsoluteUri ?? HomeUrl;

            var dialog = new Window
            {
                Title = $"Abrir URL en el panel {_focusedPane + 1}",
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                Topmost = Topmost
            };

            var rootPanel = new StackPanel
            {
                Margin = new Thickness(16)
            };

            var label = new TextBlock
            {
                Text = $"Abrir URL en el panel {_focusedPane + 1}",
                Margin = new Thickness(0, 0, 0, 8)
            };

            var textBox = new TextBox
            {
                Width = 420,
                Text = currentUrl,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var openButton = new Button
            {
                Content = "Abrir",
                Width = 80,
                IsDefault = true,
                Margin = new Thickness(0, 0, 8, 0)
            };
            openButton.Click += (_, _) => dialog.DialogResult = true;

            var cancelButton = new Button
            {
                Content = "Cancelar",
                Width = 80,
                IsCancel = true
            };

            buttonPanel.Children.Add(openButton);
            buttonPanel.Children.Add(cancelButton);

            rootPanel.Children.Add(label);
            rootPanel.Children.Add(textBox);
            rootPanel.Children.Add(buttonPanel);
            dialog.Content = rootPanel;

            dialog.SourceInitialized += (_, _) =>
            {
                textBox.Focus();
                textBox.SelectAll();
            };

            if (dialog.ShowDialog() == true)
            {
                string input = textBox.Text.Trim();
                if (!string.IsNullOrEmpty(input))
                {
                    if (!input.Contains("://", StringComparison.Ordinal))
                    {
                        input = "https://" + input;
                    }

                    if (Uri.TryCreate(input, UriKind.Absolute, out Uri? targetUri))
                    {
                        if (activePane.CoreWebView2 != null)
                        {
                            activePane.CoreWebView2.Navigate(targetUri.AbsoluteUri);
                        }
                        else
                        {
                            activePane.Source = targetUri;
                        }
                    }
                }
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.D1 || e.Key == Key.NumPad1)
                {
                    SetPaneCount(1);
                    e.Handled = true;
                }
                else if (e.Key == Key.D2 || e.Key == Key.NumPad2)
                {
                    SetPaneCount(2);
                    e.Handled = true;
                }
                else if (e.Key == Key.Oem3 || e.Key == Key.Oem5 || e.Key == Key.Oem1 || e.Key == Key.Tab)
                {
                    NextPane();
                    e.Handled = true;
                }
                else if (e.Key == Key.L)
                {
                    OpenUrlDialog();
                    e.Handled = true;
                }
                else if (e.Key == Key.F)
                {
                    FillAndCenter();
                    e.Handled = true;
                }
                else if (e.Key == Key.T)
                {
                    ToggleAlwaysOnTop();
                    e.Handled = true;
                }
            }
        }
    }
}
