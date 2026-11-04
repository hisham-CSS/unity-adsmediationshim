using System;

namespace AdsMediation
{
    /// <summary>
    /// Picks the ad service for the current build target. This is the only place that knows which
    /// implementation is in use, so swapping ad SDKs later means changing this and one new class.
    /// </summary>
    public static class AdServices
    {
        /// <summary>
        /// Creates and starts the ad service. Call this once when the game starts, pass the
        /// result to whatever needs ads, and dispose of it when the game shuts down.
        /// </summary>
        public static IAdService Create(AdConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

#if UNITY_ANDROID || UNITY_IOS
            var service = new LevelPlayAdService(config.CurrentPlatform, config.OpenTestSuite);
            service.Initialize();
            return service;
#else
            return new FakeAdService();
#endif
        }
    }
}
