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
    private NotifyIcon? _notifyIcon;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        InitializeNotifyIcon();

        // Create and show the main window (now acting as a patch)
        var patchWindow = new PatchWindow();
        patchWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Dispose the notify icon on exit
        _notifyIcon?.Dispose();
        base.OnExit(e);
    }

    private void InitializeNotifyIcon()
    {
        _notifyIcon = new NotifyIcon();
        _notifyIcon.Icon = SystemIcons.Application;
        _notifyIcon.Text = "SmokeScreen";
        _notifyIcon.Visible = true;

        var contextMenu = new ContextMenuStrip();
        
        var setupModeMenuItem = new ToolStripMenuItem("Setup Mode");
        setupModeMenuItem.CheckOnClick = true;
        setupModeMenuItem.Checked = true; // Will be managed later
        // setupModeMenuItem.Click += ...

        var startupMenuItem = new ToolStripMenuItem("Run on Windows startup");
        startupMenuItem.CheckOnClick = true;
        startupMenuItem.Checked = StartupManager.IsInStartup();
        startupMenuItem.Click += (s, e) =>
        {
            if (startupMenuItem.Checked)
            {
                StartupManager.AddToStartup();
            }
            else
            {
                StartupManager.RemoveFromStartup();
            }
        };

        var editMenuItem = new ToolStripMenuItem("Edit Mode");
        // editMenuItem.Click += ...

        var colorMenuItem = new ToolStripMenuItem("Color Settings...");
        // colorMenuItem.Click += ...

        var exitMenuItem = new ToolStripMenuItem("Exit");
        exitMenuItem.Click += (s, e) => Shutdown();
        
        contextMenu.Items.Add(setupModeMenuItem);
        contextMenu.Items.Add(editMenuItem);
        contextMenu.Items.Add(colorMenuItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(startupMenuItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(exitMenuItem);

        _notifyIcon.ContextMenuStrip = contextMenu;
    }
}

