using System;
using System.Diagnostics;

namespace ScreenshotHook.Framework
{
    public class Utilities
    {
        public static bool Is64BitProcess(Process process)
        {
            if (!Environment.Is64BitOperatingSystem)
            {
                // 如果操作系统是 32 位，所有进程都是 32 位
                return false;
            }

            bool isWow64; // 是否是 WOW64 进程（32位进程运行在64位系统上）

            // 如果是 64 位操作系统，但是进程是 32 位的，那么它一定是 WOW64 进程
            if (!Win32.IsWow64Process(process.Handle, out isWow64))
            {
                // 如果调用失败，那么就是 64 位进程
                return true;
            }

            return !isWow64;
        }
    }
}