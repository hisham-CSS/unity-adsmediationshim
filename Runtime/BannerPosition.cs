namespace AdsMediation
{
    /// <summary>
    /// Where a banner sits on screen. This is our own type rather than LevelPlay's, so game code
    /// that places banners doesn't depend on LevelPlay.
    /// </summary>
    public enum BannerPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        CenterLeft,
        Center,
        CenterRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }
}
