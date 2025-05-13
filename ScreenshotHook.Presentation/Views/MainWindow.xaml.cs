using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace ScreenshotHook.Presentation.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int WM_USER = 0x0400; // 0x0400 是用户定义的消息起始值
        public const int WM_RESTOREWINDOW = WM_USER + 1; // 自定义消息，用于恢复窗口

        public MainWindow()
        {
            InitializeComponent();

            //解决Win11菜单位置显示不正确的情况
            MyNotifyIcon.ContextMenu.Opened += (s, e) =>
            {
                Win32.GetCursorPos(out Win32.POINT point);

                PresentationSource source = PresentationSource.FromVisual(this);

                MyNotifyIcon.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
                MyNotifyIcon.ContextMenu.HorizontalOffset = point.X / source.CompositionTarget.TransformToDevice.M11;
                MyNotifyIcon.ContextMenu.VerticalOffset = point.Y / source.CompositionTarget.TransformToDevice.M22;
            };
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Hide();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        /// <summary>
        /// 捕获自定义消息（显示并置顶窗口）
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            if (source != null)
            {
                source.AddHook(WndProc);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_RESTOREWINDOW:
                    Show();
                    WindowState = WindowState.Normal;
                    // 通过先置顶再取消置顶的方式，将窗口带到前台并激活
                    Topmost = true;
                    Topmost = false;
                    break;
            }

            return IntPtr.Zero;
        }
    }
}