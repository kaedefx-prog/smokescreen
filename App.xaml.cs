using System.Configuration;
using System.Data;
using System.Windows;
using Application = System.Windows.Application;

namespace SmokeScreen;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Create and show the main window (now acting as a patch)
        var patchWindow = new PatchWindow();
        patchWindow.Show();
    }
}

