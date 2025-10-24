using System.IO;
using System.Text.Json;

namespace SmokeScreen;

/// <summary>
/// Manages loading and saving of application settings.
/// </summary>
public static class SettingsManager
{
    private static readonly string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SmokeScreen");
    private static readonly string SettingsFilePath = Path.Combine(AppDataPath, "settings.json");

    /// <summary>
    /// Saves the application settings to a file.
    /// </summary>
    public static void SaveSettings(ApplicationSettings settings)
    {
        Directory.CreateDirectory(AppDataPath);
        var options = new JsonSerializerOptions { WriteIndented = true };
        var jsonString = JsonSerializer.Serialize(settings, options);
        File.WriteAllText(SettingsFilePath, jsonString);
    }

    /// <summary>
    /// Loads application settings from a file.
    /// </summary>
    public static ApplicationSettings? LoadSettings()
    {
        if (!File.Exists(SettingsFilePath))
        {
            return null;
        }

        var jsonString = File.ReadAllText(SettingsFilePath);
        return JsonSerializer.Deserialize<ApplicationSettings>(jsonString);
    }
}

/// <summary>
/// Defines the root object for all application settings.
/// </summary>
public class ApplicationSettings
{
    public List<PatchSetting> Patches { get; set; } = new();
}


/// <summary>
/// Defines the settings for a single patch.
/// </summary>
public class PatchSetting
{
    public double Top { get; set; }
    public double Left { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public BrushInfo? Brush { get; set; }
    public string? ClipGeometry { get; set; }
}
