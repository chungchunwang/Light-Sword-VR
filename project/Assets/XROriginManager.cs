using GameSettings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XROriginManager : MonoBehaviour
{
    SettingsSystem settingsSystem;
    void Start()
    {
        settingsSystem = GameObject.FindGameObjectWithTag("Settings System").GetComponent<SettingsSystem>();
        onSettingsChanged();
        settingsSystem.onSettingsChanged += onSettingsChanged;
    }
    void onSettingsChanged()
    {
        var positionProperties = settingsSystem.settings.catagories.Find(x => x.name == "Positioning").properties;
        var posX = ((SettingsFloatProperty) positionProperties.Find(x => x.name == "Position X Offset")).value;
        var posY = ((SettingsFloatProperty) positionProperties.Find(x => x.name == "Position Y Offset")).value;
        var posZ = ((SettingsFloatProperty) positionProperties.Find(x => x.name == "Position Z Offset")).value;

        var rotX = ((SettingsFloatProperty)positionProperties.Find(x => x.name == "Rotation X Offset")).value;
        var rotY = ((SettingsFloatProperty)positionProperties.Find(x => x.name == "Rotation Y Offset")).value;
        var rotZ = ((SettingsFloatProperty)positionProperties.Find(x => x.name == "Rotation Z Offset")).value;
        this.transform.position = new Vector3(posX, posY, posZ);
        this.transform.rotation = Quaternion.Euler(new Vector3(rotX, rotY, rotZ));
    }
    private void OnDestroy()
    {
        settingsSystem.onSettingsChanged -= onSettingsChanged;
    }
}
