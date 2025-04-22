using EasyHook;
using Newtonsoft.Json;
using ScreenshotHook.Framework;
using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace ScreenshotHook.HookLibrary
{
    public class MainHook : IEntryPoint
    {
        private Watermark _watermarkData;

        public MainHook(RemoteHooking.IContext context, string watermarkJson)
        {
            _watermarkData = JsonConvert.DeserializeObject<Watermark>(watermarkJson);
        }

        public void Run(RemoteHooking.IContext context, string watermarkJson)
        {
            var hook = LocalHook.Create(LocalHook.GetProcAddress("gdi32.dll", "BitBlt"),
                new Win32.BitBltDelegate(BitBlt_Hooked),
                this);

            hook.ThreadACL.SetExclusiveACL(new int[] { 0 });
            RemoteHooking.WakeUpProcess();
            Thread.Sleep(-1);
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
                // 使用反序列化的水印数据
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