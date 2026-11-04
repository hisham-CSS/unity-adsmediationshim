# Ads Mediation Shim

A thin layer over [Unity LevelPlay](https://docs.unity.com/en-us/grow/levelplay). LevelPlay does the
mediation. This package gives your game one small interface to depend on, and handles loading and
reloading ads for you.

It also doubles as a teaching example of [how a Unity package is structured](Documentation~/package-structure.md)
and [how a shim keeps your code from breaking when a third-party SDK changes](Documentation~/writing-a-shim.md).

![Unity Version](https://img.shields.io/badge/Unity-6000.0%2B-blue)
![License](https://img.shields.io/badge/License-MIT-green)

```csharp
public interface IAdService : IDisposable
{
    bool IsInterstitialReady { get; }
    bool IsRewardedReady { get; }

    void ShowBanner(BannerPosition position = BannerPosition.BottomCenter);
    void HideBanner();

    void ShowInterstitial();
    void ShowRewarded(Action onRewarded);
}
```

## Installation

In Unity, open **Window > Package Manager**, select **+ > Install package from git URL**, and enter:

```
https://github.com/hisham-CSS/unity-adsmediationshim.git#v0.5.0
```

The `#v0.5.0` pins a version. This also installs LevelPlay, which then needs a few setup steps of its
own, covered in the [manual](Documentation~/ads-mediation-shim.md#setup).

## Quick start

```csharp
using AdsMediation;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private AdConfig config;
    private IAdService ads;
    private int coins;

    private void Awake()
    {
        ads = AdServices.Create(config);
        ads.ShowBanner(BannerPosition.TopCenter);
    }

    private void OnDestroy() => ads.Dispose();

    public void OnWatchAdPressed()
    {
        if (ads.IsRewardedReady) ads.ShowRewarded(() => coins += 50);
    }
}
```

Create your `AdConfig` with **Assets > Create > Ads > Ad Config**, and paste in the app key and ad unit
ids from your LevelPlay dashboard.

To see it working right away, select this package in the Package Manager and import the **Ads Demo**
sample from its **Samples** tab. In the Editor on an Android or iOS build target, it runs LevelPlay's
mock ads.

## Documentation

| Guide | What it covers |
|---|---|
| [Manual](Documentation~/ads-mediation-shim.md) | Setup, placing banners, testing, and the API reference |
| [How this package is structured](Documentation~/package-structure.md) | Every file in a Unity package, and how to share one |
| [Writing a shim](Documentation~/writing-a-shim.md) | Why this package exists, and the rules it follows |
| [Changelog](CHANGELOG.md) | What changed in each version |

## Status

Version 0.x. Tested in the Editor with LevelPlay's mock ads, and the Android build is verified, but it
hasn't yet been run against real ads on a device.

## License

[MIT](LICENSE.md). LevelPlay is a separate product from Unity, under its own terms. This package
depends on it but doesn't include it.
