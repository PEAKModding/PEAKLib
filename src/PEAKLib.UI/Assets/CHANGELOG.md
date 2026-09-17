# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.7.2] - 2026-09-18

### Fixed

- Fixed dropdown positioning when resizing or changing the order of size and position updates ([#61](<https://github.com/PEAKModding/PEAKLib/pull/61>))

## [1.7.1] - 2026-09-14

### Fixed

- Dropdown component positioning is better now ([#60](<https://github.com/PEAKModding/PEAKLib/pull/60>))

## [1.7.0] - 2026-09-08

[ModConfig Improvements + UI Module Additions - #58](<https://github.com/PEAKModding/PEAKLib/pull/58>)

### Added

- Added `PeakDropdown` to provide vanilla-style dropdowns via MenuAPI (currently utilized by ModConfig's new filter).
- Marked `AddToSettingsMenu` as `[Obsolete]`. Replaced it with `AddToSettingsMenus`, which uses an updated builder delegate that will run _before_ the settings page is opened in-game. The old method is retained for backward compatibility.

## [1.6.2] - 2026-08-29

### Fixed

- Templates initializing too late for ModConfig with keybind configs ([#51](<https://github.com/PEAKModding/PEAKLib/pull/51>))

## [1.6.1] - 2025-10-26

### Fixed

- Localization APIs now initialize the localization main table if it hasn't been initialized already instead of throwing

## [1.6.0] - 2025-10-03

### Added

- Added new templates relating to the recently added Controls Menu
- Added hook for PauseMenuControlsPage to grab new templates
- Added hook for PauseMenuMainPage
  - Fixed pauseMenuBuilderDelegate not invoking until the settings page was opened by moving it to here. Will now invoke on pause press.
- New in MenuAPI:
  - AddToControlsMenu (Add element(s) to Controls Menu) using controlsMenuBuilderDelegate
  - AddOnOffSetting (Add an on/off setting to any vanilla setting tab)
  - AddSliderSetting (Add slider setting to any vanilla setting tab)
  - AddEnumSetting (Add drop-down setting to any vanilla setting tab using an enum)
- Added GenericBoolSetting, GenericFloatSetting, and GenericEnumSetting classes to support the last 3 additions to MenuAPI.
- Updated PeakHorizontalTab class with some additional stuff to support modifying the tabs at runtime. Also added the option to assign a custom background color before tab creation.

### Changed

- Icon.

## [1.5.0] - 2025-08-20

### Added

- New `PeakChildPage` which inherits `UIPage`

## [1.4.3] - 2025-08-15

### Fixed

- Fixed for the latest version for the game

## [1.3.0] - 2025-07-15

### Added

- Localization support

## [1.1.1] - 2025-07-11

### Fixed

- ModConfig's Mod Config button said "back" in the latest patch, this is now fixed
