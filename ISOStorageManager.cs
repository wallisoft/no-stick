using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace NoStick;

public class ISOStorageManager
{
    private const string CONFIG_FILE = "/etc/no-stick/isos.json";
    private const string CONFIG_DIR = "/etc/no-stick";
    private List<ISOEntry> _isos = new();

    public async Task<List<ISOEntry>> LoadISOsAsync()
    {
        try
        {
            if (File.Exists(CONFIG_FILE))
            {
                var json = await File.ReadAllTextAsync(CONFIG_FILE);
                _isos = JsonSerializer.Deserialize<List<ISOEntry>>(json) ?? new();
            }
            else
            {
                _isos = new List<ISOEntry>();
            }
        }
        catch
        {
            _isos = new List<ISOEntry>();
        }

        return _isos;
    }

    public async Task<bool> SaveISOsAsync(List<ISOEntry> isos)
    {
        try
        {
            _isos = isos;

            // Create config directory if it doesn't exist
            if (!Directory.Exists(CONFIG_DIR))
            {
                Directory.CreateDirectory(CONFIG_DIR);
            }

            var json = JsonSerializer.Serialize(isos, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });

            var tempFile = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFile, json);

            // Use pkexec to write to /etc
            var result = await ExecuteCommandAsync("pkexec", $"cp {tempFile} {CONFIG_FILE}");
            File.Delete(tempFile);

            return result.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> AddISOAsync(ISOEntry iso)
    {
        _isos.Add(iso);
        return await SaveISOsAsync(_isos);
    }

    public async Task<bool> RemoveISOAsync(ISOEntry iso)
    {
        _isos.Remove(iso);
        return await SaveISOsAsync(_isos);
    }

    public async Task<bool> CopyISOToSwapAsync(string sourceIsoPath, string swapDevice, Action<int>? progressCallback = null)
    {
        try
        {
            var isoInfo = new FileInfo(sourceIsoPath);
            if (!isoInfo.Exists)
                return false;

            // Create mount point
            var mountPoint = "/mnt/no-stick-temp";
            await ExecuteCommandAsync("pkexec", $"mkdir -p {mountPoint}");

            // Turn off swap temporarily
            await ExecuteCommandAsync("pkexec", $"swapoff {swapDevice}");

            // Copy ISO to swap partition
            // We'll use dd for this as it's more reliable for raw device writing
            var targetPath = $"{swapDevice}";
            
            progressCallback?.Invoke(10);

            var ddCommand = $"dd if={sourceIsoPath} of={targetPath} bs=4M status=progress";
            var result = await ExecuteCommandAsync("pkexec", ddCommand);

            progressCallback?.Invoke(90);

            if (!result.Success)
            {
                // Re-enable swap if it fails
                await ExecuteCommandAsync("pkexec", $"swapon {swapDevice}");
                return false;
            }

            progressCallback?.Invoke(100);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CopyISOToFilesystemAsync(string sourceIsoPath, string targetDirectory, Action<int>? progressCallback = null)
    {
        try
        {
            var isoInfo = new FileInfo(sourceIsoPath);
            if (!isoInfo.Exists)
                return false;

            // Create target directory if it doesn't exist
            if (!Directory.Exists(targetDirectory))
            {
                await ExecuteCommandAsync("pkexec", $"mkdir -p {targetDirectory}");
            }

            var targetPath = Path.Combine(targetDirectory, Path.GetFileName(sourceIsoPath));
            
            progressCallback?.Invoke(10);

            // Copy file with progress
            await CopyFileWithProgressAsync(sourceIsoPath, targetPath, progressCallback);

            progressCallback?.Invoke(100);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task CopyFileWithProgressAsync(string source, string destination, Action<int>? progressCallback)
    {
        const int bufferSize = 1024 * 1024; // 1MB buffer
        var buffer = new byte[bufferSize];

        using var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read);
        using var destStream = new FileStream(destination, FileMode.Create, FileAccess.Write);

        long totalBytes = sourceStream.Length;
        long totalRead = 0;
        int currentRead;

        while ((currentRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await destStream.WriteAsync(buffer, 0, currentRead);
            totalRead += currentRead;

            int percentage = (int)((totalRead * 100) / totalBytes);
            progressCallback?.Invoke(percentage);
        }
    }

    private async Task<ProcessResult> ExecuteCommandAsync(string command, string arguments)
    {
        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
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

        return new ProcessResult
        {
            Success = process.ExitCode == 0,
            ExitCode = process.ExitCode,
            Output = output,
            Error = error
        };
    }
}

public class ProcessResult
{
    public bool Success { get; set; }
    public int ExitCode { get; set; }
    public string Output { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}
