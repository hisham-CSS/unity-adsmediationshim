using System;
using UnityEngine;

namespace AdsMediation
{
    /// <summary>
    /// The app key and ad unit ids from the LevelPlay dashboard. Android and iOS are separate
    /// apps there, so each has its own set. Leave an id empty if the game doesn't use that format.
    ///
    /// Create one via Assets > Create > Ads > Ad Config.
    /// </summary>
    [CreateAssetMenu(fileName = "AdConfig", menuName = "Ads/Ad Config")]
    public sealed class AdConfig : ScriptableObject
    {
        [Serializable]
        public struct PlatformIds
        {
            public string appKey;
            public string bannerId;
            public string interstitialId;
            public string rewardedId;
        }

        [SerializeField] private PlatformIds android;
        [SerializeField] private PlatformIds ios;

        [SerializeField, Tooltip("Opens LevelPlay's test suite on the device once it starts, to check " +
                                 "each network and ad unit. Development builds only. Needs portrait orientation.")]
        private bool openTestSuite;

        /// <summary>The ids for the platform being built.</summary>
        public PlatformIds CurrentPlatform
        {
            get
            {
#if UNITY_IOS
                return ios;
#else
                return android;
#endif
            }
        }

        public bool OpenTestSuite => openTestSuite && Debug.isDebugBuild;
    }
}
