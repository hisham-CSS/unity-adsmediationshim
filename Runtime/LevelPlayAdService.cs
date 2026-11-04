using System;
using System.Threading.Tasks;
using Unity.Services.LevelPlay;
using UnityEngine;

namespace AdsMediation
{
    /// <summary>
    /// The only class that talks to the LevelPlay SDK. It starts LevelPlay, keeps ads loaded, and
    /// translates LevelPlay's types and events into the IAdService contract. LevelPlay does the
    /// mediation itself, auctioning each ad across the networks on its dashboard.
    /// </summary>
    public sealed class LevelPlayAdService : IAdService
    {
        private const int RetryDelayMilliseconds = 10000;

        private readonly AdConfig.PlatformIds ids;
        private readonly bool openTestSuite;

        private LevelPlayInterstitialAd interstitial;
        private LevelPlayRewardedAd rewarded;
        private LevelPlayBannerAd banner;
        private BannerPosition bannerPosition;
        private bool bannerLoaded;
        private bool bannerWanted;
        private Action onRewarded;
        private bool started;
        private bool disposed;

        public LevelPlayAdService(AdConfig.PlatformIds ids, bool openTestSuite = false)
        {
            this.ids = ids;
            this.openTestSuite = openTestSuite;
        }

        public bool IsInterstitialReady => interstitial != null && interstitial.IsAdReady();
        public bool IsRewardedReady => rewarded != null && rewarded.IsAdReady();

        /// <summary>Starts LevelPlay. Ads begin loading once it reports success.</summary>
        public void Initialize()
        {
            if (string.IsNullOrEmpty(ids.appKey))
            {
                Debug.LogError("[Ads] AdConfig has no app key for this platform, so ads are off.");
                return;
            }

            var developerSettings = Resources.Load<LevelPlayMediationSettings>("LevelPlayMediationSettings");
            if (developerSettings != null && developerSettings.EnableIronsourceSDKInitAPI)
                Debug.LogWarning("[Ads] Turn off Automatic Initialization in LevelPlay's Developer Settings. This service starts LevelPlay itself.");

            if (openTestSuite) LevelPlay.SetMetaData("is_test_suite", "enable");

            LevelPlay.OnInitSuccess += HandleInitSuccess;
            LevelPlay.OnInitFailed += HandleInitFailed;
            LevelPlay.Init(ids.appKey);
        }

        public void ShowBanner(BannerPosition position = BannerPosition.BottomCenter)
        {
            bannerWanted = true;

            if (banner == null || position != bannerPosition)
            {
                // LevelPlay fixes a banner's position when it is created, so moving one means
                // replacing it. Callers never need to know that.
                bannerPosition = position;
                DestroyBanner();
                if (started) CreateBanner();
            }
            else if (bannerLoaded)
            {
                banner.ShowAd();
            }
        }

        public void HideBanner()
        {
            bannerWanted = false;
            if (bannerLoaded) banner.HideAd();
        }

        public void ShowInterstitial()
        {
            if (IsInterstitialReady) interstitial.ShowAd();
            else Debug.LogWarning("[Ads] No interstitial is ready to show.");
        }

        public void ShowRewarded(Action onRewarded)
        {
            if (!IsRewardedReady)
            {
                Debug.LogWarning("[Ads] No rewarded ad is ready to show.");
                return;
            }

            this.onRewarded = onRewarded;
            rewarded.ShowAd();
        }

        public void Dispose()
        {
            disposed = true;
            LevelPlay.OnInitSuccess -= HandleInitSuccess;
            LevelPlay.OnInitFailed -= HandleInitFailed;

            DestroyBanner();
            interstitial?.DestroyAd();
            rewarded?.DestroyAd();
            interstitial = null;
            rewarded = null;
        }

        private void HandleInitSuccess(LevelPlayConfiguration configuration)
        {
            LevelPlay.OnInitSuccess -= HandleInitSuccess;
            LevelPlay.OnInitFailed -= HandleInitFailed;
            started = true;
            Debug.Log("[Ads] LevelPlay started");

            if (!string.IsNullOrEmpty(ids.interstitialId)) CreateInterstitial();
            if (!string.IsNullOrEmpty(ids.rewardedId)) CreateRewarded();
            if (bannerWanted) CreateBanner(); // Banners load only once the game asks for one

            if (openTestSuite) LevelPlay.LaunchTestSuite();
        }

        private void HandleInitFailed(LevelPlayInitError error)
        {
            Debug.LogWarning($"[Ads] LevelPlay failed to start ({error}). Trying again soon.");
            RetryLater(() => LevelPlay.Init(ids.appKey));
        }

        private void CreateBanner()
        {
            if (string.IsNullOrEmpty(ids.bannerId))
            {
                Debug.LogWarning("[Ads] AdConfig has no banner id for this platform.");
                return;
            }

            var config = new LevelPlayBannerAd.Config.Builder()
                .SetSize(LevelPlayAdSize.BANNER)
                .SetPosition(ToLevelPlay(bannerPosition))
                .SetDisplayOnLoad(false) // ShowBanner and HideBanner decide when it appears
                .Build();

            banner = new LevelPlayBannerAd(ids.bannerId, config);
            banner.OnAdLoaded += HandleBannerLoaded;
            banner.OnAdLoadFailed += HandleBannerLoadFailed;
            banner.LoadAd();
        }

        private void DestroyBanner()
        {
            banner?.DestroyAd();
            banner = null;
            bannerLoaded = false;
        }

        // Banners refresh themselves after the first load, and fire these events each time.
        private void HandleBannerLoaded(LevelPlayAdInfo info)
        {
            if (bannerLoaded) return;

            LogLoaded("Banner", info);
            bannerLoaded = true;
            if (bannerWanted) banner.ShowAd();
        }

        private void HandleBannerLoadFailed(LevelPlayAdError error)
        {
            if (bannerLoaded) return;

            LogLoadFailed("Banner", error);
            var failedBanner = banner;
            RetryLater(() =>
            {
                if (banner == failedBanner) banner?.LoadAd(); // Unless it has been moved since
            });
        }

        private static LevelPlayBannerPosition ToLevelPlay(BannerPosition position)
        {
            switch (position)
            {
                case BannerPosition.TopLeft: return LevelPlayBannerPosition.TopLeft;
                case BannerPosition.TopCenter: return LevelPlayBannerPosition.TopCenter;
                case BannerPosition.TopRight: return LevelPlayBannerPosition.TopRight;
                case BannerPosition.CenterLeft: return LevelPlayBannerPosition.CenterLeft;
                case BannerPosition.Center: return LevelPlayBannerPosition.Center;
                case BannerPosition.CenterRight: return LevelPlayBannerPosition.CenterRight;
                case BannerPosition.BottomLeft: return LevelPlayBannerPosition.BottomLeft;
                case BannerPosition.BottomRight: return LevelPlayBannerPosition.BottomRight;
                default: return LevelPlayBannerPosition.BottomCenter;
            }
        }

        private void CreateInterstitial()
        {
            interstitial = new LevelPlayInterstitialAd(ids.interstitialId);
            interstitial.OnAdLoaded += info => LogLoaded("Interstitial", info);
            interstitial.OnAdLoadFailed += error =>
            {
                LogLoadFailed("Interstitial", error);
                RetryLater(() => interstitial?.LoadAd());
            };
            interstitial.OnAdDisplayFailed += (info, error) =>
            {
                Debug.LogWarning($"[Ads] Interstitial failed to show ({error}).");
                interstitial?.LoadAd();
            };
            interstitial.OnAdClosed += info => interstitial?.LoadAd();
            interstitial.LoadAd();
        }

        private void CreateRewarded()
        {
            rewarded = new LevelPlayRewardedAd(ids.rewardedId);
            rewarded.OnAdLoaded += info => LogLoaded("Rewarded", info);
            rewarded.OnAdLoadFailed += error =>
            {
                LogLoadFailed("Rewarded", error);
                RetryLater(() => rewarded?.LoadAd());
            };
            rewarded.OnAdDisplayFailed += (info, error) =>
            {
                Debug.LogWarning($"[Ads] Rewarded ad failed to show ({error}).");
                rewarded?.LoadAd();
            };
            rewarded.OnAdRewarded += (info, reward) => GrantReward();
            rewarded.OnAdClosed += info => rewarded?.LoadAd();
            rewarded.LoadAd();
        }

        private void GrantReward()
        {
            var grant = onRewarded;
            onRewarded = null;
            grant?.Invoke();
        }

        // async void on purpose: if a retry throws, the error reaches the Unity console instead
        // of vanishing with a Task nobody is waiting on.
        private async void RetryLater(Action retry)
        {
            await Task.Delay(RetryDelayMilliseconds);
            if (!disposed && Application.isPlaying) retry();
        }

        private static void LogLoaded(string format, LevelPlayAdInfo info)
        {
            Debug.Log($"[Ads] {format} loaded from {info.AdNetwork}");
        }

        private static void LogLoadFailed(string format, LevelPlayAdError error)
        {
            Debug.LogWarning($"[Ads] {format} failed to load ({error}). Trying again soon.");
        }
    }
}
