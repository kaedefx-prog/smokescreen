using System.Text.Json.Serialization;
using ColorConverter = System.Windows.Media.ColorConverter;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;

namespace SmokeScreen;

/// <summary>
/// シリアライズ可能なブラシ設定の基本クラスです。
/// </summary>
[JsonDerivedType(typeof(SolidBrushInfo), typeDiscriminator: "Solid")]
[JsonDerivedType(typeof(LinearGradientBrushInfo), typeDiscriminator: "Linear")]
public abstract class BrushInfo
{
    public abstract Brush ToBrush();
}

/// <summary>
/// 単色ブラシの情報を保持します。
/// </summary>
public class SolidBrushInfo : BrushInfo
{
    public string Color { get; set; } = "#80333333";

    public override Brush ToBrush()
    {
        if (ColorConverter.ConvertFromString(Color) is Color color)
        {
            return new SolidColorBrush(color);
        }
        return new SolidColorBrush(Colors.Transparent);
    }

    public static SolidBrushInfo FromBrush(SolidColorBrush brush)
    {
        return new SolidBrushInfo { Color = brush.Color.ToString() };
    }
}

/// <summary>
/// 線形グラデーションブラシの情報を保持します。
/// </summary>
public class LinearGradientBrushInfo : BrushInfo
{
    public string StartColor { get; set; } = "#FFFFFFFF";
    public string EndColor { get; set; } = "#00000000";
    public System.Windows.Point StartPoint { get; set; } = new System.Windows.Point(0.5, 0);
    public System.Windows.Point EndPoint { get; set; } = new System.Windows.Point(0.5, 1);

    public override Brush ToBrush()
    {
        var sc = (Color)(ColorConverter.ConvertFromString(StartColor) ?? Colors.Transparent);
        var ec = (Color)(ColorConverter.ConvertFromString(EndColor) ?? Colors.Transparent);
        return new LinearGradientBrush(sc, ec, StartPoint, EndPoint);
    }

    public static LinearGradientBrushInfo FromBrush(LinearGradientBrush brush)
    {
        return new LinearGradientBrushInfo
        {
            StartColor = brush.GradientStops[0].Color.ToString(),
            EndColor = brush.GradientStops[1].Color.ToString(),
            StartPoint = brush.StartPoint,
            EndPoint = brush.EndPoint
        };
    }
}
