using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SmokeScreen
{
    public partial class ColorSettingsWindow : Window
    {
        public event Action<Brush>? BrushChanged;
        private bool _isProgrammaticChange = false;

        public ColorSettingsWindow(Brush initialBrush)
        {
            InitializeComponent();
            InitializeControls(initialBrush);
        }

        private void InitializeControls(Brush brush)
        {
            _isProgrammaticChange = true;

            if (brush is LinearGradientBrush linearBrush)
            {
                BrushTypeComboBox.SelectedIndex = 1; // 線形グラデーション
                GradientControls.Visibility = Visibility.Visible;

                SetSlidersFromColor(AlphaSlider1, RedSlider1, GreenSlider1, BlueSlider1, linearBrush.GradientStops[0].Color);
                SetSlidersFromColor(AlphaSlider2, RedSlider2, GreenSlider2, BlueSlider2, linearBrush.GradientStops[1].Color);
                AngleSlider.Value = CalculateAngleFromPoints(linearBrush.StartPoint, linearBrush.EndPoint);
            }
            else if (brush is SolidColorBrush solidBrush)
            {
                BrushTypeComboBox.SelectedIndex = 0; // 単色
                GradientControls.Visibility = Visibility.Collapsed;
                SetSlidersFromColor(AlphaSlider1, RedSlider1, GreenSlider1, BlueSlider1, solidBrush.Color);
            }

            _isProgrammaticChange = false;
            UpdateBrush();
        }

        private void BrushTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isProgrammaticChange) return;

            GradientControls.Visibility = BrushTypeComboBox.SelectedIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
            UpdateBrush();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isProgrammaticChange) return;
            UpdateBrush();
        }

        private void UpdateBrush()
        {
            Brush newBrush;
            if (BrushTypeComboBox.SelectedIndex == 1)
            {
                // 線形グラデーション
                var color1 = GetColorFromSliders(AlphaSlider1, RedSlider1, GreenSlider1, BlueSlider1);
                var color2 = GetColorFromSliders(AlphaSlider2, RedSlider2, GreenSlider2, BlueSlider2);
                var (startPoint, endPoint) = CalculateGradientPoints(AngleSlider.Value);
                newBrush = new LinearGradientBrush(color1, color2, startPoint, endPoint);
            }
            else
            {
                // 単色
                var color1 = GetColorFromSliders(AlphaSlider1, RedSlider1, GreenSlider1, BlueSlider1);
                newBrush = new SolidColorBrush(color1);
            }

            ColorPreview.Fill = newBrush;
            BrushChanged?.Invoke(newBrush);
        }

        private void SetSlidersFromColor(Slider a, Slider r, Slider g, Slider b, Color color)
        {
            a.Value = color.A;
            r.Value = color.R;
            g.Value = color.G;
            b.Value = color.B;
        }

        private Color GetColorFromSliders(Slider a, Slider r, Slider g, Slider b)
        {
            return Color.FromArgb((byte)a.Value, (byte)r.Value, (byte)g.Value, (byte)b.Value);
        }

        private (Point startPoint, Point endPoint) CalculateGradientPoints(double angle)
        {
            double rad = angle * Math.PI / 180.0;
            double x = Math.Cos(rad);
            double y = Math.Sin(rad);

            Point startPoint = new Point(0.5 - x / 2, 0.5 - y / 2);
            Point endPoint = new Point(0.5 + x / 2, 0.5 + y / 2);

            return (startPoint, endPoint);
        }

        private double CalculateAngleFromPoints(Point startPoint, Point endPoint)
        {
            double dx = endPoint.X - startPoint.X;
            double dy = endPoint.Y - startPoint.Y;
            double angleRad = Math.Atan2(dy, dx);
            double angleDeg = angleRad * 180.0 / Math.PI;
            return angleDeg < 0 ? angleDeg + 360 : angleDeg;
        }
    }
}
