using System.IO;
using System;
using UnityEngine;

public class ProjectSettings : MonoBehaviour
{
    public static string savePath;
    public static string settingsFilePath = "userData/userSettings.json";
    public static string statsFilePath = "userData/userStats.json";
    public static string mapsDirPath = "maps";
    private static ProjectSettings instance = null;
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
            savePath = Application.persistentDataPath;
            instance = this;
            //#if UNITY_EDITOR
            //savePath = Application.dataPath;
            //#endif
        }
    }
}
