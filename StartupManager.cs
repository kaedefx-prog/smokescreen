using Microsoft.Win32;
using System.Reflection;

namespace SmokeScreen;

public static class StartupManager
{
    private const string AppName = "SmokeScreen";
    private static readonly string? AppPath = Assembly.GetExecutingAssembly().Location;

    // レジストリキーのパス
    private const string RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    /// <summary>
    /// スタートアップに登録されているか確認します。
    /// </summary>
    public static bool IsInStartup()
    {
        using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath, false))
        {
            return key?.GetValue(AppName) != null;
        }
    }

    /// <summary>
    /// スタートアップに登録します。
    /// </summary>
    public static void AddToStartup()
    {
        if (string.IsNullOrEmpty(AppPath)) return;

        using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath, true))
        {
            key?.SetValue(AppName, AppPath);
        }
    }

    /// <summary>
    /// スタートアップから削除します。
    /// </summary>
    public static void RemoveFromStartup()
    {
        using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath, true))
        {
            key?.DeleteValue(AppName, false);
        }
    }
}
