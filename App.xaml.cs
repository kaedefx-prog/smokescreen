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
    private readonly List<PatchWindow> _patchWindows = new();
    private PatchWindow? _lastActivePatchWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        InitializeNotifyIcon();
        CreatePatchWindow();
    }

    private PatchWindow CreatePatchWindow()
    {
        var patchWindow = new PatchWindow();
        _patchWindows.Add(patchWindow);
        patchWindow.Activated += PatchWindow_Activated;
        patchWindow.Closed += PatchWindow_Closed;
        patchWindow.Show();
        return patchWindow;
    }

    private void PatchWindow_Closed(object? sender, System.EventArgs e)
    {
        if (sender is PatchWindow patchWindow)
        {
            _patchWindows.Remove(patchWindow);
            if (_lastActivePatchWindow == patchWindow)
            {
                _lastActivePatchWindow = null;
            }
        }
    }

    private void PatchWindow_Activated(object? sender, System.EventArgs e)
    {
        if (sender is PatchWindow patchWindow)
        {
            _lastActivePatchWindow = patchWindow;
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
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
        setupModeMenuItem.Checked = true; 
        setupModeMenuItem.Click += (s, e) => _lastActivePatchWindow?.SetInteractiveMode(setupModeMenuItem.Checked);

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
        editMenuItem.Click += (s, e) => _lastActivePatchWindow?.ToggleEditMode();

        var colorMenuItem = new ToolStripMenuItem("Color Settings...");
        colorMenuItem.Click += (s, e) => _lastActivePatchWindow?.ShowColorSettingsWindow();

        var exitMenuItem = new ToolStripMenuItem("Exit");
        exitMenuItem.Click += (s, e) => 
        {
            // Make a copy of the list to avoid modification during iteration
            foreach (var window in _patchWindows.ToList())
            {
                window.Close();
            }
            Shutdown();
        };
        
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

