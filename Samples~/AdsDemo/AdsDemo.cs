using System;
using UnityEngine;

namespace AdsMediation.Demo
{
    /// <summary>
    /// The demo's composition root: the one place that creates the ad service and hands it to the
    /// code that needs it. This class owns the service's lifetime, and nothing else in the demo
    /// knows which implementation it got.
    /// </summary>
    public class AdsDemo : MonoBehaviour
    {
        [SerializeField] private AdConfig config;
        [SerializeField] private int coinsPerAd = 50;

        private IAdService ads;
        private RewardedCoins coins;
        private BannerPosition bannerPosition = BannerPosition.BottomCenter;
        private bool bannerVisible;

        private void Awake()
        {
            ads = AdServices.Create(config);
            coins = new RewardedCoins(ads, coinsPerAd);
        }

        private void OnDestroy() => ads?.Dispose();

        private void OnGUI()
        {
            GUI.depth = 10; // Behind LevelPlay's editor mock ads, which also draw with IMGUI
            float scale = Mathf.Max(1f, Mathf.Min(Screen.width, Screen.height) / 480f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            GUILayout.BeginArea(new Rect(12f, 12f, 320f, 225f), GUI.skin.box);
            GUILayout.Label($"Ad service: {ads.GetType().Name}");
            GUILayout.Label($"Coins: {coins.Coins}");

            GUI.enabled = ads.IsInterstitialReady;
            if (Button(ads.IsInterstitialReady ? "Show Interstitial" : "Interstitial loading..."))
                ads.ShowInterstitial();

            GUI.enabled = coins.CanWatchAd;
            if (Button(coins.CanWatchAd ? $"Watch Ad for {coinsPerAd} Coins" : "Rewarded ad loading..."))
                coins.WatchAd();

            GUI.enabled = true;
            if (Button(bannerVisible ? "Hide Banner" : "Show Banner"))
                ToggleBanner();

            if (Button($"Banner Position: {bannerPosition}"))
                MoveBanner();

            GUILayout.EndArea();
        }

        private void ToggleBanner()
        {
            bannerVisible = !bannerVisible;
            if (bannerVisible) ads.ShowBanner(bannerPosition);
            else ads.HideBanner();
        }

        private void MoveBanner()
        {
            int positionCount = Enum.GetValues(typeof(BannerPosition)).Length;
            bannerPosition = (BannerPosition)(((int)bannerPosition + 1) % positionCount);
            if (bannerVisible) ads.ShowBanner(bannerPosition);
        }

        private static bool Button(string label) => GUILayout.Button(label, GUILayout.Height(36f));
    }
}
