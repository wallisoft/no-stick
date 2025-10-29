using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NoStick;

public partial class GrubConfigWindow : Window
{
    public GrubConfigWindow()
    {
        InitializeComponent();
        LoadBootloaderStatus();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void LoadBootloaderStatus()
    {
        var statusLabel = this.FindControl<TextBlock>("StatusLabel");
        var installButton = this.FindControl<Button>("InstallGrubButton");
        
        if (statusLabel == null || installButton == null) return;

        var grubManager = new GrubManager();
        bool isInstalled = await grubManager.IsGrubInstalledAsync();
        
        if (isInstalled)
        {
            statusLabel.Text = "✅ GRUB2 is installed and active";
            installButton.Content = "Reinstall GRUB2";
        }
        else
        {
            statusLabel.Text = "❌ GRUB2 is not installed";
            installButton.Content = "Install GRUB2";
        }
    }

    private async void OnInstallGrub(object sender, RoutedEventArgs e)
    {
        var grubManager = new GrubManager();
        
        // Detect Windows bootloader
        bool hasWindows = await grubManager.DetectWindowsBootManagerAsync();
        
        // Show confirmation dialog
        bool confirmed = await ShowConfirmDialogAsync(hasWindows);
        if (!confirmed) return;

        UpdateStatus("Detecting primary disk...");
        string targetDisk = await grubManager.DetectPrimaryDiskAsync();
        
        UpdateStatus($"Installing GRUB2 to {targetDisk}...");
        
        try
        {
            var result = await grubManager.InstallGrubAsync(targetDisk);
            
            if (result.Success)
            {
                await ShowSuccessDialogAsync();
                LoadBootloaderStatus(); // Refresh status
            }
            else
            {
                UpdateStatus($"❌ Installation failed: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"❌ Error: {ex.Message}");
        }
    }

    private async Task<bool> ShowConfirmDialogAsync(bool hasWindows)
    {
        var dialog = new Window 
        { 
            Title = "Confirm GRUB Installation", 
            Width = 400, 
            Height = 200, 
            WindowStartupLocation = WindowStartupLocation.CenterOwner 
        };
        
        var tcs = new TaskCompletionSource<bool>();
        
        var stack = new StackPanel 
        { 
            Margin = new Avalonia.Thickness(20), 
            Spacing = 15 
        };
        
        stack.Children.Add(new TextBlock 
        { 
            Text = hasWindows ? 
                "This will replace Windows Boot Manager with GRUB2.\nWindows will still boot via GRUB2.\n\nContinue?" :
                "Install GRUB2 as system bootloader?\n\nThis requires administrator privileges.",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            FontSize = 13 
        });
        
        var btnPanel = new StackPanel 
        { 
            Orientation = Avalonia.Layout.Orientation.Horizontal, 
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, 
            Spacing = 10 
        };
        
        var btnYes = new Button { Content = "Yes, Install", Padding = new Avalonia.Thickness(20, 8) };
        btnYes.Click += (s, e) => { tcs.SetResult(true); dialog.Close(); };
        
        var btnNo = new Button { Content = "Cancel", Padding = new Avalonia.Thickness(20, 8) };
        btnNo.Click += (s, e) => { tcs.SetResult(false); dialog.Close(); };
        
        btnPanel.Children.Add(btnYes);
        btnPanel.Children.Add(btnNo);
        stack.Children.Add(btnPanel);
        
        dialog.Content = stack;
        await dialog.ShowDialog(this);
        
        return await tcs.Task;
    }

    private async Task ShowSuccessDialogAsync()
    {
        var dialog = new Window 
        { 
            Title = "Success", 
            Width = 350, 
            Height = 180, 
            WindowStartupLocation = WindowStartupLocation.CenterOwner 
        };
        
        var stack = new StackPanel 
        { 
            Margin = new Avalonia.Thickness(20), 
            Spacing = 15, 
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center 
        };
        
        stack.Children.Add(new TextBlock { Text = "✅", FontSize = 48 });
        stack.Children.Add(new TextBlock 
        { 
            Text = "GRUB2 installed successfully!", 
            FontSize = 14,
            FontWeight = Avalonia.Media.FontWeight.Bold
        });
        
        var btnOk = new Button { Content = "OK", Padding = new Avalonia.Thickness(30, 8) };
        btnOk.Click += (s, e) => dialog.Close();
        stack.Children.Add(btnOk);
        
        dialog.Content = stack;
        await dialog.ShowDialog(this);
    }

    private void UpdateStatus(string message)
    {
        var statusLabel = this.FindControl<TextBlock>("StatusLabel");
        if (statusLabel != null)
            statusLabel.Text = message;
    }
}
