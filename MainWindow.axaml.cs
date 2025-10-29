using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace NoStick;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<ISOEntry> _installedISOs;
    private readonly ObservableCollection<DistroEntry> _distros;
    private readonly DistroManager _distroManager;
    private readonly PartitionManager _partitionManager;

    public MainWindow()
    {
        InitializeComponent();
        
        _installedISOs = new ObservableCollection<ISOEntry>();
        _distros = new ObservableCollection<DistroEntry>();
        _distroManager = new DistroManager();
        _partitionManager = new PartitionManager();
        
        InitializeControls();
        LoadDistroList();
        DetectPartitions();
    }

    private void InitializeControls()
    {
        var isoComboBox = this.FindControl<ComboBox>("ISOComboBox");
        var distroComboBox = this.FindControl<ComboBox>("DistroComboBox");
        
        if (isoComboBox != null)
        {
            isoComboBox.ItemsSource = _installedISOs;
            isoComboBox.SelectionChanged += ISOComboBox_SelectionChanged;
            _installedISOs.CollectionChanged += (s, e) => UpdateISOCount();
        }
        
        if (distroComboBox != null)
        {
            distroComboBox.ItemsSource = _distros;
            distroComboBox.SelectionChanged += DistroComboBox_SelectionChanged;
        }

        // Button handlers
        var btnAddLocalISO = this.FindControl<Button>("BtnAddLocalISO");
        var btnDownload = this.FindControl<Button>("BtnDownload");
        var btnSafeBootNow = this.FindControl<Button>("BtnSafeBootNow");
        var btnBrowseForBoot = this.FindControl<Button>("BtnBrowseForBoot");
        var btnCopyToSwap = this.FindControl<Button>("BtnCopyToSwap");
        var btnRemove = this.FindControl<Button>("BtnRemove");
        var btnGrubConfig = this.FindControl<Button>("BtnGrubConfig");
        var btnRefresh = this.FindControl<Button>("BtnRefresh");
        var btnDetectPartitions = this.FindControl<Button>("BtnDetectPartitions");
        var menuVisitWebsite = this.FindControl<Button>("MenuVisitWebsite");
        var menuAbout = this.FindControl<Button>("MenuAbout");
        var chkSetAsDefault = this.FindControl<CheckBox>("ChkSetAsDefault");

        if (btnAddLocalISO != null) btnAddLocalISO.Click += BtnAddLocalISO_Click;
        if (btnDownload != null) btnDownload.Click += BtnDownload_Click;
        if (btnSafeBootNow != null) btnSafeBootNow.Click += BtnSafeBootNow_Click;
        if (btnBrowseForBoot != null) btnBrowseForBoot.Click += BtnBrowseForBoot_Click;
        if (btnCopyToSwap != null) btnCopyToSwap.Click += BtnCopyToSwap_Click;
        if (btnRemove != null) btnRemove.Click += BtnRemove_Click;
        if (btnGrubConfig != null) btnGrubConfig.Click += BtnGrubConfig_Click;
        if (btnRefresh != null) btnRefresh.Click += BtnRefresh_Click;
        if (btnDetectPartitions != null) btnDetectPartitions.Click += BtnDetectPartitions_Click;
        if (menuVisitWebsite != null) menuVisitWebsite.Click += MenuVisitWebsite_Click;
        if (menuAbout != null) menuAbout.Click += MenuAbout_Click;
        if (chkSetAsDefault != null) chkSetAsDefault.IsCheckedChanged += ChkSetAsDefault_Changed;
    }

    private void ISOComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var comboBox = sender as ComboBox;
        var selectedISO = comboBox?.SelectedItem as ISOEntry;
        
        var btnCopyToSwap = this.FindControl<Button>("BtnCopyToSwap");
        var btnRemove = this.FindControl<Button>("BtnRemove");
        var chkSetAsDefault = this.FindControl<CheckBox>("ChkSetAsDefault");
        
        bool hasSelection = selectedISO != null;
        
        if (btnCopyToSwap != null)
            btnCopyToSwap.IsEnabled = hasSelection;
        
        if (btnRemove != null)
            btnRemove.IsEnabled = hasSelection;
        
        if (chkSetAsDefault != null)
        {
            chkSetAsDefault.IsEnabled = hasSelection;
            if (hasSelection)
                chkSetAsDefault.IsChecked = selectedISO!.IsDefault;
        }
    }

    private void ChkSetAsDefault_Changed(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var chkSetAsDefault = sender as CheckBox;
        var isoComboBox = this.FindControl<ComboBox>("ISOComboBox");
        var selectedISO = isoComboBox?.SelectedItem as ISOEntry;
        
        if (selectedISO != null && chkSetAsDefault != null)
        {
            bool newValue = chkSetAsDefault.IsChecked ?? false;
            
            // If setting as default, unset all others
            if (newValue)
            {
                foreach (var iso in _installedISOs)
                {
                    iso.IsDefault = false;
                }
                selectedISO.IsDefault = true;
            }
            else
            {
                selectedISO.IsDefault = false;
            }
            
            UpdateStatus($"Default boot: {(newValue ? selectedISO.Name : "None")}");
        }
    }

    private void UpdateISOCount()
    {
        var isoCountText = this.FindControl<TextBlock>("ISOCountText");
        if (isoCountText != null)
        {
            var count = _installedISOs.Count;
            isoCountText.Text = count == 0 ? "No ISOs installed" : 
                               count == 1 ? "1 ISO installed" : 
                               $"{count} ISOs installed";
        }
    }

    private async void LoadDistroList()
    {
        UpdateStatus("Loading distribution list...");
        
        // Check website for updated list
        var distros = await _distroManager.GetDistroListAsync();
        
        _distros.Clear();
        foreach (var distro in distros)
        {
            _distros.Add(distro);
        }
        
        UpdateStatus($"Loaded {_distros.Count} distributions");
    }

    private async void DetectPartitions()
    {
        var swapInfo = await _partitionManager.DetectSwapPartitionAsync();
        
        var swapInfoText = this.FindControl<TextBlock>("SwapInfoText");
        if (swapInfoText != null && swapInfo != null)
        {
            swapInfoText.Text = $"Swap: {swapInfo.Device} ({swapInfo.SizeFormatted})";
        }
        else if (swapInfoText != null)
        {
            swapInfoText.Text = "Swap: Not detected";
        }
    }

    private void DistroComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var comboBox = sender as ComboBox;
        var selectedDistro = comboBox?.SelectedItem as DistroEntry;
        
        var selectedDistroText = this.FindControl<TextBlock>("SelectedDistroText");
        var btnDownload = this.FindControl<Button>("BtnDownload");
        
        if (selectedDistro != null && selectedDistroText != null)
        {
            selectedDistroText.Text = $"{selectedDistro.Name} - {selectedDistro.Description}";
            if (btnDownload != null)
                btnDownload.IsEnabled = true;
        }
        else if (selectedDistroText != null)
        {
            selectedDistroText.Text = "Select a distribution to download";
            if (btnDownload != null)
                btnDownload.IsEnabled = false;
        }
    }

    private async void BtnAddLocalISO_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Bootable ISO",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("ISO Images")
                {
                    Patterns = new[] { "*.iso" }
                }
            }
        });

        if (files.Count > 0)
        {
            var file = files[0];
            var fileName = file.Name;
            var filePath = file.Path.LocalPath;
            
            // Get file size
            var fileInfo = new System.IO.FileInfo(filePath);
            var sizeInGB = fileInfo.Length / (1024.0 * 1024.0 * 1024.0);
            
            var newISO = new ISOEntry
            {
                Name = fileName,
                Location = "Local",
                SizeInBytes = fileInfo.Length,
                FilePath = filePath,
                IsDefault = _installedISOs.Count == 0
            };
            
            _installedISOs.Add(newISO);
            
            // Auto-select the newly added ISO
            var isoComboBox = this.FindControl<ComboBox>("ISOComboBox");
            if (isoComboBox != null)
            {
                isoComboBox.SelectedItem = newISO;
            }
            
            UpdateStatus($"Added: {fileName} ({sizeInGB:F2} GB)");
        }
    }

    private async void BtnDownload_Click(object? sender, RoutedEventArgs e)
    {
        var distroComboBox = this.FindControl<ComboBox>("DistroComboBox");
        var selectedDistro = distroComboBox?.SelectedItem as DistroEntry;
        
        if (selectedDistro != null)
        {
            UpdateStatus($"Starting download: {selectedDistro.Name}...");
            var downloadProgress = this.FindControl<ProgressBar>("DownloadProgress");
            if (downloadProgress != null)
            {
                downloadProgress.IsVisible = true;
                downloadProgress.IsIndeterminate = true;
            }
            
            // TODO: Implement actual download
            await Task.Delay(2000); // Simulate download
            
            if (downloadProgress != null)
            {
                downloadProgress.IsVisible = false;
            }
            
            UpdateStatus($"Download feature ready - {selectedDistro.Name}");
        }
    }

    private async void BtnSafeBootNow_Click(object? sender, RoutedEventArgs e)
    {
        var distroComboBox = this.FindControl<ComboBox>("DistroComboBox");
        var selectedDistro = distroComboBox?.SelectedItem as DistroEntry;
        
        if (selectedDistro == null)
        {
            // Show dialog: Download ISO or Browse Filesystem
            var dialog = new Window
            {
                Title = "Select ISO Source",
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };
            
            var stack = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 20
            };
            
            stack.Children.Add(new TextBlock 
            { 
                Text = "No ISO selected. How would you like to proceed?",
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                FontSize = 14,
                Margin = new Avalonia.Thickness(0, 10, 0, 0)
            });
            
            var buttonPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Spacing = 10
            };
            
            var btnDownloadFirst = new Button
            {
                Content = "⬇️ Download ISO",
                Padding = new Avalonia.Thickness(20, 10),
                FontSize = 13
            };
            btnDownloadFirst.Click += (s, e) => { dialog.Close(); };
            
            var btnBrowse = new Button
            {
                Content = "📁 Browse Filesystem",
                Padding = new Avalonia.Thickness(20, 10),
                FontSize = 13
            };
            btnBrowse.Click += async (s, e) => 
            { 
                dialog.Close();
                await BtnBrowseForBoot_ClickAsync();
            };
            
            var btnCancel = new Button
            {
                Content = "Cancel",
                Padding = new Avalonia.Thickness(20, 10),
                FontSize = 13
            };
            btnCancel.Click += (s, e) => { dialog.Close(); };
            
            buttonPanel.Children.Add(btnDownloadFirst);
            buttonPanel.Children.Add(btnBrowse);
            buttonPanel.Children.Add(btnCancel);
            
            stack.Children.Add(buttonPanel);
            dialog.Content = stack;
            
            await dialog.ShowDialog(this);
            return;
        }
        
        UpdateStatus($"Safe Boot: {selectedDistro.Name} - This will reboot your system!");
        // TODO: Implement safe boot functionality
    }

    private async void BtnBrowseForBoot_Click(object? sender, RoutedEventArgs e)
    {
        await BtnBrowseForBoot_ClickAsync();
    }

    private async Task BtnBrowseForBoot_ClickAsync()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select ISO to Boot",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("ISO Images")
                {
                    Patterns = new[] { "*.iso" }
                }
            }
        });

        if (files.Count > 0)
        {
            var file = files[0];
            var fileName = file.Name;
            var filePath = file.Path.LocalPath;
            
            UpdateStatus($"Selected for boot: {fileName}");
            // TODO: Implement immediate boot from this ISO
        }
    }

    private void BtnCopyToSwap_Click(object? sender, RoutedEventArgs e)
    {
        var isoComboBox = this.FindControl<ComboBox>("ISOComboBox");
        var selectedISO = isoComboBox?.SelectedItem as ISOEntry;
        
        if (selectedISO != null)
        {
            UpdateStatus($"Copying {selectedISO.Name} to swap partition...");
            // TODO: Implement copy to swap
        }
        else
        {
            UpdateStatus("Please select an ISO to copy");
        }
    }

    private void BtnRemove_Click(object? sender, RoutedEventArgs e)
    {
        var isoComboBox = this.FindControl<ComboBox>("ISOComboBox");
        var selectedISO = isoComboBox?.SelectedItem as ISOEntry;
        
        if (selectedISO != null)
        {
            _installedISOs.Remove(selectedISO);
            UpdateStatus($"Removed: {selectedISO.Name}");
        }
        else
        {
            UpdateStatus("Please select an ISO to remove");
        }
    }

    private void BtnGrubConfig_Click(object? sender, RoutedEventArgs e)
    {
        UpdateStatus("Opening GRUB2 configuration editor...");
        // TODO: Implement GRUB2 config editor
    }

    private void BtnRefresh_Click(object? sender, RoutedEventArgs e)
    {
        LoadDistroList();
        DetectPartitions();
    }

    private void BtnDetectPartitions_Click(object? sender, RoutedEventArgs e)
    {
        DetectPartitions();
    }

    private void MenuVisitWebsite_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://no-stick.uk",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error opening website: {ex.Message}");
        }
    }

    private void MenuAbout_Click(object? sender, RoutedEventArgs e)
    {
        UpdateStatus("no-stick v1.0.0 - Bootable ISO Manager - Visit no-stick.uk");
    }

    private void UpdateStatus(string message)
    {
        var statusText = this.FindControl<TextBlock>("StatusText");
        if (statusText != null)
        {
            statusText.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
        }
    }
}
