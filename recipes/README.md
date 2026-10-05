# Recipes

A recipe teaches No-Stick how one family of Linux systems boots from a disk-image file. No-Stick picks a
recipe by the system's **boot machinery** (how it builds its initramfs), not by its name, so one recipe
usually covers a whole family of distros.

| File | Covers | Status |
|---|---|---|
| `10-debian.recipe` | Debian, Ubuntu, Linux Mint (initramfs-tools) | Debian 13 and Mint 22 boot; Ubuntu 24.04 ISO boots live |
| `20-arch.recipe` | Arch Linux (mkinitcpio) | experimental |
| `30-fedora.recipe` | Fedora and relatives (dracut) | Fedora 41 boots, SELinux enforcing |

## Read this first: a recipe runs as root

A recipe runs with full administrator rights on the user's PC, and it writes code into the boot process
of the system it prepares. So:

- Recipes are **never downloaded at run time**. No-Stick only uses the recipes that shipped with the
  release the user installed, plus any the user has put on their own machine by hand.
- New and changed recipes arrive as **pull requests** here and are read line by line before they ship.
- Keep a recipe short, plain and commented. If a reviewer can't follow it, it won't be merged.
- A recipe must not use the network, and must not touch anything outside the image mounted at `$M`.

## What a recipe is

A shell file named `NN-name.recipe` (the number sets the order recipes are tried in). It starts with a
comment header and defines up to three functions:

```
# No-Stick recipe: ...
# name:    Shown in the log, e.g. "Fedora and relatives (dracut)"
# tested:  What has actually been booted, on what
# live:    Which live ISOs it can boot, and whether that has been tested

recipe_detect()  { ... }   # is this my kind of system?
recipe_install() { ... }   # make it able to boot from an image file
recipe_live()    { ... }   # optional: can I live-boot this ISO?
```

### `recipe_detect`
The installed system is mounted at `$M`. Return success if this recipe handles it. Test for the boot
machinery, not the distro name: `[ -d "$M/etc/initramfs-tools" ]`, not "is this Debian?".

### `recipe_install`
Called only after `recipe_detect` succeeded. `$M` is the system's root, mounted read-write, with its
`/boot` partition mounted too if it has one, and `/dev`, `/proc`, `/sys` and `/run` bound in, so
`chroot "$M" ...` works. It must:

1. **Install a hook** into the system's initramfs that attaches the image file early in boot. The hook
   reads two kernel parameters: `vhdhost=` (the UUID of the real partition holding the image) and
   `vhdfile=` (the image's path on that partition). It mounts that partition read-write at
   `/run/nostick/host` and attaches the file as a whole disk with its partitions
   (`losetup --find --show --partscan`). The system's own root-finding then works unchanged.
2. **Rebuild the initramfs** so it contains the hook, with drivers for real hardware, not just for the
   virtual machine the installer ran in.
3. **Make it survive kernel updates** made later inside the image.
4. **Leave a fixed name for the newest kernel**: `/boot/vmlinuz` with `/boot/initrd.img` or
   `/boot/initramfs.img`. If it doesn't, No-Stick falls back to the highest-versioned kernel it can find
   at the time, which then goes stale.
5. Set `ARGS` to the kernel parameters the system needs besides `root=` (No-Stick adds `root=UUID=...`,
   `rootflags=subvol=...` for btrfs subvolumes, `vhdhost=` and `vhdfile=` itself).

It must be safe to run twice (that is what "Repair boot" does). Anything it prints appears in the log.
Available variables: `M`, `NAME` (the disk's name), `PRETTY` (the distro's own name), `SUBVOL`.

### `recipe_live` (optional)
A live ISO is mounted read-only at `$M`. If this recipe can boot it, set `K` and `I` to the kernel and
initrd paths inside the ISO, `ARGS` to the kernel parameters that make the live system find its ISO
file, and `KIND` to a short label, then return success; otherwise return failure. Available variables:
`REL` (the ISO's path on its partition), `HOSTUUID` (that partition's UUID), `ISO` (the file).

## Writing and testing one

1. Copy the closest existing recipe into `~/.no-stick/recipes/`. Recipes there are tried **before** the
   shipped ones, so yours wins while you work on it.
2. Install the distro into a disk with No-Stick as usual; the installer runs in a virtual machine and
   can't harm your PC.
3. Click the disk's marker (or "Repair boot") and read the log. Then boot it.
4. If it stops at an emergency prompt, the last lines on screen that start with `nostick:` say which
   step failed.

When it boots, open a pull request with the recipe, an honest `# tested:` line saying exactly what you
booted and on what hardware, and a matching entry in [../RECIPES.md](../RECIPES.md).
