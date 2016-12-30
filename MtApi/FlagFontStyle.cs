using System;

namespace MtApi
{
    [Flags]
    public enum FlagFontStyle
    {
        //Flags for specifying font style
        FONT_ITALIC     = -2147483648,
        FONT_UNDERLINE  = 1073741824,
        FONT_STRIKEOUT  = 536870912,

        //Flags for specifying font width
        FW_DONTCARE     = 0,
        FW_THIN         = 1,
        FW_EXTRALIGHT   = 2,
        FW_ULTRALIGHT   = 3,
        FW_LIGHT        = 4,
        FW_NORMAL       = 5,
        FW_REGULAR      = 6,
        FW_MEDIUM       = 7,
        FW_SEMIBOLD     = 8,
        FW_DEMIBOLD     = 9,
        FW_BOLD         = 10,
        FW_EXTRABOLD    = 11,
        FW_ULTRABOLD    = 12,
        FW_HEAVY        = 13,
        FW_BLACK        = 14
    }
}
