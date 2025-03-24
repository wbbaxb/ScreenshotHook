using EasyHook;
using Framework;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace ScreenshotHook
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Hook();
        }

        private void Hook()
        {
            var dllDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dlls");

            if (!Directory.Exists(dllDir))
            {
                MessageBox.Show("Dll目录不存在");
                return;
            }

            string dllName = Environment.Is64BitProcess ? "EasyHook64.dll" : "EasyHook32.dll";
            IntPtr hModule = Win32.LoadLibrary(Path.Combine(dllDir, dllName));

            if (hModule == IntPtr.Zero)
            {
                // LoadLibrary 失败，检查错误码
                int error = Marshal.GetLastWin32Error();

                if (error != 0)
                {
                    MessageBox.Show("Dll路径设置失败：" + error);
                    return;
                }
            }

            string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HookLibrary.dll");

            if (!File.Exists(dllPath))
            {
                MessageBox.Show("dll路径错误！");
                return;
            }

            try
            {
                string processName = "wechat";
                var processes = Process.GetProcessesByName(processName);

                if (processes.Length == 0)
                {
                    MessageBox.Show($"{processName} 未运行！");
                    return;
                }

                var process = processes[0];

                var currentPlat = Environment.Is64BitProcess ? 64 : 32;
                var targetPlat = Utilities.Is64BitProcess(process) ? 64 : 32;

                if (currentPlat != targetPlat)
                {
                    MessageBox.Show(string.Format("当前程序是{0}位程序，目标进程是{1}位程序，" +
                        "请调整编译选项重新编译后重试！", currentPlat, targetPlat));
                    return;
                }

                try
                {
                    RemoteHooking.Inject(
                        process.Id,
                        InjectionOptions.Default,
                        dllPath,
                        dllPath,
                        "test"
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"进程ID:{process.Id}:Hook失败:{ex.ToString()}");
                    return;
                }

                MessageBox.Show("Hook 成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show("注入失败: " + ex.Message);
            }
        }
    }
}