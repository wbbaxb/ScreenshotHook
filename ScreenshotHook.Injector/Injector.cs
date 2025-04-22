using EasyHook;
using ScreenshotHook.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace ScreenshotHook.Injector
{
    public class Injector
    {
        [DllExport]
        public static void Hook(int processId, string watermark)
        {
            var dllDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dlls");

            if (!Directory.Exists(dllDir))
            {
                ShowError("The dll directory does not exist");
                return;
            }

            string dllName = Environment.Is64BitProcess ? "EasyHook64.dll" : "EasyHook32.dll";
            IntPtr hModule = Win32.LoadLibrary(Path.Combine(dllDir, dllName));

            if (hModule == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();

                if (error != 0)
                {
                    ShowError("Failed to set the dll path: " + error);
                    return;
                }
            }

            string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScreenshotHook.HookLibrary.dll");

            if (!File.Exists(dllPath))
            {
                ShowError("The dll path is incorrect!");
                return;
            }

            try
            {
                var process = Process.GetProcessById(processId);

                if (process == null)
                {
                    ShowError($"id: {processId} is not running!");
                    return;
                }

                var currentPlat = Environment.Is64BitProcess ? 64 : 32;
                var targetPlat = Utilities.Is64BitProcess(process) ? 64 : 32;

                if (currentPlat != targetPlat)
                {
                    ShowError(string.Format($@"The current program is {currentPlat} bit, and the target process is {targetPlat} bit,
                        Please adjust the compilation options and try again!"));
                    return;
                }

                try
                {
                    RemoteHooking.Inject(
                        process.Id,
                        InjectionOptions.Default,
                        dllPath,
                        dllPath,
                        watermark
                    );
                }
                catch (Exception ex)
                {
                    ShowError($"Process ID: {process.Id}: Injection failed: {ex}");
                    return;
                }

                MessageBox.Show("Injection successful!", "Success", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                ShowError("Injection failed: " + ex.Message);
            }
        }

        private static void ShowError(string text)
        {
            MessageBox.Show(text, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}