using GameSettings;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SettingsSystemHandler : MonoBehaviour
{
    void OnEnable()
    {
        //Game Settings
        SettingsCatagory gameSettingsCatagory = new SettingsCatagory("Game Settings");
        SettingsBoolProperty noFail = new SettingsBoolProperty("No Fail", false);
        SettingsBoolProperty realisticSlice = new SettingsBoolProperty("Pre-Slice", true);
        gameSettingsCatagory.properties.Add(noFail);
        gameSettingsCatagory.properties.Add(realisticSlice);
        //Audio
        SettingsCatagory audioSettingsCatagory = new SettingsCatagory("Audio");
        SettingsFloatProperty masterVolume = new SettingsFloatProperty("Master Volume", .5f, 0, 1, .1f);
        SettingsFloatProperty sfxVolume = new SettingsFloatProperty("SFX Volume", .5f, 0, 1, .1f);
        SettingsFloatProperty inGameMusicVolume = new SettingsFloatProperty("In Game Music Volume", .5f, 0, 1, .1f);
        SettingsFloatProperty menuMusicVolume = new SettingsFloatProperty("Menu Music Volume", .5f, 0, 1, .1f);
        audioSettingsCatagory.properties.Add(masterVolume);
        audioSettingsCatagory.properties.Add(sfxVolume);
        audioSettingsCatagory.properties.Add(inGameMusicVolume);
        audioSettingsCatagory.properties.Add(menuMusicVolume);

        //Positioning
        SettingsCatagory positioningSettingsCatagory = new SettingsCatagory("Positioning");
        SettingsHeaderProperty positionLabel = new SettingsHeaderProperty("Position");
        SettingsFloatProperty positionXOffset = new SettingsFloatProperty("Position X Offset", 0, -1, 1, .1f);
        SettingsFloatProperty positionYOffset = new SettingsFloatProperty("Position Y Offset", 0, -1, 1, .1f);
        SettingsFloatProperty positionZOffset = new SettingsFloatProperty("Position Z Offset", 0, -1, 1, .1f);
        SettingsHeaderProperty rotationLabel = new SettingsHeaderProperty("Rotation");
        SettingsFloatProperty rotationXOffset = new SettingsFloatProperty("Rotation X Offset", 0, 0, 360, 1);
        SettingsFloatProperty rotationYOffset = new SettingsFloatProperty("Rotation Y Offset", 0, 0, 360, 1);
        SettingsFloatProperty rotationZOffset = new SettingsFloatProperty("Rotation Z Offset", 0, 0, 360, 1);
        positioningSettingsCatagory.properties.Add(positionLabel);
        positioningSettingsCatagory.properties.Add(positionXOffset);
        positioningSettingsCatagory.properties.Add(positionYOffset);
        positioningSettingsCatagory.properties.Add(positionZOffset);
        positioningSettingsCatagory.properties.Add(rotationLabel);
        positioningSettingsCatagory.properties.Add(rotationXOffset);
        positioningSettingsCatagory.properties.Add(rotationYOffset);
        positioningSettingsCatagory.properties.Add(rotationZOffset);

        //Create Settings
        Settings settings = new Settings();
        settings.catagories.Add(gameSettingsCatagory);
        settings.catagories.Add(audioSettingsCatagory);
        settings.catagories.Add(positioningSettingsCatagory);
        SettingsSystem.defaultSettings = settings;
        SettingsSystem.path = Path.Combine(ProjectSettings.savePath, ProjectSettings.settingsFilePath);
    }
}
