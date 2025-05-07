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
        private const string UNHOOK_COMMAND = "UNHOOK_COMMAND"; // 卸载钩子标识

        private static string DllPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScreenshotHook.HookLibrary.dll");

        private static bool LoadDll()
        {
            var dllDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dlls");

            if (!Directory.Exists(dllDir))
            {
                ShowError("The dll directory does not exist");
                return false;
            }

            string dllName = Environment.Is64BitProcess ? "EasyHook64.dll" : "EasyHook32.dll";
            IntPtr hModule = Win32.LoadLibrary(Path.Combine(dllDir, dllName));

            if (hModule == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                ShowError("Failed to load the dll: " + error);
                return false;
            }

            return true;
        }

        private static bool CheckPlatform(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);

                if (process == null)
                {
                    ShowError($"id: {processId} is not running!");
                    return false;
                }

                var currentPlat = Environment.Is64BitProcess ? 64 : 32;
                var targetPlat = Utilities.Is64BitProcess(process) ? 64 : 32;

                if (currentPlat != targetPlat)
                {
                    ShowError(string.Format($@"The current program is {currentPlat} bit, and the target process is {targetPlat} bit,
                        Please adjust the compilation options and try again!"));
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        [DllExport]
        public static void Hook(int processId, string watermark)
        {
            if (!LoadDll())
            {
                return;
            }

            if (!File.Exists(DllPath))
            {
                ShowError("The dll path is incorrect!");
                return;
            }

            if (!CheckPlatform(processId))
            {
                return;
            }

            try
            {
                RemoteHooking.Inject(
                    processId,
                    InjectionOptions.Default,
                    DllPath,
                    DllPath,
                    watermark
                );
            }
            catch (Exception ex)
            {
                ShowError($"Process ID: {processId}: Injection failed: {ex}");
                return;
            }

            //MessageBox.Show("Injection successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [DllExport]
        public static void UnHook(int processId)
        {
            try
            {
                if (!LoadDll())
                {
                    return;
                }

                if (!File.Exists(DllPath))
                {
                    ShowError("The dll path is incorrect!");
                    return;
                }

                if (!CheckPlatform(processId))
                {
                    return;
                }

                try
                {
                    RemoteHooking.Inject(
                        processId,
                        InjectionOptions.Default,
                        DllPath,
                        DllPath,
                        UNHOOK_COMMAND
                    );

                    //MessageBox.Show("UnHook successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    ShowError($"Process ID: {processId}: UnHook failed: {ex}");
                }
            }
            catch (Exception ex)
            {
                ShowError("UnHook failed: " + ex.Message);
            }
        }

        private static void ShowError(string text)
        {
            MessageBox.Show(text, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}