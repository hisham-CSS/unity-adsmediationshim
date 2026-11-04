# Ads Mediation Shim manual

## Overview

Ads Mediation Shim is a thin layer over Unity LevelPlay. Your game talks to one small interface,
`IAdService`, for banner, interstitial and rewarded ads. LevelPlay does the mediation, auctioning each
ad across the networks you set up on its dashboard.

The package loads and reloads ads itself, so your code only asks whether an ad is ready and shows it.

## Package contents

| Location | What's there |
|---|---|
| `Runtime/` | The package code |
| `Tests/Runtime/` | Tests for the package |
| `Samples~/AdsDemo/` | The Ads Demo sample |
| `Documentation~/` | This manual and the teaching guides |

## Requirements

- Unity 6000.0 or newer. Tested on Unity 6000.0 and 6000.6.
- LevelPlay 9.5.1 or newer, which installs automatically with this package.
- Android or iOS for real ads. LevelPlay supports only those platforms.

## Setup

1. **Set up LevelPlay's dashboard.** Add your game on the [LevelPlay dashboard](https://platform.ironsrc.com).
   Android and iOS are separate apps there, each with its own app key. In each app, create an ad unit
   for every format you use, and note its ad unit id. Add your phone as a test device so it gets test
   ads. Clicking live ads on your own device can get your account flagged.
2. **Install this package** from its Git URL, as in the [README](../README.md#installation).
3. **Import Mobile Dependency Resolver.** When Unity asks, choose **Import**.
4. **Install LevelPlay's native SDK.** Open **Ads Mediation > Network Manager**, install the **LevelPlay
   SDK**, then install an adapter for each network you use. Without the SDK, Android builds fail at the
   Gradle step with `package com.unity3d.mediation does not exist`.
5. **Create your config.** Use **Assets > Create > Ads > Ad Config**, and paste in your app keys and ad
   unit ids. Leave an id empty if you don't use that format.

Leave **Automatic Initialization** off in LevelPlay's Developer Settings. `AdServices.Create` starts
LevelPlay for you, and you'll get a console warning if both try.

## Workflows

### Showing ads

Create the service once when the game starts, pass it to whatever needs ads, and dispose of it when the
game shuts down. Don't call `AdServices.Create` a second time while a service is alive.

```csharp
IAdService ads = AdServices.Create(config);

if (ads.IsInterstitialReady) ads.ShowInterstitial();
if (ads.IsRewardedReady) ads.ShowRewarded(() => coins += 50);

ads.Dispose();
```

Grant rewards in the `ShowRewarded` callback, not when the ad closes. The callback runs only if the
player earns the reward, and some networks send the reward after the ad has closed.

### Placing banners

Pass a `BannerPosition` to `ShowBanner`. With no argument, the banner goes at the bottom center.

```csharp
ads.ShowBanner();                          // Bottom center
ads.ShowBanner(BannerPosition.TopRight);   // Moves it to the top right
ads.HideBanner();
ads.ShowBanner(BannerPosition.TopRight);   // Shows the same banner again
```

A banner loads the first time you ask for one, then refreshes itself. LevelPlay sets a banner's position
when it creates it, so moving a banner replaces it with a new one, which makes a new ad request and may
take a moment to appear. Pick a position and stay there if you can.

### Testing

| Where | What runs | What it proves |
|---|---|---|
| Unit tests | `FakeAdService` | Your game logic |
| Editor, Android or iOS build target | LevelPlay's mock ads | The connection to LevelPlay |
| Editor, any other build target | `FakeAdService`, which only logs to the Console | Your game flow |
| Development build on a test device | LevelPlay's test ads | Your app key, ad units and network setup |

**The Editor can't show real ads.** LevelPlay's mock never contacts its servers and accepts any key or id,
so only a phone proves your LevelPlay setup. To check it there, watch the log:

```bash
adb logcat -s Unity
```

```
[Ads] LevelPlay started
[Ads] Rewarded loaded from ironSource
```

For a deeper check, tick **Open Test Suite** on your config. LevelPlay's test suite then opens on the
device after it starts, and shows whether each network and ad unit is set up correctly. It opens only in
development builds, and needs portrait orientation.

To run this package's own tests, add it to `testables` in your project's `Packages/manifest.json`, then
open **Window > General > Test Runner > PlayMode**:

```json
{
  "dependencies": { },
  "testables": ["com.catsplatstudios.adsmediation"]
}
```

### Before you ship

- **Consent.** Players in some regions must agree before ads are personalized. Ask for consent before
  calling `AdServices.Create`, then pass the answer on with `LevelPlay.SetConsent`.
- **Revenue data.** For impression-level revenue, use `LevelPlay.OnImpressionDataReady` directly, or
  LevelPlay's dashboard reports.

## Limitations

- **Banners use the nine preset positions and the standard 320×50 size.** LevelPlay also supports custom
  coordinates and adaptive sizes, which you could add by extending `BannerPosition` and
  `LevelPlayAdService`.
- **LevelPlay's mock banner looks cut off near the screen edge.** The mock draws its logo at a fixed
  534×96 pixels, even though the banner in the Editor is about 192×30, so the logo overflows it. The
  banner's position is correct, and it's only the mock's drawing. Try `BannerPosition.Center` to see the
  whole logo. Real banners are drawn by the ad network.
- **LevelPlay's mock always pays rewards.** Closing its rewarded ad early still grants the reward, which a
  real ad wouldn't do.
- **LevelPlay's mock writes `NullReferenceException` text to the Editor log** while building its ad info.
  This comes from inside LevelPlay and doesn't affect the ads.
- **Consent isn't handled for you.** See [Before you ship](#before-you-ship).

## Troubleshooting

**The buttons stay on "loading" in the Editor.** Check that the build target is Android or iOS, and read the
Console for lines starting with `[Ads]`. A missing app key is reported there.

**An Android build fails at the Gradle step with `package com.unity3d.mediation does not exist`.** The
native SDK isn't in the build. If you haven't done [step 4 of Setup](#setup), do that. If the build used
to work, Mobile Dependency Resolver has probably cleared its lines from
`Assets/Plugins/Android/mainTemplate.gradle`, which can happen if the Editor opens while scripts don't
compile. Fix any compile errors, then run **Assets > Mobile Dependency Resolver > Android Resolver > Force
Resolve**.

## Reference

### `IAdService`

| Member | What it does |
|---|---|
| `bool IsInterstitialReady` | Whether an interstitial has loaded and can be shown now |
| `bool IsRewardedReady` | Whether a rewarded ad has loaded and can be shown now |
| `void ShowBanner(BannerPosition position = BottomCenter)` | Shows the banner now or as soon as it loads. A different position moves it |
| `void HideBanner()` | Takes the banner off screen, and keeps it for next time |
| `void ShowInterstitial()` | Shows an interstitial, or does nothing if none is ready |
| `void ShowRewarded(Action onRewarded)` | Shows a rewarded ad, or does nothing if none is ready. `onRewarded` runs only if the reward is earned |
| `void Dispose()` | Destroys all ads. Call it when the game shuts down |

### `AdServices`

| Member | What it does |
|---|---|
| `IAdService Create(AdConfig config)` | Creates and starts the right service for the build target: `LevelPlayAdService` on Android and iOS, `FakeAdService` everywhere else |

### `AdConfig`

| Field | What it's for |
|---|---|
| Android, iOS: **App Key** | The app key from the LevelPlay dashboard. Without it, ads stay off and the Console says so |
| Android, iOS: **Banner Id**, **Interstitial Id**, **Rewarded Id** | Ad unit ids from the dashboard. Leave one empty if you don't use that format |
| **Open Test Suite** | Opens LevelPlay's test suite on the device, in development builds only |

### `BannerPosition`

`TopLeft`, `TopCenter`, `TopRight`, `CenterLeft`, `Center`, `CenterRight`, `BottomLeft`, `BottomCenter`,
`BottomRight`.

### `FakeAdService`

An `IAdService` for tests. Ads are ready straight away and finish instantly. It adds members for tests to
set and check:

| Member | What it's for |
|---|---|
| `IsInterstitialReady`, `IsRewardedReady` | Settable. Set to `false` to test what happens when no ad is available |
| `IsBannerVisible`, `BannerPosition` | The banner's current state |
| `InterstitialsShown` | How many interstitials have been shown |

## Samples

Import **Ads Demo** from this package's **Samples** tab in the Package Manager. Unity copies it to
`Assets/Samples/Ads Mediation Shim/<version>/Ads Demo`, where you can edit it freely.

| File | What to look at |
|---|---|
| `AdsDemo.unity` | Open it and press Play. The panel has a button for each ad, plus show, hide and move for the banner |
| `AdsDemo.cs` | The composition root: it creates the service, passes it on and disposes of it |
| `RewardedCoins.cs` | Game logic that uses ads and never mentions LevelPlay |
| `Tests/RewardedCoinsTests.cs` | That game logic, tested with a fake and no scene or SDK |
| `AdConfig.asset` | Paste your own keys and ids here to try it on a phone |
