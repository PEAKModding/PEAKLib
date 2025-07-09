# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
