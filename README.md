# 🚀 no-stick - Bootable ISO Manager

**Turn any swap partition or dedicated filesystem into a bootable ISO library with GRUB2 integration**

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Platform: Linux](https://img.shields.io/badge/Platform-Linux-blue.svg)](https://www.linux.org/)
[![Framework: .NET 8](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)

## What is no-stick?

no-stick eliminates the need for USB sticks to boot Linux distributions. Store multiple bootable ISOs on your hard drive's swap partition or a dedicated filesystem, and boot them directly via GRUB2.

**Perfect for:**
- 🔧 System administrators who need multiple rescue/installer ISOs
- 💻 Linux enthusiasts testing different distributions
- 🎓 Anyone tired of maintaining a collection of USB sticks
- 🌍 Users who want a portable multi-boot solution on their laptop

## Features

### ✅ Implemented
- **GRUB2 Installation** - Automatically install/configure GRUB2 as system bootloader
- **Windows Detection** - Safely preserves Windows Boot Manager via chainloader
- **UEFI & BIOS Support** - Works with both modern UEFI and legacy BIOS systems
- **Dual Mode Operation**
  - Desktop mode: Full management UI
  - Boot mode: Minimal UI for boot-time selection

### 🚧 In Development
- **Copy to Swap** - Copy ISOs to swap partition with progress tracking
- **Safe Boot** - One-time boot via grub-reboot (no permanent config changes)
- **ISO Download** - Download popular distributions directly from the app
- **Distro Database** - Curated list of Linux distributions with metadata

## Architecture

```
no-stick/
├── Program.cs              # Mode selector (--bootmgr flag)
├── Models/
│   ├── ISOEntry.cs        # ISO metadata
│   └── DistroEntry.cs     # Distribution database entry
├── Managers/
│   ├── GrubManager.cs     # GRUB2 operations (8.6KB)
│   ├── ISOStorageManager.cs  # ISO persistence (6.3KB)
│   ├── PartitionManager.cs   # Partition detection
│   └── DistroManager.cs      # Distro database
└── UI/
    ├── DesktopMode/
    │   ├── MainWindow.axaml[.cs]        # Main management UI
    │   └── GrubConfigWindow.axaml[.cs]  # GRUB configuration
    └── BootMode/
        └── BootManagerWindow.axaml[.cs] # Boot-time selector
```

## How It Works

1. **Install GRUB2** - no-stick installs GRUB2 as your bootloader (if not already installed)
2. **Copy ISOs** - Store bootable ISOs on your swap partition or dedicated filesystem
3. **Generate Boot Entries** - Automatically creates GRUB menu entries for each ISO
4. **Boot Directly** - Select ISOs from GRUB menu at boot time

## Installation

### Prerequisites
- Linux system (tested on Ubuntu/Debian-based distributions)
- .NET 8.0 SDK/Runtime
- Root/sudo access (for GRUB installation)

### Build from Source
```bash
git clone https://github.com/wallisoft/no-stick.git
cd no-stick
dotnet build
dotnet run
```

### Usage
```bash
# Desktop mode (full UI)
./no-stick

# Boot manager mode (minimal UI)
./no-stick --bootmgr
```

## Technology Stack

- **Framework**: .NET 8.0
- **UI**: Avalonia 11.x (cross-platform XAML)
- **Platform**: Linux (with potential macOS/BSD support)
- **Bootloader**: GRUB2

## Roadmap

- [ ] Complete ISO copying functionality
- [ ] Implement safe boot (grub-reboot)
- [ ] Add ISO download feature
- [ ] Create distro database with popular ISOs
- [ ] Package as .deb/.rpm
- [ ] Add automated testing
- [ ] Multi-language support
- [ ] macOS/BSD support (if feasible)

## Contributing

Contributions welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## License

Dual-licensed under MIT License - see [LICENSE](LICENSE) file for details.

## Credits

**Created by:**
- Steve "recursion hurts my head" Wallis - [wallisoft@gmail.com](mailto:wallisoft@gmail.com)
- Claude "set: paste" (Anthropic)

**Project Homepage:** https://no-stick.uk

## Support

- 📧 Email: wallisoft@gmail.com
- 🌐 Website: https://no-stick.uk
- 🐛 Issues: https://github.com/wallisoft/no-stick/issues

---

**Warning:** Modifying bootloaders can render your system unbootable if done incorrectly. Always ensure you have backups and recovery media before using no-stick.
