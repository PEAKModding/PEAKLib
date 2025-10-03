# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.6.0] - 2025-10-03

### Added

- Added Modded Controls page & buttons in Controls Menu and Modded Settings page
- Modded Controls page has a button to Modded Settings page as well
- Modded Controls page automatically generates a listing from KeyCode type configuration items AND string type configuration items with a default value matching an expected input path binding string.
- Modded Controls page is equipped with interactive key-rebinding pages and re-uses the reset element from the vanilla controls page.
- Key name values in the Modded Controls page are translated to key control sprites in a best effort basis. A method has been created to translate KeyCode names to input binding paths.
- Duplicate Key bindings will show with a warning symbol to inform the player they have multiple modded controls bound to the same key. Both types of keybinds (keycode and binding path string) work together with this feature as their string results are compared.
- The configuration items on the Modded Controls page will still exist in the Mod Settings page to avoid confusing players.
- Added secondary PeakHorizontalTab element to Mod Settings page for config sections (also added relevant code for tracking this information)
- Added some common use generic methods to SettingsHandlerUtility.cs
- Added support for Double type config items
- Added AcceptableValueList detection for string type config items to create an enum drop-down box setting type

### Changed

- Visual overhaul of Mod Settings page & Buttons
- Updated Bepinex SettingOptions to use ConfigEntryBase for better tracking of value changes.
  - Fixes an issue where changing a configuration item outside the Mod Settings page would not update the value in the Mod Settings page
  - The ConfigEntryBase value is required as part of the IBepInExProperty interface, as well as a new RefreshValueFromConfig method.
  - This also allowed for the functionality behind displaying only config items with a specific section name

## [1.5.2] - 2025-09-01

### Added

- Polish translation for Mod Settings button

## [1.5.0] - 2025-08-20

### Changed

- Mod Settings menu page now uses PEAKLib.UI's `PeakChildPage` type which uses the new `UIPage` system in the game

### Fixed

- Fixed issue where pressing esc in ModConfig Mod Settings menu in in-game Pause Menu messes it up

## [1.4.3] - 2025-08-15

### Changed

- Config entries are reloaded when opening the settings menu instead of once during game load

### Fixed

- Fixed for the latest version for the game

## [1.3.0] - 2025-07-15

### Added

- Localization support

### Changed

- Mod Config button is now inside the settings menu

## [0.1.3] - 2025-07-11

### Changed

- Fixed "LOC: " text appearing before option names

## [0.1.2] - 2025-07-08

### Changed

- "Mod Settings" text is now all in uppercase for consistency
- New mod icon

## [0.1.1] - 2025-07-07

### Added

- New SettingsType for Enum configs

## [0.1.0] - 2025-07-06

### Added

- Everything
