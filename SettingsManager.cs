using System.IO;
using System.Text.Json;

namespace SmokeScreen;

public static class SettingsManager
{
    // 設定ファイルのパスを定義します。
    private static readonly string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SmokeScreen");
    private static readonly string SettingsFilePath = Path.Combine(AppDataPath, "settings.json");

    /// <summary>
    /// 設定をファイルに保存します。
    /// </summary>
    public static void SaveSettings(AppSettings settings)
    {
        // ディレクトリが存在しない場合は作成します。
        Directory.CreateDirectory(AppDataPath);

        var options = new JsonSerializerOptions { WriteIndented = true };
        var jsonString = JsonSerializer.Serialize(settings, options);
        File.WriteAllText(SettingsFilePath, jsonString);
    }

    /// <summary>
    /// ファイルから設定を読み込みます。
    /// </summary>
    public static AppSettings? LoadSettings()
    {
        if (!File.Exists(SettingsFilePath))
        {
            return null;
        }

        var jsonString = File.ReadAllText(SettingsFilePath);
        return JsonSerializer.Deserialize<AppSettings>(jsonString);
    }
}

/// <summary>
/// 保存する設定項目を定義するクラスです。
/// </summary>
public class AppSettings
{
    public double Top { get; set; }
    public double Left { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
}
