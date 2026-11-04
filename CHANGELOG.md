# Changelog

All notable changes to this package are documented here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and versions follow
[Semantic Versioning](https://semver.org/).

## [0.5.0] - 2026-11-04

### Added
- `BannerPosition`, with nine positions from `TopLeft` to `BottomRight`. `ShowBanner` takes one,
  and calling it again with a different position moves the banner.
- Tests in `Tests/Runtime`, and the Ads Demo sample in `Samples~/AdsDemo`.
- A manual, a guide to how the package is structured, and a guide to writing a shim, all in
  `Documentation~`.

### Changed
- Moved to its own repository, with the package at the root, so it installs from a Git URL.
- The assemblies now follow Unity's naming convention: `CatSplatStudios.AdsMediation` and
  `CatSplatStudios.AdsMediation.Tests`.
- A banner now loads when the game first asks for one, not at startup, so a game that never shows
  a banner makes no banner requests.

## [0.4.0] - 2026-11-03

### Changed
- Replaced a prototype mediator with a thin shim over LevelPlay. LevelPlay already does the
  mediation, so this package now only gives game code one small interface, `IAdService`, and
  handles loading and reloading ads.
