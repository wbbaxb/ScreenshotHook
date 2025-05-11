using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace ScreenshotHook.Presentation.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
    }
}