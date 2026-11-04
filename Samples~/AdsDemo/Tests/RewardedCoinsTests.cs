using NUnit.Framework;

namespace AdsMediation.Demo.Tests
{
    // The payoff of depending on IAdService: game logic is tested with a fake, and needs no
    // SDK, scene or device.
    public class RewardedCoinsTests
    {
        [Test]
        public void WatchAd_AddsCoins_WhenAnAdIsReady()
        {
            var coins = new RewardedCoins(new FakeAdService(), coinsPerAd: 50);

            coins.WatchAd();

            Assert.AreEqual(50, coins.Coins);
        }

        [Test]
        public void WatchAd_AddsNothing_WhenNoAdIsReady()
        {
            var coins = new RewardedCoins(new FakeAdService { IsRewardedReady = false }, coinsPerAd: 50);

            coins.WatchAd();

            Assert.AreEqual(0, coins.Coins);
        }

        [Test]
        public void CanWatchAd_FollowsAdReadiness()
        {
            var ads = new FakeAdService();
            var coins = new RewardedCoins(ads, coinsPerAd: 50);
            Assert.IsTrue(coins.CanWatchAd);

            ads.IsRewardedReady = false;

            Assert.IsFalse(coins.CanWatchAd);
        }
    }
}
