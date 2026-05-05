# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]
### Added

### Changed

### Deprecated

### Removed

### Fixed
- Exception when duplicate weight definitions existing.

### Security

## [1.1.2] - 2026-05-05
### Fixed
- Error when specifying a material slot greater than the number of submeshes in BoneWeightByMask.

## [1.1.1] - 2026-05-05
### Fixed
- BoneWeightByMask was not correctly handling meshes with multiple submeshes.

## [1.1.0] - 2026-04-26
### Added
- Support MeshRenderer.

## [1.0.2] - 2026-04-25
### Fixed
- Exception was thrown when there were missing bones.

## [1.0.1] - 2026-04-22
### Fixed
- BoneWeightByMask.Weight was not being clamped.
- BoneWeightByMask.Weight was not being localized.

## [1.0.0] - 2026-04-22
### Added
- Initial release.
