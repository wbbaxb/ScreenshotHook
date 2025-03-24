using EasyHook;
using Framework;
using System.Drawing;

namespace HookLibrary
{
    public class MainHook : IEntryPoint
    {
        private string _watermark;

        public MainHook(RemoteHooking.IContext context, string watermark)
        {
            _watermark = watermark;
        }

        public void Run(RemoteHooking.IContext context, string watermark)
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

            var text = _watermark + " " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            using (var graphics = Graphics.FromHdc(hdcDest))
            {
                // 1) 测量文本大小，用于确定水印间隔
                SizeF textSize = graphics.MeasureString(text, new Font("宋体", 20, FontStyle.Regular));
                float textWidth = textSize.Width;
                float textHeight = textSize.Height;

                // 2) 平移 + 旋转，使后续文字呈对角斜
                //    先把坐标原点移到屏幕中心，再旋转 -46.5 度
                graphics.TranslateTransform(w / 2, h / 2);
                graphics.RotateTransform(-46.5f);

                // 3) 在大的坐标范围内循环绘制同样的文字
                //    让水印覆盖到整个图片区域
                //    “-bitmap.Width” 到 “bitmap.Width” 是为了保证
                //    在旋转坐标系中，四角也都有水印
                int stepX = (int)(textWidth + 130);   // 两列水印之间的水平间隔
                int stepY = (int)(textHeight + 160);  // 两行水印之间的垂直间隔

                for (int y = -h; y < h; y += stepY)
                {
                    for (int x = -w; x < w; x += stepX)
                    {
                        graphics.DrawString(text,
                            new Font("宋体", 20, FontStyle.Regular),
                            new SolidBrush(Color.FromArgb(130, Color.Gray)),
                            x, y);
                    }
                }

                // 4) 重置坐标变换，避免影响后面其它绘制
                graphics.ResetTransform();
            }

            return ret;
        }
    }
}