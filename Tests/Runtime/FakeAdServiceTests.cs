using NUnit.Framework;

namespace AdsMediation.Tests
{
    public class FakeAdServiceTests
    {
        [Test]
        public void ShowInterstitial_CountsTheAd_WhenReady()
        {
            var ads = new FakeAdService();

            ads.ShowInterstitial();

            Assert.AreEqual(1, ads.InterstitialsShown);
        }

        [Test]
        public void ShowInterstitial_DoesNothing_WhenNotReady()
        {
            var ads = new FakeAdService { IsInterstitialReady = false };

            ads.ShowInterstitial();

            Assert.AreEqual(0, ads.InterstitialsShown);
        }

        [Test]
        public void ShowRewarded_GrantsTheReward_OnlyWhenReady()
        {
            var ads = new FakeAdService();
            int rewards = 0;

            ads.ShowRewarded(() => rewards++);
            Assert.AreEqual(1, rewards);

            ads.IsRewardedReady = false;
            ads.ShowRewarded(() => rewards++);
            Assert.AreEqual(1, rewards);
        }

        [Test]
        public void ShowBanner_RecordsVisibilityAndPosition()
        {
            var ads = new FakeAdService();

            ads.ShowBanner(BannerPosition.TopLeft);
            Assert.IsTrue(ads.IsBannerVisible);
            Assert.AreEqual(BannerPosition.TopLeft, ads.BannerPosition);

            ads.HideBanner();
            Assert.IsFalse(ads.IsBannerVisible);
        }

        [Test]
        public void ShowBanner_DefaultsToBottomCenter()
        {
            var ads = new FakeAdService();

            ads.ShowBanner(BannerPosition.TopLeft);
            ads.ShowBanner();

            Assert.AreEqual(BannerPosition.BottomCenter, ads.BannerPosition);
        }
    }
}
