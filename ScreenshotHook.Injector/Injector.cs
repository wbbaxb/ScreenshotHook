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
        public static bool Hook(int processId, string watermark)
        {
            if (!LoadDll())
            {
                return false;
            }

            if (!File.Exists(DllPath))
            {
                ShowError("The dll path is incorrect!");
                return false;
            }

            if (!CheckPlatform(processId))
            {
                return false;
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
            try
            {
                if (!LoadDll())
                {
                    return false;
                }

                if (!File.Exists(DllPath))
                {
                    ShowError("The dll path is incorrect!");
                    return false;
                }

                if (!CheckPlatform(processId))
                {
                    return false;
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

                    return true;
                }
                catch (Exception ex)
                {
                    ShowError($"Process ID: {processId}: UnHook failed: {ex}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                ShowError("UnHook failed: " + ex.Message);
                return false;
            }
        }

        private static void ShowError(string text)
        {
            MessageBox.Show(text, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}