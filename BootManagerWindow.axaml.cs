using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace NoStick;

public partial class BootManagerWindow : Window
{
    private readonly ObservableCollection<ISOEntry> _availableISOs;
    private DispatcherTimer? _countdownTimer;
    private int _countdown = 5;
    private ISOEntry? _defaultISO;

    public BootManagerWindow()
    {
        InitializeComponent();
        
        _availableISOs = new ObservableCollection<ISOEntry>();
        
        InitializeControls();
        LoadISOs();
        StartCountdown();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InitializeControls()
    {
        var isoList = this.FindControl<ItemsControl>("ISOList");
        var btnBootNow = this.FindControl<Button>("BtnBootNow");
        var btnBootNormal = this.FindControl<Button>("BtnBootNormal");
        var btnCancel = this.FindControl<Button>("BtnCancel");

        if (isoList != null)
        {
            isoList.ItemsSource = _availableISOs;
        }

        if (btnBootNow != null)
            btnBootNow.Click += BtnBootNow_Click;

        if (btnBootNormal != null)
            btnBootNormal.Click += BtnBootNormal_Click;

        if (btnCancel != null)
            btnCancel.Click += BtnCancel_Click;
    }

    private void LoadISOs()
    {
        // TODO: Load ISOs from configuration file or database
        // For now, add some test data
        
        var testISO1 = new ISOEntry
        {
            Name = "Ubuntu 24.04 LTS",
            Location = "/swap",
            SizeInBytes = 4_700_000_000,
            IsDefault = true
        };

        var testISO2 = new ISOEntry
        {
            Name = "Kali Linux 2024.1",
            Location = "/swap",
            SizeInBytes = 3_900_000_000,
            IsDefault = false
        };

        _availableISOs.Add(testISO1);
        _availableISOs.Add(testISO2);

        // Find default ISO
        foreach (var iso in _availableISOs)
        {
            if (iso.IsDefault)
            {
                _defaultISO = iso;
                break;
            }
        }

        // Show/hide no ISOs panel
        var noISOsPanel = this.FindControl<Border>("NoISOsPanel");
        if (noISOsPanel != null)
        {
            noISOsPanel.IsVisible = _availableISOs.Count == 0;
        }

        // Disable boot button if no ISOs
        var btnBootNow = this.FindControl<Button>("BtnBootNow");
        if (btnBootNow != null)
        {
            btnBootNow.IsEnabled = _availableISOs.Count > 0;
        }
    }

    private void StartCountdown()
    {
        if (_availableISOs.Count == 0 || _defaultISO == null)
        {
            // No default ISO, just show message
            UpdateCountdownText("Select an ISO to boot");
            return;
        }

        _countdownTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        _countdownTimer.Tick += CountdownTimer_Tick;
        _countdownTimer.Start();

        UpdateCountdownText($"Booting {_defaultISO.Name} in {_countdown} seconds...");
    }

    private void CountdownTimer_Tick(object? sender, EventArgs e)
    {
        _countdown--;

        if (_countdown <= 0)
        {
            _countdownTimer?.Stop();
            BootDefaultISO();
        }
        else
        {
            UpdateCountdownText($"Booting {_defaultISO!.Name} in {_countdown} second{(_countdown != 1 ? "s" : "")}...");
        }
    }

    private void UpdateCountdownText(string text)
    {
        var countdownText = this.FindControl<TextBlock>("CountdownText");
        if (countdownText != null)
        {
            countdownText.Text = text;
        }
    }

    private void StopCountdown()
    {
        _countdownTimer?.Stop();
        UpdateCountdownText("Countdown stopped");
    }

    private void BtnBootNow_Click(object? sender, RoutedEventArgs e)
    {
        StopCountdown();
        
        // TODO: Boot the selected ISO
        UpdateCountdownText("Booting selected ISO...");
        
        // For now, just close
        Close();
    }

    private void BtnBootNormal_Click(object? sender, RoutedEventArgs e)
    {
        StopCountdown();
        
        // TODO: Boot system normally (chainload to next bootloader)
        UpdateCountdownText("Booting system normally...");
        
        // For now, just close
        Close();
    }

    private void BtnCancel_Click(object? sender, RoutedEventArgs e)
    {
        StopCountdown();
        Close();
    }

    private void BootDefaultISO()
    {
        if (_defaultISO == null) return;

        UpdateCountdownText($"Booting {_defaultISO.Name}...");
        
        // TODO: Implement actual ISO boot
        // This would involve:
        // 1. Writing GRUB config for one-time boot
        // 2. Calling grub-reboot with the ISO entry
        // 3. Rebooting the system
        
        Task.Delay(2000).ContinueWith(_ =>
        {
            Dispatcher.UIThread.Post(() => Close());
        });
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        StopCountdown();
        base.OnClosing(e);
    }
}
