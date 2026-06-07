using System;
using System.IO;
using System.Text.Json;

namespace CommunityHub.Ui.Settings;

// Globalna podesavanja aplikacije — perzistuju se u JSON fajl u AppData
public static class AppSettings
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "CommunityHub",
        "settings.json");

    public static bool TooltipsEnabled { get; set; } = true;

    // Pozvati pri pokretanju aplikacije (u App.xaml.cs)
    public static void Load()
    {
        try
        {
            if (!File.Exists(SettingsPath)) return;
            string json = File.ReadAllText(SettingsPath);
            SettingsData? data = JsonSerializer.Deserialize<SettingsData>(json);
            if (data == null) return;
            TooltipsEnabled = data.TooltipsEnabled;
        }
        catch
        {
            // Ako fajl ne postoji ili je corrupted — koristi default vrednosti
        }
    }

    // Sačuva trenutne vrednosti u JSON
    public static void Save()
    {
        try
        {
            string? dir = Path.GetDirectoryName(SettingsPath);
            if (dir != null) Directory.CreateDirectory(dir);

            SettingsData data = new SettingsData { TooltipsEnabled = TooltipsEnabled };
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Ako ne moze da upise — ignorisi (npr. read-only profil)
        }
    }

    private class SettingsData
    {
        public bool TooltipsEnabled { get; set; } = true;
    }
}
