#!/usr/bin/env bash
#
# No-Stick installer for Linux. Installs the Visualised runtime (if needed) and the No-Stick app,
# adds it to your app menu, and gives you a `no-stick` command.
#   curl -fsSL https://github.com/wallisoft/no-stick/releases/latest/download/install.sh | bash
#
set -euo pipefail

DEST="$HOME/.local/opt/no-stick"
VML_DEST="$HOME/.local/opt/vml"

if [ "$(id -u)" -eq 0 ]; then
    echo "Please run as your normal user, not root. It asks for sudo only when it needs it." >&2
    exit 1
fi

echo "==> Installing No-Stick"

# 1. The Visualised runtime (No-Stick is a Visualised app). Install it if it isn't already present.
if ! command -v vml >/dev/null 2>&1 && [ ! -x "$VML_DEST/vml" ]; then
    echo "==> Installing the Visualised runtime"
    curl -fsSL https://github.com/wallisoft/vml-releases/releases/latest/download/install.sh | bash
fi
VML_BIN="$(command -v vml || echo "$VML_DEST/vml")"

# 2. Tools No-Stick uses (best effort; it also asks at first use)
if command -v apt-get >/dev/null 2>&1; then
    need=""
    for pkg in parted kexec-tools; do
        dpkg-query -W -f='${Status}' "$pkg" 2>/dev/null | grep -q "install ok installed" || need="$need $pkg"
    done
    if [ -n "$need" ]; then
        echo "==> Need sudo to install:$need"
        sudo apt-get install -y $need || echo "    (carry on; No-Stick will ask again if it needs these)"
    fi
fi

# 3. The app itself
tmp="$(mktemp -d)"; trap 'rm -rf "$tmp"' EXIT
echo "==> Downloading No-Stick"
curl -fsSL "https://github.com/wallisoft/no-stick/releases/latest/download/no-stick.zip" -o "$tmp/no-stick.zip"
rm -rf "$DEST"; mkdir -p "$DEST"
unzip -q "$tmp/no-stick.zip" -d "$DEST"

mkdir -p "$HOME/.local/bin" "$HOME/.local/share/applications"
cat > "$HOME/.local/bin/no-stick" << LAUNCH
#!/bin/sh
exec "$VML_BIN" "$DEST/no-stick.vml" "\$@"
LAUNCH
chmod +x "$HOME/.local/bin/no-stick"

cat > "$HOME/.local/share/applications/no-stick.desktop" << DESKTOP
[Desktop Entry]
Type=Application
Name=No-Stick
Comment=Boot and install Linux from your hard drive - no USB stick
Exec=$HOME/.local/bin/no-stick
Icon=$DEST/no-stick.png
Terminal=false
Categories=System;Utility;
StartupNotify=false
DESKTOP
command -v update-desktop-database >/dev/null && update-desktop-database "$HOME/.local/share/applications" >/dev/null 2>&1 || true

echo
echo "Done. No-Stick is in your app menu, and 'no-stick' works in a terminal."
case ":$PATH:" in *":$HOME/.local/bin:"*) ;; *) echo "(open a new terminal so ~/.local/bin is on your PATH)";; esac
