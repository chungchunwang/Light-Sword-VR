using TMPro;
using UnityEngine;
using GameSettings;
using UnityEngine.UI;

public class SettingsBoolPropertyObject : SettingsPropertyObject
{
    [SerializeField]Toggle toggle;
    [SerializeField]TMP_Text nameLabel;
    void Start()
    {
        SettingsBoolProperty settingsProperty = (SettingsBoolProperty)this.settingsProperty;
        nameLabel.text = settingsProperty.name;
        toggle.isOn = (settingsProperty).value;
        toggle.onValueChanged.AddListener((value) => 
        {
            settingsProperty.value = value;
            saveSettings();
        });
    }
}
