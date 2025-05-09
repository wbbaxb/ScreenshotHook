using System;
using System.Runtime.InteropServices;

namespace ScreenshotHook.HookLibrary
{
    internal class Win32
    {
        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int w, int h, IntPtr hdcSrc, int xSrc, int ySrc, int rop);

        //委托的参数和返回值必须与win32 api函数的参数和返回值一致
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool BitBltDelegate(IntPtr hdcDest, int xDest, int yDest, int w, int h, IntPtr hdcSrc, int xSrc, int ySrc, int rop);
    }
}