using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Ink;

namespace SmokeScreen;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private bool _isEditMode = false;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Only allow dragging when not in edit mode and not a double click.
        if (!_isEditMode && e.ClickCount < 2)
        {
            DragMove();
        }
    }

    private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount >= 2)
        {
            _isEditMode = !_isEditMode;

            if (_isEditMode)
            {
                // Enter Edit Mode
                EditControls.Visibility = Visibility.Visible;
                MainGrid.Background = new SolidColorBrush(Color.FromArgb(0x80, 0xFF, 0xFF, 0xFF)); // Semi-transparent white
                MainGrid.Clip = null; // Remove clip to show the full window for editing
                this.Background = Brushes.Transparent; // Make window background transparent to see through
                inkCanvas.IsHitTestVisible = true;
            }
            else
            {
                // Exit Edit Mode
                EditControls.Visibility = Visibility.Collapsed;
                this.Background = new SolidColorBrush(Color.FromArgb(0x80, 0x33, 0x33, 0x33)); // Restore original background
            }
        }
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        if (inkCanvas.Strokes.Count > 0)
        {
            PathGeometry pathGeometry = new PathGeometry();
            foreach (Stroke stroke in inkCanvas.Strokes)
            {
                PathFigure pathFigure = new PathFigure();
                pathFigure.StartPoint = stroke.StylusPoints[0].ToPoint();
                PolyLineSegment polyLineSegment = new PolyLineSegment();
                for (int i = 1; i < stroke.StylusPoints.Count; i++)
                {
                    polyLineSegment.Points.Add(stroke.StylusPoints[i].ToPoint());
                }
                pathFigure.Segments.Add(polyLineSegment);
                pathGeometry.Figures.Add(pathFigure);
            }

            MainGrid.Clip = pathGeometry;
        }

        // Exit edit mode after applying
        _isEditMode = false;
        EditControls.Visibility = Visibility.Collapsed;
        MainGrid.Background = Brushes.Transparent; // Make grid background transparent
        this.Background = new SolidColorBrush(Color.FromArgb(0x80, 0x33, 0x33, 0x33)); // Set the window background to the overlay color
        inkCanvas.IsHitTestVisible = false;
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        inkCanvas.Strokes.Clear();
        MainGrid.Clip = null;
    }
}