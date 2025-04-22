using System;
using System.Runtime.InteropServices;

namespace ScreenshotHook.Framework
{
    public class Win32
    {
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);

        #region 动态加载非托管Dll

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetDllDirectory(string lpPathName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void AddDllDirectory(string lpPathName);

        /// <summary>
        /// 搜索 AddDllDirectory 添加的路径
        /// </summary>
        public const uint LOAD_LIBRARY_SEARCH_USER_DIRS = 0x00000400;

        /// <summary>
        /// 搜索默认路径
        /// </summary>
        public const uint LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

        /// <summary>
        /// 加载指定路径的DLL文件
        /// 适用于简单的加载，不需要设置搜索路径
        /// </summary>
        /// <param name="lpFileName"></param>
        /// <returns>DLL模块的句柄</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        #endregion 动态加载非托管Dll

        #region GDI截图

        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int w, int h, IntPtr hdcSrc, int xSrc, int ySrc, int rop);

        //委托的参数和返回值必须与win32 api函数的参数和返回值一致
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool BitBltDelegate(IntPtr hdcDest, int xDest, int yDest, int w, int h, IntPtr hdcSrc, int xSrc, int ySrc, int rop);

        #endregion
    }
}