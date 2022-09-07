using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace GameSettings
{
    public class Settings
    {
        public List<SettingsCatagory> catagories;
        public Settings()
        {
            catagories = new List<SettingsCatagory>();
        }
        public Settings clone()
        {
            string settingsJSON = JsonConvert.SerializeObject(this, Formatting.None, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
            Settings settings = JsonConvert.DeserializeObject<Settings>(settingsJSON, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
            return settings;
        }
        public static Settings getFromFile(string path)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            if (!File.Exists(path)) return null;
            string settingsJSON = File.ReadAllText(path);
            Settings settings = JsonConvert.DeserializeObject<Settings>(settingsJSON, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
            return settings;
        }
        public static void saveToFile(string path, Settings obj)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            string settingsJSON = JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
            File.WriteAllText(path, settingsJSON);
        }
    }
    public class SettingsCatagory
    {
        public string name;
        public List<SettingsProperty> properties;
        public SettingsCatagory(string name)
        {
            this.name = name;
            properties = new List<SettingsProperty>();
        }
    }
    public abstract class SettingsProperty
    {
        public string name;
        public string tag;
    }
    public class SettingsBoolProperty : SettingsProperty
    {
        public bool value;
        public SettingsBoolProperty(string name, bool value, string tag = "boolSettingsProperty")
        {
            this.name = name;
            this.value = value;
            this.tag = tag;
        }
    }
    public class SettingsFloatProperty : SettingsProperty
    {
        public SettingsFloatProperty(string name, float value, float lowBound, float highBound, float step, string tag = "floatSettingsProperty")
        {
            this.name = name;
            this.value = value;
            this.lowBound = lowBound;
            this.highBound = highBound;
            this.tag = tag;
            this.step = step;
        }
        public float value;
        public float lowBound;
        public float highBound;
        public float step;
    }
    public class SettingsHeaderProperty : SettingsProperty
    {
        public SettingsHeaderProperty(string name, string tag = "headerSettingsProperty")
        {
            this.name = name;
            this.tag = tag;
        }
    }

}