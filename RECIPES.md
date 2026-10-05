# What boots: the test record

The recipes themselves are in [recipes/](recipes/); this file records what has actually been booted with them.

What has actually been booted from a disk image on real hardware, what each distro needed, and what is
still untested. No-Stick picks its method from the image's **initramfs family**, not from the distro name,
so a new distro in a supported family should work without a new recipe.

Test machine for everything below: A9-Max (UEFI, NVMe, Ubuntu 24.04 host, images on an ext4 /home partition).

## Supported families

| Family | Distros | How No-Stick hooks in |
|---|---|---|
| initramfs-tools | Debian, Ubuntu, Linux Mint | `scripts/local-top/nostick` attaches the image before root is looked for; the real `losetup` is copied to `/usr/lib/nostick/losetup` |
| mkinitcpio | Arch | `nostickloop` hook, placed before `filesystems` |
| dracut | Fedora and relatives | a `90nostick` dracut module attaches the image while the system waits for its root device |

Every image is attached as a whole disk with its partitions, so any partition layout works. GRUB reads the
kernel and initrd straight from inside the image file; nothing is copied to the host's /boot.

## Tested

### Debian 13 (trixie), live GNOME ISO 13.7.0 — works
- Installed with the guided installer (QEMU), then prepared automatically.
- The installer made an old-style (msdos) partition table: root on partition 1, swap on logical partition 5.
- Needed fix (in 0.4.2): Debian's initrd fills `/usr/sbin` with busybox applets, and busybox's `losetup`
  shadowed the real one, so the image was never attached and boot stopped at a busybox prompt.
- Debian keeps its kernel links in `/`, not `/boot`; the `zz-nostick` kernel hook creates `/boot/vmlinuz`
  and `/boot/initrd.img` so every initramfs-tools distro looks the same to No-Stick.
- Boot now (kexec) into this image works on the test machine.

### Fedora Workstation 41 — works
- Installed with the guided installer (QEMU). GPT layout: BIOS-boot, a 1 GB ext4 `/boot`, and one btrfs partition
  with the system in the `root` subvolume (and `home` beside it).
- Three things differed from the Debian family, all handled in 0.7.0:
  - **btrfs root in a subvolume.** No-Stick now finds a system on btrfs and passes `rootflags=subvol=root`.
  - **The installer's initramfs only had drivers for the virtual machine** (32 MB, against 173 MB for the rescue
    copy), so it could not have seen a real NVMe drive. `/etc/dracut.conf.d/90-nostick.conf` sets `hostonly="no"`,
    now and for every future kernel.
  - **No fixed "latest kernel" name.** `/etc/kernel/install.d/99-nostick.install` keeps `/boot/vmlinuz` and
    `/boot/initramfs.img` on the newest kernel.
- SELinux is enforcing. On the test run the first-boot relabel did the labelling: 0.7.0 had a typo that made the
  direct `setfiles` step fail and fall back to it. Fixed in 0.8.0; the direct route has not been re-tested yet.
- Boots to the desktop on the test machine with the ISO's own 6.11 kernel. With two monitors attached the first-run
  setup did not appear until the second monitor was unplugged.
- Fedora as the *host* (running No-Stick itself on Fedora) is not supported yet: it uses grub2 naming and btrfs.

### Linux Mint 22 Cinnamon — works, after a kernel update
- Installed with the guided installer (QEMU). GPT layout: BIOS-boot, EFI, root on partition 3.
- The ISO's 6.8 kernel is too old for the test machine's graphics: the system boots fully, but Xorg crashes
  and the screen looks frozen at the last boot message. Fixed by installing `linux-generic-hwe-24.04` and
  `linux-firmware` inside the image (chroot from the host).
- Mint then had two kernel series installed, and its own `/boot/vmlinuz` link followed the *last installed*
  kernel (the older one). The `zz-nostick` kernel hook (0.4.1) keeps the link on the highest version.
- 15 GB was too small once updates were applied; the default disk size is now 30 GB.

### Ubuntu 24.04.3 desktop ISO, live — works
- "Try it live" restarted the test machine straight into the ISO's live session (kexec, no firmware restart),
  with the ISO read from the hard drive. Nothing installed, the ISO unchanged.

### Arch Linux — worked with the earlier hook; current hook untested
- An Arch image booted (writable root) using the first, byte-offset version of the `nostickloop` hook.
- The current whole-disk version of that hook has not been booted yet.

## Not yet tested
- Live-booting Mint, Debian, Arch and Fedora ISOs (entries are generated; only the Ubuntu ISO has been booted).
  Expect the Mint 22 ISO to hit the same graphics problem live as it did installed.
- Images stored on anything other than ext4 (NTFS for Windows dual-boot, btrfs, LUKS).
- Images whose root filesystem is neither ext4 nor btrfs.
- openSUSE and the Red Hat family, which should follow Fedora's recipe but have not been booted.
- Hosts that do not boot with GRUB, and Fedora-style hosts (grub2, btrfs). No-Stick says so and stops.

## Known gaps
- If an ISO's kernel is too old for the PC, No-Stick does not yet detect or fix that by itself.
- Kernel updates inside an image are picked up automatically for initramfs-tools distros; Arch has no
  equivalent of the kernel hook yet.
