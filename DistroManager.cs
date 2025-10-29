using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace NoStick;

public class DistroManager
{
    private const string WEBSITE_URL = "https://no-stick.uk/api/distros.json";
    private readonly List<DistroEntry> _defaultDistros;

    public DistroManager()
    {
        _defaultDistros = new List<DistroEntry>
        {
            // Popular Linux Distributions
            new DistroEntry
            {
                Name = "Ubuntu 24.04 LTS",
                Description = "Popular, user-friendly Linux desktop",
                DownloadUrl = "https://releases.ubuntu.com/24.04/ubuntu-24.04-desktop-amd64.iso",
                Icon = "🟠",
                Category = "Linux Desktop"
            },
            new DistroEntry
            {
                Name = "Ubuntu 22.04 LTS",
                Description = "Long-term support Ubuntu release",
                DownloadUrl = "https://releases.ubuntu.com/22.04/ubuntu-22.04-desktop-amd64.iso",
                Icon = "🟠",
                Category = "Linux Desktop"
            },
            new DistroEntry
            {
                Name = "Linux Mint 21.3",
                Description = "Elegant, easy-to-use desktop Linux",
                DownloadUrl = "https://mirrors.kernel.org/linuxmint/stable/21.3/linuxmint-21.3-cinnamon-64bit.iso",
                Icon = "🍃",
                Category = "Linux Desktop"
            },
            new DistroEntry
            {
                Name = "Fedora Workstation 39",
                Description = "Cutting-edge Linux with latest features",
                DownloadUrl = "https://download.fedoraproject.org/pub/fedora/linux/releases/39/Workstation/x86_64/iso/Fedora-Workstation-Live-x86_64-39-1.5.iso",
                Icon = "🔵",
                Category = "Linux Desktop"
            },
            new DistroEntry
            {
                Name = "Debian 12 Live",
                Description = "Stable, reliable Linux foundation",
                DownloadUrl = "https://cdimage.debian.org/debian-cd/current-live/amd64/iso-hybrid/debian-live-12.5.0-amd64-gnome.iso",
                Icon = "🔴",
                Category = "Linux Desktop"
            },
            new DistroEntry
            {
                Name = "Pop!_OS 22.04 LTS",
                Description = "System76's Ubuntu-based distro for creators",
                DownloadUrl = "https://iso.pop-os.org/22.04/amd64/intel/27/pop-os_22.04_amd64_intel_27.iso",
                Icon = "🚀",
                Category = "Linux Desktop"
            },
            new DistroEntry
            {
                Name = "Manjaro GNOME",
                Description = "User-friendly Arch-based distribution",
                DownloadUrl = "https://download.manjaro.org/gnome/23.1.4/manjaro-gnome-23.1.4-240113-linux66.iso",
                Icon = "🟢",
                Category = "Linux Desktop"
            },
            new DistroEntry
            {
                Name = "Zorin OS 17",
                Description = "Beautiful, Windows-like Linux interface",
                DownloadUrl = "https://zorinos.com/download/17/core",
                Icon = "💎",
                Category = "Linux Desktop"
            },
            
            // Server Distributions
            new DistroEntry
            {
                Name = "Ubuntu Server 24.04 LTS",
                Description = "Popular server OS with long support",
                DownloadUrl = "https://releases.ubuntu.com/24.04/ubuntu-24.04-live-server-amd64.iso",
                Icon = "🖥️",
                Category = "Linux Server"
            },
            new DistroEntry
            {
                Name = "Rocky Linux 9.3",
                Description = "Enterprise Linux, RHEL-compatible",
                DownloadUrl = "https://download.rockylinux.org/pub/rocky/9/isos/x86_64/Rocky-9.3-x86_64-dvd.iso",
                Icon = "⛰️",
                Category = "Linux Server"
            },
            new DistroEntry
            {
                Name = "AlmaLinux 9.3",
                Description = "Community-driven enterprise Linux",
                DownloadUrl = "https://repo.almalinux.org/almalinux/9/isos/x86_64/AlmaLinux-9.3-x86_64-dvd.iso",
                Icon = "🦅",
                Category = "Linux Server"
            },
            
            // Security & Penetration Testing
            new DistroEntry
            {
                Name = "Kali Linux 2024.1",
                Description = "Advanced penetration testing platform",
                DownloadUrl = "https://cdimage.kali.org/kali-2024.1/kali-linux-2024.1-installer-amd64.iso",
                Icon = "🔐",
                Category = "Security"
            },
            new DistroEntry
            {
                Name = "Parrot Security",
                Description = "Security and forensics distribution",
                DownloadUrl = "https://download.parrot.sh/parrot/iso/5.3/Parrot-security-5.3_amd64.iso",
                Icon = "🦜",
                Category = "Security"
            },
            new DistroEntry
            {
                Name = "Tails 6.0",
                Description = "Privacy-focused portable OS",
                DownloadUrl = "https://tails.net/install/download/index.en.html",
                Icon = "🔒",
                Category = "Security"
            },
            
            // Windows
            new DistroEntry
            {
                Name = "Windows 11 (23H2)",
                Description = "Latest Windows 11 release",
                DownloadUrl = "https://www.microsoft.com/software-download/windows11",
                Icon = "🪟",
                Category = "Windows"
            },
            new DistroEntry
            {
                Name = "Windows 10 (22H2)",
                Description = "Windows 10 latest version",
                DownloadUrl = "https://www.microsoft.com/software-download/windows10ISO",
                Icon = "🪟",
                Category = "Windows"
            },
            
            // Rescue & Recovery
            new DistroEntry
            {
                Name = "SystemRescue",
                Description = "Linux system rescue toolkit",
                DownloadUrl = "https://sourceforge.net/projects/systemrescuecd/files/latest/download",
                Icon = "🛟",
                Category = "Rescue"
            },
            new DistroEntry
            {
                Name = "Clonezilla Live",
                Description = "Disk cloning and imaging tool",
                DownloadUrl = "https://sourceforge.net/projects/clonezilla/files/latest/download",
                Icon = "💾",
                Category = "Rescue"
            },
            new DistroEntry
            {
                Name = "GParted Live",
                Description = "Partition editor and disk manager",
                DownloadUrl = "https://sourceforge.net/projects/gparted/files/latest/download",
                Icon = "📊",
                Category = "Rescue"
            },
            new DistroEntry
            {
                Name = "Ultimate Boot CD",
                Description = "Comprehensive diagnostic toolkit",
                DownloadUrl = "https://www.ultimatebootcd.com/download.html",
                Icon = "🔧",
                Category = "Rescue"
            },
            new DistroEntry
            {
                Name = "Hiren's BootCD PE",
                Description = "Windows PE-based recovery environment",
                DownloadUrl = "https://www.hirensbootcd.org/download/",
                Icon = "⚕️",
                Category = "Rescue"
            },
            
            // Lightweight Distributions
            new DistroEntry
            {
                Name = "Puppy Linux",
                Description = "Ultra-lightweight, fast Linux",
                DownloadUrl = "https://puppylinux-woof-ce.github.io/",
                Icon = "🐕",
                Category = "Lightweight"
            },
            new DistroEntry
            {
                Name = "antiX",
                Description = "Fast, lightweight systemd-free",
                DownloadUrl = "https://sourceforge.net/projects/antix-linux/files/latest/download",
                Icon = "⚡",
                Category = "Lightweight"
            }
        };
    }

    public async Task<List<DistroEntry>> GetDistroListAsync()
    {
        // Try to fetch updated list from website
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            
            var response = await client.GetAsync(WEBSITE_URL);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var webDistros = JsonSerializer.Deserialize<List<DistroEntry>>(json);
                
                if (webDistros != null && webDistros.Count > 0)
                {
                    return webDistros;
                }
            }
        }
        catch
        {
            // If website fetch fails, use default list
        }

        // Return default list
        return _defaultDistros;
    }

    public List<DistroEntry> GetDistrosByCategory(string category)
    {
        return _defaultDistros
            .Where(d => d.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public List<string> GetCategories()
    {
        return _defaultDistros
            .Select(d => d.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }
}
