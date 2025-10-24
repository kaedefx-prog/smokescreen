using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Ink;
// For NotifyIcon
using System.Windows.Forms;
// For GDI+ types
using System.Drawing;
// Aliases to resolve ambiguity
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace SmokeScreen;

/// <summary>
/// Interaction logic for PatchWindow.xaml
/// </summary>
public partial class PatchWindow : Window
{
    private bool _isEditMode = false;
    private bool _isInteractive = true;
    // The ColorSettingsWindow is also managed by this window.
    private ColorSettingsWindow? _colorSettingsWindow;
    // The overlay brush for this specific patch.
    private Brush _overlayBrush = new SolidColorBrush(Color.FromArgb(0x80, 0x33, 0x33, 0x33));

    public PatchWindow()
    {
        InitializeComponent();
        LoadWindowSettings();
        // Apply initial brush to the grid
        MainGrid.Background = _overlayBrush;
        SetInteractiveMode(_isInteractive);
    }

    /// <summary>
    /// Sets the interactive mode for the window (click-through or not).
    /// </summary>
    public void SetInteractiveMode(bool isInteractive)
    {
        _isInteractive = isInteractive;
        IsHitTestVisible = isInteractive;

        // If interactivity is turned off, force exit edit mode.
        if (!isInteractive && _isEditMode)
        {
            ToggleEditMode(forceExit: true);
        }
    }

    /// <summary>
    /// Loads window settings. This will be adapted to receive settings from the manager.
    /// </summary>
    private void LoadWindowSettings()
    {
        var settings = SettingsManager.LoadSettings();
        if (settings != null)
        {
            Top = settings.Top;
            Left = settings.Left;
            Width = settings.Width;
            Height = settings.Height;

            // Restore Brush
            if (settings.Brush != null)
            {
                _overlayBrush = settings.Brush.ToBrush();
                MainGrid.Background = _overlayBrush;
            }

            // Restore Shape
            if (!string.IsNullOrEmpty(settings.ClipGeometry))
            {
                MainGrid.Clip = PathGeometry.Parse(settings.ClipGeometry);
            }
        }
    }

    /// <summary>
    /// Saves the current window settings. This will be adapted to pass settings to the manager.
    /// </summary>
    private void SaveWindowSettings()
    {
        BrushInfo? brushInfo = null;
        if (_overlayBrush is SolidColorBrush solidBrush)
        {
            brushInfo = SolidBrushInfo.FromBrush(solidBrush);
        }
        else if (_overlayBrush is LinearGradientBrush linearBrush)
        {
            brushInfo = LinearGradientBrushInfo.FromBrush(linearBrush);
        }

        var settings = new AppSettings
        {
            Top = Top,
            Left = Left,
            Width = Width,
            Height = Height,
            Brush = brushInfo,
            ClipGeometry = MainGrid.Clip?.ToString(),
        };
        SettingsManager.SaveSettings(settings);
    }
    
    /// <summary>
    /// Shows the color settings window.
    /// </summary>
    public void ShowColorSettingsWindow()
    {
        // Opening the window automatically enables setup mode.
        if (!_isInteractive) SetInteractiveMode(true);

        if (_colorSettingsWindow == null)
        {
            _colorSettingsWindow = new ColorSettingsWindow(_overlayBrush);
            _colorSettingsWindow.Owner = this;
            _colorSettingsWindow.BrushChanged += SettingsWindow_BrushChanged;
            _colorSettingsWindow.Closed += (s, e) => _colorSettingsWindow = null;
            _colorSettingsWindow.Show();
        }
        else
        {
            _colorSettingsWindow.Activate();
        }
    }

    /// <summary>
    /// Called when the brush is changed from the color settings window.
    /// </summary>
    private void SettingsWindow_BrushChanged(Brush newBrush)
    {
        _overlayBrush = newBrush;
        // Only apply background if not in edit mode.
        if (!_isEditMode)
        {
            MainGrid.Background = _overlayBrush;
        }
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // This event won't fire if IsHitTestVisible is false, but check for safety.
        if (!_isInteractive || (_isEditMode && e.ClickCount >= 2)) return;

        DragMove();
    }

    private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!_isInteractive) return;

        if (e.ClickCount >= 2)
        {
            ToggleEditMode();
        }
    }
    
    /// <summary>
    /// Toggles the ink canvas editing mode.
    /// </summary>
    public void ToggleEditMode(bool forceExit = false)
    {
        // Entering edit mode automatically enables setup mode.
        if (!_isInteractive) SetInteractiveMode(true);

        // Invert the current state or force exit.
        _isEditMode = forceExit ? false : !_isEditMode;

        if (_isEditMode)
        {
            // Enter Edit Mode
            EditControls.Visibility = Visibility.Visible;
            MainGrid.Background = new SolidColorBrush(Color.FromArgb(0x80, 0xFF, 0xFF, 0xFF)); // Semi-transparent white
            MainGrid.Clip = null; // Remove clip for editing
            inkCanvas.IsHitTestVisible = true;
        }
        else
        {
            // Exit Edit Mode
            EditControls.Visibility = Visibility.Collapsed;
            MainGrid.Background = _overlayBrush; // Restore original background
            inkCanvas.IsHitTestVisible = false;
        }
    }


    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        if (inkCanvas.Strokes.Count > 0)
        {
            var pathGeometry = new PathGeometry();
            foreach (var stroke in inkCanvas.Strokes)
            {
                var pathFigure = new PathFigure();
                pathFigure.StartPoint = stroke.StylusPoints[0].ToPoint();
                var polyLineSegment = new PolyLineSegment();
                for (var i = 1; i < stroke.StylusPoints.Count; i++)
                {
                    polyLineSegment.Points.Add(stroke.StylusPoints[i].ToPoint());
                }
                pathFigure.Segments.Add(polyLineSegment);
                pathGeometry.Figures.Add(pathFigure);
            }

            MainGrid.Clip = pathGeometry;
        }

        // Exit edit mode after applying.
        ToggleEditMode(forceExit: true);
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        inkCanvas.Strokes.Clear();
        MainGrid.Clip = null;
    }
    
    /// <summary>
    /// Called when the window is closing.
    /// </summary>
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // Save settings before closing.
        SaveWindowSettings();

        // Close the color settings window if it's open.
        _colorSettingsWindow?.Close();
        
        base.OnClosing(e);
    }
}