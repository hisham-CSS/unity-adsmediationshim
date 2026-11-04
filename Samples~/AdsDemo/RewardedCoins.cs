namespace AdsMediation.Demo
{
    /// <summary>
    /// Game logic that uses ads without knowing who serves them. It is plain C# and only sees
    /// IAdService, so a test can run it with a FakeAdService and no scene, SDK or device.
    /// </summary>
    public sealed class RewardedCoins
    {
        private readonly IAdService ads;
        private readonly int coinsPerAd;

        public int Coins { get; private set; }

        public bool CanWatchAd => ads.IsRewardedReady;

        public RewardedCoins(IAdService ads, int coinsPerAd)
        {
            this.ads = ads;
            this.coinsPerAd = coinsPerAd;
        }

        public void WatchAd() => ads.ShowRewarded(() => Coins += coinsPerAd);
    }
}
