# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.5.1] - 2025-08-26

### Added

- Asset bundles with the extension `.autoload_peakbundle` are loaded automatically in addition to `.autoload.peakbundle` due to issues where Unity doesn't want to build Asset Bundles with two dots in the bundle name.

### Fixed

- Fixed an issue with BundleLoader's API where `LoadBundleAndContentsWithName` method didn't have the `Action<PeakBundle>? onLoaded` parameter marked as nullable.
- Fixed an issue where `onLoaded` would be called after `LoadBundleAndContents*` registered all content from the bundle, which would make this method unusable in certain cases.

## [1.2.0] - 2025-07-14

### Added

- ModDefinition now overrides `GetHashCode` to return the hash code of the Id string.

## [1.1.2] - 2025-07-12

### Fixed

- Invalid Content or bad Content implementations should no longer break the BundleLoader.

## [1.1.1] - 2025-07-09

### Fixed

- If the last loaded AssetBundle was invalid, the `BundleLoader.OnAllBundlesLoaded` event would never fire.

## [1.1.0] - 2025-07-09

### Added

- New APIs to BundleLoader.

### Changed

- BundleLoader now loads and registers content as soon as possible, which also means it calls the event for all bundles loaded as soon as possible instead of at airport loading screen.
- BundleLoader now has an UI indication if PEAKLib is taking a while loading asset bundles.

## [1.0.0] - 2025-07-08

### Added

- The changelog to track new updates.
- Async BundleLoader to load PEAKLib content automatically or manually via an API.
