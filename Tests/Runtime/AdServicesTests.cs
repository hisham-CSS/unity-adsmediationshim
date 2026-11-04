using System;
using NUnit.Framework;
using UnityEngine;
#if UNITY_ANDROID || UNITY_IOS
using System.Text.RegularExpressions;
using UnityEngine.TestTools;
#endif

namespace AdsMediation.Tests
{
    public class AdServicesTests
    {
        [Test]
        public void Create_RejectsAMissingConfig()
        {
            Assert.Throws<ArgumentNullException>(() => AdServices.Create(null));
        }

        [Test]
        public void Create_PicksTheServiceForThisBuildTarget()
        {
            var config = ScriptableObject.CreateInstance<AdConfig>();

#if UNITY_ANDROID || UNITY_IOS
            LogAssert.Expect(LogType.Error, new Regex("no app key"));
            using (var ads = AdServices.Create(config))
                Assert.IsInstanceOf<LevelPlayAdService>(ads);
#else
            using (var ads = AdServices.Create(config))
                Assert.IsInstanceOf<FakeAdService>(ads);
#endif
        }
    }
}
