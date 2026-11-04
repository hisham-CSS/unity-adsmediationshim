using System;

namespace AdsMediation
{
    /// <summary>
    /// Everything game code needs from ads, and the only ads type it should depend on. Ads load
    /// themselves, so callers just check that one is ready and show it. Showing an ad that isn't
    /// ready does nothing, so a missing ad can never break gameplay.
    /// </summary>
    public interface IAdService : IDisposable
    {
        bool IsInterstitialReady { get; }
        bool IsRewardedReady { get; }

        /// <summary>
        /// Shows the banner at this position, now or as soon as one has loaded. Calling this again
        /// with a different position moves the banner.
        /// </summary>
        void ShowBanner(BannerPosition position = BannerPosition.BottomCenter);
        void HideBanner();

        void ShowInterstitial();

        /// <summary>
        /// Shows a rewarded ad. <paramref name="onRewarded"/> runs only if the player earns the
        /// reward, and it can arrive after the ad has closed.
        /// </summary>
        void ShowRewarded(Action onRewarded);
    }
}
