# Changelog

All notable changes to no-stick will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### In Progress
- Copy ISO to swap partition with progress tracking
- Safe boot via grub-reboot (one-time boot)
- ISO download functionality
- Distro database with popular distributions

## [0.1.0] - 2025-10-29

### Added
- Initial release
- GRUB2 installation and configuration
- Windows Boot Manager detection and chainloader support
- UEFI and BIOS mode detection
- Desktop mode UI (MainWindow, GrubConfigWindow)
- Boot manager mode UI (BootManagerWindow)
- Backend managers (GrubManager, ISOStorageManager, PartitionManager, DistroManager)
- Dual-mode operation (desktop vs boot-time)
- MIT License with attribution
- Comprehensive documentation (README, CONTRIBUTING)

### Technical
- Built with .NET 8.0
- Avalonia 11.x UI framework
- Cross-platform C# codebase
- Linux-first design with GRUB2 integration

### Authors
- Steve "recursion hurts my head" Wallis <wallisoft@gmail.com>
- Claude "set: paste" (Anthropic)

[Unreleased]: https://github.com/wallisoft/no-stick/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/wallisoft/no-stick/releases/tag/v0.1.0
