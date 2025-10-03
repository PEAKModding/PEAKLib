# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
