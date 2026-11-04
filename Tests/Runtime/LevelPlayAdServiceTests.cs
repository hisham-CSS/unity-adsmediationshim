#if UNITY_ANDROID || UNITY_IOS
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AdsMediation.Tests
{
    // Runs the real service through LevelPlay's own editor mock ads, which only exist when the
    // build target is Android or iOS. The mock never contacts LevelPlay, so these prove the
    // service's wiring, not that real ads fill.
    public class LevelPlayAdServiceTests
    {
        private const BindingFlags AnyInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private static readonly AdConfig.PlatformIds AllFormats = new AdConfig.PlatformIds
        {
            appKey = "test-key",
            bannerId = "test-banner",
            interstitialId = "test-interstitial",
            rewardedId = "test-rewarded"
        };

        private LevelPlayAdService ads;

        private void Start(AdConfig.PlatformIds ids)
        {
            ads = new LevelPlayAdService(ids);
            ads.Initialize();
        }

        [TearDown]
        public void TearDown() => ads?.Dispose();

        [Test]
        public void Ads_LoadOnceLevelPlayHasStarted()
        {
            Start(AllFormats);

            Assert.IsTrue(ads.IsInterstitialReady);
            Assert.IsTrue(ads.IsRewardedReady);
        }

        [Test]
        public void MissingAppKey_LeavesAdsOff()
        {
            LogAssert.Expect(LogType.Error, new Regex("no app key"));

            Start(new AdConfig.PlatformIds());

            Assert.IsFalse(ads.IsInterstitialReady);
            Assert.IsFalse(ads.IsRewardedReady);
        }

        [Test]
        public void EmptyId_LeavesThatFormatOff()
        {
            var ids = AllFormats;
            ids.rewardedId = "";

            Start(ids);

            Assert.IsTrue(ads.IsInterstitialReady);
            Assert.IsFalse(ads.IsRewardedReady);
        }

        [Test]
        public void ShowingAnAdThatIsNotReady_DoesNothing()
        {
            ads = new LevelPlayAdService(AllFormats); // Never started, so nothing has loaded
            bool rewarded = false;

            LogAssert.Expect(LogType.Warning, new Regex("No interstitial"));
            ads.ShowInterstitial();
            LogAssert.Expect(LogType.Warning, new Regex("No rewarded"));
            ads.ShowRewarded(() => rewarded = true);

            Assert.IsFalse(rewarded);
        }

        [Test]
        public void Interstitial_LoadsTheNextAd_AfterClosing()
        {
            Start(AllFormats);

            ads.ShowInterstitial();
            Assert.IsFalse(ads.IsInterstitialReady, "The ad should be used up once shown");

            CloseMockAd("InterstitialPrefab");

            Assert.IsTrue(ads.IsInterstitialReady);
        }

        [Test]
        public void Rewarded_GrantsTheReward_AndLoadsTheNextAd()
        {
            Start(AllFormats);
            int rewards = 0;

            ads.ShowRewarded(() => rewards++);
            Assert.AreEqual(0, rewards, "The reward should wait until the ad finishes");

            // LevelPlay's mock sends the reward after the close event, like some real networks.
            CloseMockAd("RewardedPrefab");

            Assert.AreEqual(1, rewards);
            Assert.IsTrue(ads.IsRewardedReady);
        }

        [Test]
        public void Banner_DoesNotLoad_UntilTheGameAsksForOne()
        {
            Start(AllFormats);

            Assert.IsEmpty(MockBanners());
        }

        [Test]
        public void ShowBanner_BeforeLevelPlayHasStarted_ShowsItOnceStarted()
        {
            ads = new LevelPlayAdService(AllFormats);

            ads.ShowBanner(BannerPosition.TopRight);
            Assert.IsEmpty(MockBanners());

            ads.Initialize();

            AssertSingleBannerShowingAt(BannerPosition.TopRight);
        }

        [Test]
        public void ShowBanner_MovesTheBanner_ToEveryPosition()
        {
            Start(AllFormats);

            foreach (BannerPosition position in Enum.GetValues(typeof(BannerPosition)))
            {
                ads.ShowBanner(position);

                AssertSingleBannerShowingAt(position);
            }
        }

        [Test]
        public void HideBanner_ThenShowBanner_KeepsTheSameBanner()
        {
            Start(AllFormats);
            ads.ShowBanner(BannerPosition.TopCenter);
            var banner = MockBanners().Single();

            ads.HideBanner();
            Assert.IsFalse(IsShowing(banner));

            ads.ShowBanner(BannerPosition.TopCenter);
            Assert.IsTrue(IsShowing(banner));
            Assert.AreSame(banner, MockBanners().Single());
        }

        private static void AssertSingleBannerShowingAt(BannerPosition position)
        {
            var banner = MockBanners().Single();
            Assert.IsTrue(IsShowing(banner));

            string mockPosition = FindField(banner, "m_BannerPosition").GetValue(banner).ToString();
            StringAssert.EndsWith($": {position}", mockPosition);
        }

        private static MonoBehaviour[] MockBanners()
        {
            return Resources.FindObjectsOfTypeAll<MonoBehaviour>()
                .Where(m => m.GetType().Name == "BannerPrefab" && m.gameObject.scene.IsValid())
                .ToArray();
        }

        private static bool IsShowing(MonoBehaviour mock) => (bool)FindField(mock, "m_Preview").GetValue(mock);

        // The mock's state isn't public, and some of it lives on its base class.
        private static FieldInfo FindField(object target, string name)
        {
            for (var type = target.GetType(); type != null; type = type.BaseType)
            {
                var field = type.GetField(name, AnyInstance);
                if (field != null) return field;
            }
            throw new MissingFieldException(target.GetType().Name, name);
        }

        // LevelPlay's mock has no public way to close an ad, so this calls what its Close button calls.
        private static void CloseMockAd(string mockType)
        {
            var mock = Resources.FindObjectsOfTypeAll<MonoBehaviour>()
                .First(m => m.GetType().Name == mockType && m.gameObject.scene.IsValid());

            mock.GetType().GetMethod("HideAd", AnyInstance).Invoke(mock, null);
        }
    }
}
#endif
