using ScreenshotHook.Presentation.Enums;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ScreenshotHook.Presentation
{
    internal class Win32
    {
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        private static extern bool IsWow64Process([In] System.IntPtr hProcess, [Out] out bool wow64Process);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;

            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassrName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        public static Bit GetProcessBit(Process process)
        {
            if (!Environment.Is64BitOperatingSystem)
            {
                // 如果操作系统是 32 位，所有进程都是 32 位
                return Bit.Bit32;
            }

            bool isWow64; // 是否是 WOW64 进程（32位进程运行在64位系统上）

            // 如果是 64 位操作系统，但是进程是 32 位的，那么它一定是 WOW64 进程
            try
            {
                if (!IsWow64Process(process.Handle, out isWow64))
                {
                    // 如果调用失败，那么就是 64 位进程
                    return Bit.Bit64;
                }
            }
            catch
            {
                return Bit.Unknown;
            }

            return isWow64 ? Bit.Bit32 : Bit.Bit64;
        }
    }
}