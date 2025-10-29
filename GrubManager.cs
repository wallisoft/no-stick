using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NoStick;

public class GrubManager
{
    private const string GRUB_CUSTOM_CONFIG = "/etc/grub.d/40_custom";
    private const string GRUB_CONFIG = "/boot/grub/grub.cfg";
    
    public async Task<bool> IsGrubInstalledAsync()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return false;

        try
        {
            var result = await ExecuteCommandAsync("which", "grub-install");
            return result.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> DetectPrimaryDiskAsync()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "Unknown";

        try
        {
            var result = await ExecuteCommandAsync("lsblk", "-dno NAME,TYPE");
            if (result.Success)
            {
                var lines = result.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 && parts[1] == "disk")
                    {
                        return "/dev/" + parts[0];
                    }
                }
            }
        }
        catch { }

        return "/dev/sda";
    }

    public async Task<GrubInstallResult> InstallGrubAsync(string targetDisk, bool uefiMode = false)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return new GrubInstallResult 
            { 
                Success = false, 
                ErrorMessage = "GRUB installation only supported on Linux" 
            };
        }

        try
        {
            string args;
            if (uefiMode)
            {
                // UEFI installation
                args = $"--target=x86_64-efi --efi-directory=/boot/efi --bootloader-id=GRUB --recheck";
            }
            else
            {
                // BIOS/MBR installation
                args = $"--target=i386-pc {targetDisk}";
            }

            var result = await ExecuteCommandAsync("pkexec", $"grub-install {args}");
            
            if (result.Success)
            {
                // Update GRUB configuration
                await ExecuteCommandAsync("pkexec", "update-grub");
                
                return new GrubInstallResult 
                { 
                    Success = true, 
                    Message = "GRUB2 installed successfully" 
                };
            }
            else
            {
                return new GrubInstallResult 
                { 
                    Success = false, 
                    ErrorMessage = result.Error 
                };
            }
        }
        catch (Exception ex)
        {
            return new GrubInstallResult 
            { 
                Success = false, 
                ErrorMessage = ex.Message 
            };
        }
    }

    public async Task<bool> GenerateISOBootEntriesAsync(List<ISOEntry> isos, int timeout = 5, string defaultEntry = "")
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return false;

        try
        {
            var config = new StringBuilder();
            config.AppendLine("#!/bin/sh");
            config.AppendLine("exec tail -n +3 $0");
            config.AppendLine("# no-stick custom boot entries");
            config.AppendLine();

            // Set timeout
            config.AppendLine($"set timeout={timeout}");
            config.AppendLine();

            // Generate entry for each ISO
            int menuIndex = 0;
            foreach (var iso in isos)
            {
                var entryName = iso.Name.Replace(".iso", "");
                var isoPath = iso.FilePath;

                config.AppendLine($"menuentry '{entryName}' {{");
                config.AppendLine($"    set isofile='{isoPath}'");
                config.AppendLine("    loopback loop $isofile");
                config.AppendLine("    linux (loop)/casper/vmlinuz boot=casper iso-scan/filename=$isofile quiet splash");
                config.AppendLine("    initrd (loop)/casper/initrd");
                config.AppendLine("}");
                config.AppendLine();

                menuIndex++;
            }

            // Write to custom config
            var tempFile = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFile, config.ToString());

            var result = await ExecuteCommandAsync("pkexec", $"cp {tempFile} {GRUB_CUSTOM_CONFIG}");
            
            if (result.Success)
            {
                await ExecuteCommandAsync("pkexec", $"chmod +x {GRUB_CUSTOM_CONFIG}");
                await ExecuteCommandAsync("pkexec", "update-grub");
                
                File.Delete(tempFile);
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SetDefaultBootEntryAsync(string entryName)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return false;

        try
        {
            // Set GRUB default
            var result = await ExecuteCommandAsync("pkexec", $"grub-set-default '{entryName}'");
            return result.Success;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SafeBootISOAsync(string isoPath)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return false;

        try
        {
            // Use grub-reboot for one-time boot
            var isoName = Path.GetFileNameWithoutExtension(isoPath);
            var result = await ExecuteCommandAsync("pkexec", $"grub-reboot '{isoName}'");
            
            if (result.Success)
            {
                // Reboot the system
                await ExecuteCommandAsync("pkexec", "reboot");
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DetectWindowsBootManagerAsync()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return true;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Check for Windows partition
            try
            {
                var result = await ExecuteCommandAsync("lsblk", "-o NAME,FSTYPE");
                return result.Output.Contains("ntfs");
            }
            catch { }
        }

        return false;
    }

    public async Task<bool> AddWindowsChainloaderAsync()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return false;

        try
        {
            // os-prober will automatically detect Windows
            var result = await ExecuteCommandAsync("pkexec", "update-grub");
            return result.Success;
        }
        catch
        {
            return false;
        }
    }

    private async Task<CommandResult> ExecuteCommandAsync(string command, string arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        
        await process.WaitForExitAsync();

        return new CommandResult
        {
            Success = process.ExitCode == 0,
            ExitCode = process.ExitCode,
            Output = output,
            Error = error
        };
    }
}

public class GrubInstallResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}

public class CommandResult
{
    public bool Success { get; set; }
    public int ExitCode { get; set; }
    public string Output { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}
