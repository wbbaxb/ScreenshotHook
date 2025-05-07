using EasyHook;
using Newtonsoft.Json;
using ScreenshotHook.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace ScreenshotHook.HookLibrary
{
    public class MainHook : IEntryPoint
    {
        private static List<LocalHook> _hooks = new List<LocalHook>();
        private static readonly object _lock = new object();

        private Watermark _watermarkData;
        private LocalHook _hook;
        private bool _shouldUnhook = false;
        private const string UNHOOK_COMMAND = "UNHOOK_COMMAND";

        public MainHook(RemoteHooking.IContext context, string watermarkJson)
        {
            if (watermarkJson == UNHOOK_COMMAND)
            {
                _shouldUnhook = true;
                return;
            }

            _watermarkData = JsonConvert.DeserializeObject<Watermark>(watermarkJson);
        }

        public void Run(RemoteHooking.IContext context, string watermarkJson)
        {
            if (_shouldUnhook)
            {
                UninstallAllHooks();
                return;
            }

            try
            {
                _hook = LocalHook.Create(LocalHook.GetProcAddress("gdi32.dll", "BitBlt"),
                    new Win32.BitBltDelegate(BitBlt_Hooked),
                    this);

                _hook.ThreadACL.SetExclusiveACL(new int[] { 0 });

                lock (_lock)
                {
                    _hooks.Add(_hook);
                }

                RemoteHooking.WakeUpProcess();

                // 等待，直到进程结束或钩子被卸载
                while (true)
                {
                    Thread.Sleep(1000);

                    lock (_lock)
                    {
                        if (!_hooks.Contains(_hook))
                        {
                            // 钩子已被卸载，退出循环
                            break;
                        }
                    }
                }
            }
            catch
            {
                CleanupHook();
            }
        }

        /// <summary>
        /// 卸载所有钩子
        /// </summary>
        private void UninstallAllHooks()
        {
            lock (_lock)
            {
                foreach (var hook in _hooks)
                {
                    try
                    {
                        hook.Dispose();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("卸载钩子失败: " + ex.Message);
                    }
                }

                _hooks.Clear();
            }
        }

        /// <summary>
        /// 卸载单个钩子
        /// </summary>
        private void CleanupHook()
        {
            if (_hook != null)
            {
                lock (_lock)
                {
                    try
                    {
                        _hooks.Remove(_hook);
                        _hook.Dispose();
                    }
                    finally
                    {
                        _hook = null;
                    }
                }
            }
        }

        // 劫持后的 BitBlt
        private bool BitBlt_Hooked(IntPtr hdcDest, int xDest, int yDest, int w, int h, IntPtr hdcSrc, int xSrc, int ySrc, int rop)
        {
            bool ret = Win32.BitBlt(hdcDest, xDest, yDest, w, h, hdcSrc, xSrc, ySrc, rop);

            int screenWidth = Screen.AllScreens.Sum(x => x.Bounds.Width);
            int screenHeight = Screen.AllScreens.Sum(x => x.Bounds.Height);

            if (w != screenWidth && h != screenHeight)
            {
                return ret;
            }

            var dt = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            var text = _watermarkData.Text + " " + dt;

            using (var graphics = Graphics.FromHdc(hdcDest))
            {
                Font font = new Font(_watermarkData.FontName, _watermarkData.FontSize, (FontStyle)_watermarkData.FontStyle);
                Color color = Color.FromArgb(_watermarkData.ColorA, _watermarkData.ColorR, _watermarkData.ColorG, _watermarkData.ColorB);

                SizeF textSize = graphics.MeasureString(text, font);
                float textWidth = textSize.Width;
                float textHeight = textSize.Height;

                graphics.TranslateTransform(w / 2, h / 2);
                graphics.RotateTransform(-46.5f);

                int stepX = (int)(textWidth + 130);
                int stepY = (int)(textHeight + 160);

                for (int y = -h; y < h; y += stepY)
                {
                    for (int x = -w; x < w; x += stepX)
                    {
                        graphics.DrawString(text, font, new SolidBrush(color), x, y);
                    }
                }

                graphics.ResetTransform();
            }

            return ret;
        }
    }
}