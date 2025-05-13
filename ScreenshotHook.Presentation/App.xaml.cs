using ScreenshotHook.Presentation.Utilities;
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
            LogHelper.Configure();
            base.OnStartup(e);

            // 创建互斥体
            string mutexName = Assembly.GetExecutingAssembly().GetName().Name;
            mutex = new Mutex(true, mutexName, out bool createdNew);

            if (createdNew)
            {
                //非UI线程中未处理的异常
                AppDomain.CurrentDomain.UnhandledException += (s, args) =>
                {
                    if (args.ExceptionObject is Exception ex)
                    {
                        LogHelper.Error(ex);
                        LogHelper.Info("程序即将异常关闭！");
                    }
                };

                //UI线程异常
                this.DispatcherUnhandledException += (s, args) =>
                {
                    LogHelper.Error(args.Exception);
                    args.Handled = true;
                };
            }
            else
            {
                LogHelper.Info("检测到实例已运行，退出当前实例。");
                //给已启动的程序发送自定义消息（显示并置顶窗口）
                var windowHandle = Win32.FindWindow(null, ViewModels.MainWindowViewModel.Title);
                Win32.SendMessage(windowHandle, Views.MainWindow.WM_RESTOREWINDOW, IntPtr.Zero, IntPtr.Zero);
                Environment.Exit(0);
            }
        }
    }
}