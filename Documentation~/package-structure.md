# How this package is structured

This is a walkthrough of a real Unity package, file by file. It follows Unity's
[package development guide](https://docs.unity.com/en-us/engine/6000.0/manual/packages-list/cus-pkg-development),
and you can use it as a template for your own.

## What a package gives you

Without packages, you reuse code by copying folders between projects, and every copy drifts. A package
lives in its own repository. A project lists it as a dependency, with a version, and the Package Manager
fetches it. You fix a bug once, publish a new version, and every project can update.

## The layout

```
AdsMediationShim/                 The repository root is the package root
├── package.json                  The manifest. Without it, this isn't a package
├── README.md                     First thing people see on GitHub
├── CHANGELOG.md                  What changed in each version
├── LICENSE.md                    What people may do with the code
├── Runtime/                      The code that ships in the game
│   ├── CatSplatStudios.AdsMediation.asmdef
│   ├── IAdService.cs
│   ├── BannerPosition.cs
│   ├── AdConfig.cs
│   ├── AdServices.cs
│   ├── LevelPlayAdService.cs
│   └── FakeAdService.cs
├── Tests/
│   └── Runtime/                  Tests for the runtime code
│       ├── CatSplatStudios.AdsMediation.Tests.asmdef
│       └── ...
├── Samples~/
│   └── AdsDemo/                  Copied into a project when someone imports the sample
└── Documentation~/               The manual and these guides
```

A package with Editor tools would also have `Editor/` and `Tests/Editor/`. This one has no Editor code, so
it leaves them out. It also includes no third-party code, so it needs no `Third Party Notices.md`. It
*depends* on LevelPlay, but it doesn't contain it.

## `package.json`

This is the file that makes a folder a package. Its name must be lowercase, and it must sit at the root.

```json
{
  "name": "com.catsplatstudios.adsmediation",
  "version": "0.5.0",
  "displayName": "Ads Mediation Shim",
  "description": "A thin layer over Unity LevelPlay...",
  "unity": "6000.0",
  "dependencies": {
    "com.unity.services.levelplay": "9.5.1"
  },
  "samples": [
    {
      "displayName": "Ads Demo",
      "description": "A scene with buttons for every ad format...",
      "path": "Samples~/AdsDemo"
    }
  ]
}
```

| Field | Required? | What it does |
|---|---|---|
| `name` | Yes | The unique id, in reverse domain style: `com.company.package`. Lowercase, and never changes |
| `version` | Yes | The version, in `major.minor.patch` form. See [Versions](#versions) |
| `displayName` | Recommended | The name shown in the Package Manager window |
| `description` | Recommended | The text shown in the Package Manager's details panel |
| `unity` | Recommended | The oldest Unity version this works on. Older editors won't offer the package |
| `dependencies` | Optional | Other packages this one needs. Exact versions only, with no ranges. The Package Manager installs them for you, which is why installing this package also installs LevelPlay |
| `samples` | Optional | The samples listed in the Package Manager |

The real file also has `author`, `keywords`, `license`, and links to the repository, changelog and
documentation. See Unity's [manifest reference](https://docs.unity.com/en-us/engine/6000.0/manual/packages-list/cus-pkg-development/upm-manifest-pkg)
for all of them.

## Assembly definitions

Every script in a package must belong to an assembly definition (`.asmdef`). It compiles those scripts
into their own assembly, separate from your game's. That gives you two things:

- **Dependencies you can see.** `Runtime/CatSplatStudios.AdsMediation.asmdef` lists exactly what this code
  may use. One line, `"Unity.LevelPlay"`, is the whole connection to the SDK.
- **Faster compiles.** Changing game code doesn't recompile the package.

```json
{
  "name": "CatSplatStudios.AdsMediation",
  "rootNamespace": "AdsMediation",
  "references": [
    "Unity.LevelPlay"
  ],
  "autoReferenced": true
}
```

Unity's naming convention is `<company>.<package>` for runtime code, with `.Editor` and `.Tests` suffixes
for the others. `autoReferenced` means game scripts that have no assembly definition of their own can use
this one without being told.

The test assembly is different in three ways. It references the Test Framework, it has
`"autoReferenced": false` so game code can't see it, and its `UNITY_INCLUDE_TESTS` constraint keeps it out
of builds.

## `.meta` files

Unity gives every asset a GUID, and stores it in a `.meta` file next to the asset. Scenes and prefabs refer
to scripts and other assets by that GUID, not by path.

**Commit every `.meta` file.** This trips up nearly everyone. A package installed from Git is read-only in
the project, so Unity can't generate a missing `.meta` file, and it ignores that asset instead:

```
Asset Packages/com.mycompany.mypackage/Runtime/Thing.cs has no meta file, but it's in an immutable folder. The asset will be ignored.
```

The package works on your machine, where it's editable, and breaks for everyone else. To catch this,
install your package from its Git URL in a blank project before you share it.

## Folders ending in `~`

Unity ignores any folder whose name ends in `~`. It doesn't import the contents, compile the scripts or
generate `.meta` files. That's why samples and documentation live in `Samples~` and `Documentation~`: they
don't cost anything until someone wants them.

## Samples

Each folder in `Samples~` is one sample, listed in `package.json`. When someone clicks **Import** in the
Package Manager, Unity copies it into their project at
`Assets/Samples/<package display name>/<version>/<sample display name>`. That copy is theirs to edit.

**A sample with a scene needs `.meta` files too**, even though Unity doesn't generate them inside a `~`
folder. `AdsDemo.unity` finds `AdsDemo.cs` and `AdConfig.asset` through their GUIDs, so without their
`.meta` files, importing would give those assets new GUIDs and the scene's references would break. We
committed them in `Samples~/AdsDemo` for this reason.

The sample also has its own `Tests` folder, so importing it brings along an example of testing game code
against the package.

## Tests

Tests live in `Tests/Runtime` for code that runs in the game, and `Tests/Editor` for Editor code. Installed
packages don't compile their tests by default, which keeps everyone else's projects fast. To run them, list
the package under `testables` in your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.catsplatstudios.adsmediation": "https://github.com/hisham-CSS/unity-adsmediationshim.git#v0.5.0"
  },
  "testables": ["com.catsplatstudios.adsmediation"]
}
```

Then open **Window > General > Test Runner**.

## Versions

Versions follow [Semantic Versioning](https://semver.org/), `major.minor.patch`:

| Change | Bump | Example from this package |
|---|---|---|
| Fixes a bug, and nothing else changes | Patch: `0.5.0` to `0.5.1` | Fixing a retry that doesn't fire |
| Adds something, and existing code still works | Minor: `0.5.0` to `0.6.0` | Adding a new banner size |
| Breaks existing code | Major: `1.0.0` to `2.0.0` | Removing a method from `IAdService` |

Below `1.0.0`, anything can change between minor versions. Going to `1.0.0` is a promise to stop breaking
people casually. This package will do that once it's been proven on real devices.

To release a version:

1. Update `version` in `package.json`.
2. Add an entry at the top of `CHANGELOG.md`.
3. Commit, then tag the commit with `git tag v0.6.0`, and push with `git push --follow-tags`.

## Sharing a package

| How | Install with | Good for |
|---|---|---|
| **Git URL** | **Install package from git URL** | Most small teams and classes. All you need is a repository. This is what we use |
| **Local folder** | **Install package from disk** | Developing the package and a project side by side |
| **Tarball** | **Install package from tarball** | Offline sharing. Sign it first with the UPM CLI, because Unity 6.3 and newer check package signatures |
| **Scoped registry** | Add the registry in Project Settings | Studios that want real version lists, but you must host a registry server |

Git URLs have a few forms:

```
https://github.com/hisham-CSS/unity-adsmediationshim.git          The default branch
https://github.com/hisham-CSS/unity-adsmediationshim.git#v0.5.0   A tag. Use this one
https://github.com/hisham-CSS/unity-adsmediationshim.git#main     A branch
```

`package.json` must be at the repository root. If it's in a subfolder, add `?path=/that/folder`.

Unity records the exact commit in the project's `Packages/packages-lock.json`, so everyone on a team gets
the same code. A branch URL won't pick up new commits until someone updates it. To move to a new version,
change the tag in the URL.

## Try it

1. **Install it.** Make a blank project, and install this package from its Git URL. Read the Console for
   any `.meta` warnings.
2. **Run the tests.** Add it to `testables`, then run them in the Test Runner. Switch the build target
   between Android and Windows, and watch which tests appear.
3. **Import the sample.** Find where Unity puts it, and work out why the scene still finds its script.
4. **Break it on purpose.** Fork the repository, delete `Runtime/BannerPosition.cs.meta`, push, and install
   your fork. Read the error. Then put the file back.
5. **Release a version.** Add a small feature, bump the version, update the changelog, tag it, and update
   your project to it.
6. **Make your own.** Move some code you've written twice into a new package.
