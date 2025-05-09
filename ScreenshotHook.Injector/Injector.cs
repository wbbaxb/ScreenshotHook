using EasyHook;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace ScreenshotHook.Injector
{
    public class Injector
    {
        private const string UNHOOK_COMMAND = "UNHOOK_COMMAND"; // 卸载钩子标识

        [DllExport]
        public static bool Hook(int processId, string watermark)
        {
            string dllPath = GetDllPath(processId);

            if (!File.Exists(dllPath))
            {
                ShowError("The dll path is incorrect!");
                return false;
            }

            try
            {
                RemoteHooking.Inject(
                    processId,
                    InjectionOptions.Default,
                    dllPath,
                    dllPath,
                    watermark
                );

                return true;
            }
            catch (Exception ex)
            {
                ShowError($"Process ID: {processId}: Injection failed: {ex}");
                return false;
            }
        }

        [DllExport]
        public static bool UnHook(int processId)
        {
            string dllPath = GetDllPath(processId);

            if (!File.Exists(dllPath))
            {
                ShowError("The dll path is incorrect!");
                return false;
            }

            try
            {
                RemoteHooking.Inject(
                    processId,
                    InjectionOptions.Default,
                    dllPath,
                    dllPath,
                    UNHOOK_COMMAND
                );

                return true;
            }
            catch (Exception ex)
            {
                ShowError($"Process ID: {processId}: UnHook failed: {ex}");
                return false;
            }
        }

        private static string GetDllPath(int processId)
        {
            var process = Process.GetProcessById(processId);

            if (process == null)
            {
                return string.Empty;
            }

            bool is64 = Utilities.Is64BitProcess(process);

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, is64 ? "x64" : "x86", "ScreenshotHook.HookLibrary.dll");
        }

        private static void ShowError(string text)
        {
            MessageBox.Show(text, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}