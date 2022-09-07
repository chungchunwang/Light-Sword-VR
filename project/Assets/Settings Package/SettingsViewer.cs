using GameSettings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SettingsViewer : MonoBehaviour
{
    SettingsCatagory currentCatagory = null;
    Action saveSettings = null;
    SettingsSystem settingsSystem;
    Settings settings;
    [SerializeField] GameObject catagoryButtonPrefab = null;
    [SerializeField] Transform catagoryButtonParent;
    [SerializeField] Transform pageParent;
    [SerializeField] List<SettingsPropertyObject> propertyObjects;
    private void Start()
    {
        settingsSystem = GameObject.FindGameObjectWithTag("Settings System").GetComponent<SettingsSystem>();
        settings = settingsSystem.getSettings();
        saveSettings = settingsSystem.getSaveSettings();
        foreach (SettingsCatagory catagory in settings.catagories)
        {
            //add catagory button
            GameObject catagoryButton = Instantiate(catagoryButtonPrefab);
            SettingsCatagorySelectButton script = catagoryButton.GetComponent<SettingsCatagorySelectButton>();
            script.settingsViewer = this;
            script.settingsCatagory = catagory;
            catagoryButton.transform.SetParent(catagoryButtonParent, false);
        }
    }
    public void changeCatagory(SettingsCatagory catagory)
    {
        currentCatagory = catagory;
        loadPage();
    }
    public void loadPage()
    {
        //delete all children
        for(int i = 0; i < pageParent.childCount; i++)
        {
            Destroy(pageParent.GetChild(i).gameObject);
        }
        //add page
        foreach (var property in currentCatagory.properties)
        {
            SettingsPropertyObject s = propertyObjects.Find(x => x.tag.Equals(property.tag));
            //add property object
            GameObject propertyObject = Instantiate(s.gameObject);
            propertyObject.transform.SetParent(pageParent, false);
            propertyObject.GetComponent<SettingsPropertyObject>().settingsProperty = property;
            propertyObject.GetComponent<SettingsPropertyObject>().saveSettings = saveSettings;
        }
    }
}