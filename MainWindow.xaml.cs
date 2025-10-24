using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Ink;
// タスクトレイ表示に利用
using System.Windows.Forms;
using System.Drawing;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace SmokeScreen;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private bool _isEditMode = false;
    // タスクトレイに表示するアイコン
    private NotifyIcon? _notifyIcon;
    // 色設定ウィンドウのインスタンス
    private ColorSettingsWindow? _colorSettingsWindow;
    // オーバーレイの色
    private Color _overlayColor = Color.FromArgb(0x80, 0x33, 0x33, 0x33);

    public MainWindow()
    {
        InitializeComponent();
        InitializeNotifyIcon();
        LoadWindowSettings();
        // 初期色を適用
        Background = new SolidColorBrush(_overlayColor);
    }

    /// <summary>
    /// ウィンドウ設定を読み込み、適用します。
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

            // 色を復元
            if (settings.OverlayColor != null)
            {
                if (ColorConverter.ConvertFromString(settings.OverlayColor) is Color color)
                {
                    _overlayColor = color;
                    Background = new SolidColorBrush(_overlayColor);
                }
            }

            // 形状を復元
            if (!string.IsNullOrEmpty(settings.ClipGeometry))
            {
                MainGrid.Clip = PathGeometry.Parse(settings.ClipGeometry);
            }
        }
    }

    /// <summary>
    /// 現在のウィンドウ設定を保存します。
    /// </summary>
    private void SaveWindowSettings()
    {
        var settings = new AppSettings
        {
            Top = this.Top,
            Left = this.Left,
            Width = this.Width,
            Height = this.Height,
            OverlayColor = _overlayColor.ToString(),
            ClipGeometry = MainGrid.Clip?.ToString(),
        };
        SettingsManager.SaveSettings(settings);
    }
    
    /// <summary>
    /// タスクトレイアイコンを初期化します。
    /// </summary>
    private void InitializeNotifyIcon()
    {
        _notifyIcon = new NotifyIcon();
        _notifyIcon.Icon = SystemIcons.Application;
        _notifyIcon.Text = "SmokeScreen";
        _notifyIcon.Visible = true;

        // コンテキストメニューを作成します。
        var contextMenu = new ContextMenuStrip();
        var editMenuItem = new ToolStripMenuItem("編集モード切り替え");
        editMenuItem.Click += (s, e) => ToggleEditMode();
        var colorMenuItem = new ToolStripMenuItem("色設定...");
        colorMenuItem.Click += (s, e) => ShowColorSettingsWindow();
        var exitMenuItem = new ToolStripMenuItem("終了");
        exitMenuItem.Click += (s, e) => Close();
        
        contextMenu.Items.Add(editMenuItem);
        contextMenu.Items.Add(colorMenuItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(exitMenuItem);

        _notifyIcon.ContextMenuStrip = contextMenu;
    }
    
    /// <summary>
    /// 色設定ウィンドウを表示します。既に開いている場合はアクティブにします。
    /// </summary>
    private void ShowColorSettingsWindow()
    {
        if (_colorSettingsWindow == null)
        {
            _colorSettingsWindow = new ColorSettingsWindow(_overlayColor);
            _colorSettingsWindow.Owner = this;
            _colorSettingsWindow.ColorChanged += ColorSettingsWindow_ColorChanged;
            _colorSettingsWindow.Closed += (s, e) => _colorSettingsWindow = null;
            _colorSettingsWindow.Show();
        }
        else
        {
            _colorSettingsWindow.Activate();
        }
    }

    /// <summary>
    /// 色設定ウィンドウから色が変更されたときに呼び出されます。
    /// </summary>
    private void ColorSettingsWindow_ColorChanged(Color newColor)
    {
        _overlayColor = newColor;
        // 編集モードでない場合のみ、背景色を即時反映
        if (!_isEditMode)
        {
            Background = new SolidColorBrush(_overlayColor);
        }
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
            ToggleEditMode();
        }
    }
    
    /// <summary>
    /// 編集モードのオン/オフを切り替えます。
    /// </summary>
    private void ToggleEditMode()
    {
        _isEditMode = !_isEditMode;

        if (_isEditMode)
        {
            // 編集モードに入る
            EditControls.Visibility = Visibility.Visible;
            MainGrid.Background = new SolidColorBrush(Color.FromArgb(0x80, 0xFF, 0xFF, 0xFF)); // 半透明の白
            MainGrid.Clip = null; // 編集のためにクリップを解除
            Background = Brushes.Transparent; // ウィンドウの背景を透明にして背後が見えるようにする
            inkCanvas.IsHitTestVisible = true;
        }
        else
        {
            // 編集モードを終了する
            EditControls.Visibility = Visibility.Collapsed;
            Background = new SolidColorBrush(_overlayColor); // 元の背景に戻す
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

        // 適用後に編集モードを終了
        _isEditMode = false;
        EditControls.Visibility = Visibility.Collapsed;
        MainGrid.Background = Brushes.Transparent; // グリッドの背景を透明にする
        Background = new SolidColorBrush(_overlayColor); // ウィンドウの背景をオーバーレイの色に設定
        inkCanvas.IsHitTestVisible = false;
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        inkCanvas.Strokes.Clear();
        MainGrid.Clip = null;
    }
    
    /// <summary>
    /// ウィンドウが閉じられるときに呼び出されます。
    /// </summary>
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // 現在の設定を保存します。
        SaveWindowSettings();

        // 色設定ウィンドウを閉じる
        _colorSettingsWindow?.Close();
        
        // タスクトレイのアイコンを非表示にし、リソースを解放します。
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }
        base.OnClosing(e);
    }
}