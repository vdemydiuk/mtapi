namespace MtApi5
{
    public enum CopyTicksFlag
    {
        Info    = 1, // ticks with Bid and/or Ask changes
        Trade   = 2, // ticks with changes in Last and Volume
        All     = -1 // all ticks
    }
}