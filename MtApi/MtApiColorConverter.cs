using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MtApi
{
    class MtApiColorConverter
    {
        public static Color ConvertFromMtColor(int color)
        {
            return Color.FromArgb((byte)(color), (byte)(color >> 8), (byte)(color >> 16));
        }

        public static int ConvertToMtColor(Color color)
        {
            return color == Color.Empty ? 0xffffff : (Color.FromArgb(color.B, color.G, color.R).ToArgb() & 0xffffff);
        }
    }
}
