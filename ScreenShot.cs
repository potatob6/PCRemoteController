using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NvidiaStuUpdater
{
    class ScreenShot
    {
        public Bitmap captureScreen()
        {
            Size screensize = Screen.PrimaryScreen.Bounds.Size;
            int width = screensize.Width;
            int height = screensize.Height;
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(0, 0, 0, 0, new Size(width, height));
            }

            return bmp;
        }
    }
}
