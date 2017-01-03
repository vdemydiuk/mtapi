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

        public static int ConvertToMtColor(Color? color)
        {
            return color == null || color == Color.Empty ? 0xffffff : (Color.FromArgb(color.Value.B, color.Value.G, color.Value.R).ToArgb() & 0xffffff);
        }
    }
}
