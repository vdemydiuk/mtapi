namespace MtApi
{
    enum EnumColorFormat
    {
        COLOR_FORMAT_XRGB_NOALPHA       = 0, //The component of the alpha channel is ignored
        COLOR_FORMAT_ARGB_RAW           = 1, //Color components are not handled by the terminal (must be correctly set by the user)
        COLOR_FORMAT_ARGB_NORMALIZE     = 2  //Color components are handled by the terminal
    }
}
