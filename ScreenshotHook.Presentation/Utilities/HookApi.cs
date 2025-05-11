using System.Runtime.InteropServices;

namespace ScreenshotHook.Presentation.Utilities
{
    public static class HookApi
    {
        [DllImport("ScreenshotHook.Injector.dll", EntryPoint = "Hook")]
        public static extern bool Hook(int processId, bool is64Bit, string watermark);

        [DllImport("ScreenshotHook.Injector.dll", EntryPoint = "UnHook")]
        public static extern bool UnHook(int processId, bool is64Bit);
    }
}