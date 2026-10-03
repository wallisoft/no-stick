# No-Stick

**Boot and install Linux from your hard drive. No USB stick required.**

No-Stick is a bare-metal boot manager: keep a library of ISOs and virtual disks (VHDs) on your
machine, and boot or install any of them directly through GRUB2, with none of the overhead of
virtualisation. Think of it as VirtualBox's convenience, but running on the real hardware.

Built with [Visualised](https://visualised.io) — the whole app is two small `.vml` files you can read.

## What it does

- **Try a distro live** — boot an ISO without installing anything.
- **Install to a VHD** — a full, real install inside a disk-image file, booted directly by GRUB. Make
  as many as you like; each is just a file.
- **Install to a partition** *(advanced)* — including repurposing an existing Linux swap partition, or
  shrinking a partition to make room, so you can even replace your main OS without a stick.
- **Download a distro** — pick from a community-maintained catalogue; the ISO comes from the distro's
  own official site.
- **Boot now** (via kexec, no reboot) or **at next restart**.

## Dual-licensed

No-Stick is free and open source for individuals and community use. A commercial licence is available
for organisations that want support or different terms — see [LICENSE](LICENSE). Wallisoft's intent is
that No-Stick stays free for people, always.

## Install

**Linux (Ubuntu 22.04+ and similar):**

~~~
curl -fsSL https://github.com/wallisoft/no-stick/releases/latest/download/install.sh | bash
~~~

This installs No-Stick and the Visualised runtime it needs. Run it from your app menu, or `no-stick`
from a terminal.

## Build / run from source

No-Stick is a Visualised app. With Visualised installed (`vml` on your PATH):

~~~
git clone https://github.com/wallisoft/no-stick.git
cd no-stick
vml no-stick.vml
~~~

## The distro catalogue

The "Download a distro" list is maintained in the open at
[wallisoft/nostick-distros](https://github.com/wallisoft/nostick-distros). Pull requests to add or
update distributions are welcome — No-Stick reads it live, so additions appear without a new release.

## Safety

No-Stick can change your bootloader and, in advanced mode, your partitions. Those actions carry clear
warnings and ask before doing anything irreversible. **Always have recovery media and a backup before
changing partitions on your main disk.**

## Credits

Created through AI–human collaboration:
- Steve "recursion hurts my head" Wallis — vision, architecture, implementation
- Claude "set: paste" (Anthropic) — development assistance

**Homepage:** https://no-stick.uk
