# No-Stick

**Boot and install Linux from your hard drive. No USB stick.**

No-Stick keeps a library of ISOs and virtual disks on your machine and boots any of them directly on
real hardware. You get the convenience of a virtual machine (each system is just a file you can copy
or delete) with none of the overhead: full speed, your real graphics card, your real Wi-Fi.

![The No-Stick window](site/screenshot.png)

Website: [no-stick.uk](https://no-stick.uk)

## Install

Linux with the GRUB boot menu (Ubuntu, Mint, Debian and similar):

```
curl -fsSL https://github.com/wallisoft/no-stick/releases/latest/download/install.sh | bash
```

This installs into your home folder and adds No-Stick to your app menu, plus a `no-stick` command.
Installing changes nothing about how your PC boots; that only happens when you ask No-Stick to add
something to the boot menu. Re-run the same command to update.

## What it does

- **Install to a virtual disk.** The distro's own installer runs in a quick VM that can only see the
  disk-image file, never your real drives. No-Stick then prepares the result to boot on real hardware.
- **One boot-menu entry.** Your main boot menu gains a single "No-Stick" entry. Everything else lives in
  No-Stick's own menu, so your existing entries are never edited.
- **One click puts something in the menu.** Each row has a marker: empty is not in the boot menu, orange is in it,
  green is the default. Right-click a row for the rest: default, rename, back up, delete. A timer can boot the
  default by itself.
- **Boot at restart** boots one chosen disk once, then your PC goes back to normal.
- **Boot now** restarts straight into a disk in seconds (kexec). Experimental.
- **Try it live** restarts straight into an ISO's live system, without installing. New and lightly tested.
- **Back up and restore.** A backup is a copy of the disk that takes only the space actually used; restoring
  makes a new disk, so nothing is overwritten.
- **Repair boot** rebuilds a disk's boot setup after an update or if something stops booting.

Everything lives in `~/.no-stick`: ISOs in `isos/`, disks in `vhds/`, backups in `backups/`.

## What works today

Tested on real hardware. See [RECIPES.md](RECIPES.md) for the details and the known gaps.

| Distro | Status |
|---|---|
| Debian 13 | Installs and boots |
| Linux Mint 22 | Installs and boots; very new PCs need a newer kernel than the ISO ships |
| Ubuntu | Same method as Mint and Debian |
| Fedora 41 | Installs and boots |
| Arch | Experimental |
| Immutable distros (Silverblue, Bazzite and friends) | Not yet |

Requirements: a Debian, Ubuntu or Mint style host with GRUB as its bootloader (Fedora works as an installed
system, but not yet as the host), the disk images on an ext4 partition, and about 30 GB free per
installed system. Guided installs use QEMU, which No-Stick offers to install. Windows is not supported yet.

## How it works

A virtual disk is an ordinary file. At boot, GRUB reads the kernel straight out of that file, and a
small hook inside the installed system's initramfs attaches the file as a disk before the system looks
for its root partition. From then on it is a normal Linux install that happens to live in a file.
Nothing is repartitioned, and deleting the file removes the system.

## Uninstall

In No-Stick, untick everything from the boot menu first. Then:

```
sudo rm -f /etc/grub.d/40_nostick && sudo rm -rf /boot/no-stick && sudo update-grub
rm -rf ~/.vml/apps/no-stick ~/.local/bin/no-stick ~/.local/share/applications/no-stick.desktop
```

Your ISOs and disks stay in `~/.no-stick` until you delete that folder yourself.

## Read it before you run it

No-Stick changes your boot menu and runs some steps as root, so you should be able to see exactly what
it does. The whole app is two readable files: [no-stick.vml](no-stick.vml) (the window and all its
logic, including the root helper script) and [download.vml](download.vml) (the distro catalogue).

It is built with [Visualised](https://visualised.io), which runs `.vml` files. The Visualised runtime
is a separate product: free for individuals, but not open source.

## Contributing

Issues and pull requests are welcome, especially reports of what does or doesn't boot on your hardware.

Support for a family of distros lives in one small file each, in [recipes/](recipes/). A new distro usually
needs no new code if its family already has a recipe. To add a family, write a recipe:
[recipes/README.md](recipes/README.md) explains what one must do and how to test it on your own machine.
Recipes run as root, so they are reviewed before they ship and are never downloaded at run time.

## Licence

MIT. See [LICENSE](LICENSE). Made by Wallisoft, with development help from Claude (Anthropic).
