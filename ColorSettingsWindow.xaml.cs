using System;
using System.Windows;
using System.Windows.Media;

namespace SmokeScreen
{
    /// <summary>
    /// Interaction logic for ColorSettingsWindow.xaml
    /// </summary>
    public partial class ColorSettingsWindow : Window
    {
        /// <summary>
        /// 色が変更されたときに発生するイベントです。
        /// </summary>
        public event Action<Color>? ColorChanged;

        /// <summary>
        /// 現在選択されている色を取得または設定します。
        /// </summary>
        public Color CurrentColor
        {
            get => Color.FromArgb((byte)AlphaSlider.Value, (byte)RedSlider.Value, (byte)GreenSlider.Value, (byte)BlueSlider.Value);
            set
            {
                AlphaSlider.Value = value.A;
                RedSlider.Value = value.R;
                GreenSlider.Value = value.G;
                BlueSlider.Value = value.B;
                UpdateColorPreview();
            }
        }

        public ColorSettingsWindow(Color initialColor)
        {
            InitializeComponent();
            CurrentColor = initialColor;
        }

        /// <summary>
        /// スライダーの値が変更されたときに呼び出されます。
        /// </summary>
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsInitialized)
            {
                var newColor = CurrentColor;
                UpdateColorPreview(newColor);
                // イベントを通じてMainWindowに通知します。
                ColorChanged?.Invoke(newColor);
            }
        }

        /// <summary>
        /// 色のプレビューを更新します。
        /// </summary>
        private void UpdateColorPreview(Color? color = null)
        {
            ColorPreview.Fill = new SolidColorBrush(color ?? CurrentColor);
        }
    }
}
