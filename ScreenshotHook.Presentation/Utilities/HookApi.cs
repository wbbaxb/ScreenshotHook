using System.Runtime.InteropServices;

namespace ScreenshotHook.Presentation.Utilities
{
    public static class HookApi
    {
        [DllImport("ScreenshotHook.Injector.dll", EntryPoint = "Hook")]
        public static extern void Hook(int processId,string watermark);
    }
}