using System;
using UnityEngine;

namespace AdsMediation
{
    /// <summary>
    /// Stands in for real ads in tests and on build targets LevelPlay doesn't support. Ads are
    /// ready straight away and finish instantly. Set the ready flags to false to test what the
    /// game does when no ad is available.
    /// </summary>
    public sealed class FakeAdService : IAdService
    {
        public bool IsInterstitialReady { get; set; } = true;
        public bool IsRewardedReady { get; set; } = true;
        public bool IsBannerVisible { get; private set; }
        public BannerPosition BannerPosition { get; private set; } = BannerPosition.BottomCenter;
        public int InterstitialsShown { get; private set; }

        public void ShowBanner(BannerPosition position = BannerPosition.BottomCenter)
        {
            IsBannerVisible = true;
            BannerPosition = position;
            Debug.Log($"[Ads] Fake banner shown at {position}");
        }

        public void HideBanner()
        {
            IsBannerVisible = false;
            Debug.Log("[Ads] Fake banner hidden");
        }

        public void ShowInterstitial()
        {
            if (!IsInterstitialReady) return;

            InterstitialsShown++;
            Debug.Log("[Ads] Fake interstitial shown");
        }

        public void ShowRewarded(Action onRewarded)
        {
            if (!IsRewardedReady) return;

            Debug.Log("[Ads] Fake rewarded ad shown");
            onRewarded?.Invoke();
        }

        public void Dispose() { }
    }
}
