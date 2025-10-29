using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NoStick;

public class PartitionManager
{
    public async Task<SwapInfo?> DetectSwapPartitionAsync()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return await DetectLinuxSwapAsync();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return await DetectWindowsSwapAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error detecting swap: {ex.Message}");
        }

        return null;
    }

    private async Task<SwapInfo?> DetectLinuxSwapAsync()
    {
        try
        {
            // Run swapon -s to detect active swap
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "swapon",
                    Arguments = "-s",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
            {
                var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 1) // Skip header line
                {
                    var parts = lines[1].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        var device = parts[0];
                        if (long.TryParse(parts[2], out long sizeInKB))
                        {
                            return new SwapInfo
                            {
                                Device = device,
                                SizeInBytes = sizeInKB * 1024,
                                IsActive = true
                            };
                        }
                    }
                }
            }

            // Try reading /proc/swaps as fallback
            if (File.Exists("/proc/swaps"))
            {
                var swapLines = await File.ReadAllLinesAsync("/proc/swaps");
                if (swapLines.Length > 1)
                {
                    var parts = swapLines[1].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        var device = parts[0];
                        if (long.TryParse(parts[2], out long sizeInKB))
                        {
                            return new SwapInfo
                            {
                                Device = device,
                                SizeInBytes = sizeInKB * 1024,
                                IsActive = true
                            };
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Linux swap detection error: {ex.Message}");
        }

        return null;
    }

    private async Task<SwapInfo?> DetectWindowsSwapAsync()
    {
        try
        {
            // On Windows, check pagefile.sys
            var systemDrive = Environment.GetFolderPath(Environment.SpecialFolder.System);
            var driveLetter = Path.GetPathRoot(systemDrive);
            var pagefilePath = Path.Combine(driveLetter!, "pagefile.sys");

            if (File.Exists(pagefilePath))
            {
                var fileInfo = new FileInfo(pagefilePath);
                return new SwapInfo
                {
                    Device = pagefilePath,
                    SizeInBytes = fileInfo.Length,
                    IsActive = true
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Windows swap detection error: {ex.Message}");
        }

        return await Task.FromResult<SwapInfo?>(null);
    }

    public async Task<bool> CreateSwapPartitionAsync(long sizeInGB)
    {
        // TODO: Implement swap partition creation
        await Task.Delay(100);
        return false;
    }

    public async Task<bool> CopyISOToSwapAsync(string isoPath, string targetDevice)
    {
        // TODO: Implement ISO copy to swap
        await Task.Delay(100);
        return false;
    }
}

public class SwapInfo
{
    public string Device { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public bool IsActive { get; set; }

    public string SizeFormatted
    {
        get
        {
            double size = SizeInBytes / (1024.0 * 1024.0 * 1024.0);
            return $"{size:F2} GB";
        }
    }
}
