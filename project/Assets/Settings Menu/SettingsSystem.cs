using UnityEngine;
using GameSettings;
using System;
using System.Diagnostics;

public class SettingsSystem : MonoBehaviour
{
    public Settings settings;
    public static Settings defaultSettings;
    public static string path;
    public delegate void settingsChangedAction();
    public event settingsChangedAction onSettingsChanged;
    private static SettingsSystem instance = null;
    private void OnEnable()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        if (instance == null)
        {
            instance = this;
            if (path == null) throw new MissingFieldException("Missing 'path' in Settings System");
            if (defaultSettings == null) throw new MissingFieldException("Missing 'defaultSettings' in Settings System");
            settings = Settings.getFromFile(path);
            if (settings == null) settings = CreateBlankSettings();
        }
    }
    public void saveSettings()
    {
        Settings.saveToFile(path, settings);
        onSettingsChanged();
    }
    public Settings getSettings() { return settings; }
    public Action getSaveSettings() { return saveSettings; }
    private static Settings CreateBlankSettings()
    {
        return defaultSettings.clone();
    }
}
