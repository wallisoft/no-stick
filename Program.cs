using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.Linq;

namespace NoStick;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Check for boot manager mode
        if (args.Contains("--bootmgr") || args.Contains("-b"))
        {
            Console.WriteLine("Starting in Boot Manager mode...");
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args, Avalonia.Controls.ShutdownMode.OnMainWindowClose);
        }
        else
        {
            Console.WriteLine("Starting in Desktop mode...");
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}
