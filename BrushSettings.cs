using System.Text.Json.Serialization;
using System.Windows.Media;

namespace SmokeScreen;

/// <summary>
/// シリアライズ可能なブラシ設定の基本クラスです。
/// </summary>
[JsonDerivedType(typeof(SolidBrushInfo), typeDiscriminator: "Solid")]
// 今後グラデーションを追加するための準備
// [JsonDerivedType(typeof(LinearGradientBrushInfo), typeDiscriminator: "Linear")]
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
