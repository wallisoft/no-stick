using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.Linq;

namespace NoStick;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Check for boot manager mode
            var args = desktop.Args ?? new string[0];
            
            if (args.Contains("--bootmgr") || args.Contains("-b"))
            {
                // Boot Manager Mode - minimal UI at boot time
                desktop.MainWindow = new BootManagerWindow();
            }
            else
            {
                // Desktop Mode - full featured UI
                desktop.MainWindow = new MainWindow();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
