namespace Unity.Services.LevelPlay
{
    public static class AdsExtensions
    {
        public static LevelPlayBannerPosition ToBannerPosition(this BannerPositionType positionType)
        {
            return positionType switch
            {
                BannerPositionType.TopLeft => LevelPlayBannerPosition.TopLeft,
                BannerPositionType.TopCenter => LevelPlayBannerPosition.TopCenter,
                BannerPositionType.TopRight => LevelPlayBannerPosition.TopRight,
                BannerPositionType.CenterLeft => LevelPlayBannerPosition.CenterLeft,
                BannerPositionType.Center => LevelPlayBannerPosition.Center,
                BannerPositionType.CenterRight => LevelPlayBannerPosition.CenterRight,
                BannerPositionType.BottomLeft => LevelPlayBannerPosition.BottomLeft,
                BannerPositionType.BottomCenter => LevelPlayBannerPosition.BottomCenter,
                BannerPositionType.BottomRight => LevelPlayBannerPosition.BottomRight,
                _ => LevelPlayBannerPosition.BottomCenter
            };
        }
    }
}