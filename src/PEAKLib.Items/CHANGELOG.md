# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.4.2] - 2025-08-12

### Fixed

- Recompiled against the latest game assembly references as `CharacterData.currentItem` was changed from a field to a property which was a breaking change.
- If a registered item doesn't have a particle system, PEAKLib won't explode anymore.

## [1.4.1] - 2025-07-25

### Fixed

- All shaders in an item's material will be replaced with the real shader from the game, instead of just one.

## [1.4.0] - 2025-07-18

### Added

- ItemAcceptor feature that allows players to feed/use items on an object implementing the interface.

## [1.3.0] - 2025-07-14

### Added

- Items can now be registered later than before.
- More shaders will be swapped to the correct ones from the game than before.

## [1.2.0] - 2025-07-14

### Added

- Custom item data is now supported for items.

## [1.1.2] - 2025-07-12

### Fixed

- Invalid Content no longer breaks the BundleLoader.

## [1.0.0] - 2025-07-08

### Added

- The changelog to track new updates.
- Fixed UnityItemContent to work with adding content via Unity.
