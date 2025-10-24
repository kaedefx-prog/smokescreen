using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Ink;
// タスクトレイ表示に利用
using System.Windows.Forms;
using System.Drawing;
using Brush = System.Windows.Media.Brush;
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
    // オーバーレイのブラシ
    private Brush _overlayBrush = new SolidColorBrush(Color.FromArgb(0x80, 0x33, 0x33, 0x33));

    public MainWindow()
    {
        InitializeComponent();
        InitializeNotifyIcon();
        LoadWindowSettings();
        // 初期ブラシをグリッドに適用
        MainGrid.Background = _overlayBrush;
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

            // ブラシを復元
            if (settings.Brush != null)
            {
                _overlayBrush = settings.Brush.ToBrush();
                MainGrid.Background = _overlayBrush;
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
        
        var startupMenuItem = new ToolStripMenuItem("Windows起動時に実行");
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

        var editMenuItem = new ToolStripMenuItem("編集モード切り替え");
        editMenuItem.Click += (s, e) => ToggleEditMode();
        var colorMenuItem = new ToolStripMenuItem("色設定...");
        colorMenuItem.Click += (s, e) => ShowColorSettingsWindow();
        var exitMenuItem = new ToolStripMenuItem("終了");
        exitMenuItem.Click += (s, e) => Close();
        
        contextMenu.Items.Add(startupMenuItem);
        contextMenu.Items.Add(new ToolStripSeparator());
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
    /// 色設定ウィンドウからブラシが変更されたときに呼び出されます。
    /// </summary>
    private void SettingsWindow_BrushChanged(Brush newBrush)
    {
        _overlayBrush = newBrush;
        // 編集モードでない場合のみ、背景色を即時反映
        if (!_isEditMode)
        {
            MainGrid.Background = _overlayBrush;
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
            inkCanvas.IsHitTestVisible = true;
        }
        else
        {
            // 編集モードを終了する
            EditControls.Visibility = Visibility.Collapsed;
            MainGrid.Background = _overlayBrush; // 元の背景に戻す
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
        MainGrid.Background = _overlayBrush; // グリッドの背景をオーバーレイの色に設定
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