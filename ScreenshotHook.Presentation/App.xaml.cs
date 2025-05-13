using System;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace ScreenshotHook.Presentation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Mutex mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 创建互斥体
            string mutexName = Assembly.GetExecutingAssembly().GetName().Name;
            bool createdNew;
            mutex = new Mutex(true, mutexName, out createdNew);

            if (createdNew)
            {
                // TODO:
            }
            else
            {
                //给已启动的程序发送自定义消息（显示并置顶窗口）
                var windowHandle = Win32.FindWindow(null, ViewModels.MainWindowViewModel.Title);
                Win32.SendMessage(windowHandle, Views.MainWindow.WM_RESTOREWINDOW, IntPtr.Zero, IntPtr.Zero);
                Environment.Exit(0);
            }
        }
    }
}