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
            return Color.FromArgb(color);
        }

        public static int ConvertToMtColor(Color color)
        {
            return color == Color.Empty ? 0xffffff : (color.ToArgb() & 0xffffff);
        }
    }
}
