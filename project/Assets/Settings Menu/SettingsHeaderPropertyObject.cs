using GameSettings;
using TMPro;
using UnityEngine;

public class SettingsHeaderPropertyObject : SettingsPropertyObject
{
    [SerializeField] TMP_Text nameLabel;
    void Start()
    {
        SettingsHeaderProperty settingsProperty = (SettingsHeaderProperty)this.settingsProperty;
        nameLabel.text = settingsProperty.name;
    }
}
