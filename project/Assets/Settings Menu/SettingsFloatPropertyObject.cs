using GameSettings;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsFloatPropertyObject : SettingsPropertyObject
{
    [SerializeField] Button decrementButton;
    [SerializeField] Button incrementButton;
    [SerializeField]TMP_Text numberLabel;
    [SerializeField] TMP_Text nameLabel;
    [SerializeField] Slider slider;
    SettingsFloatProperty prop;
    void Start()
    {
        SettingsFloatProperty settingsProperty = (SettingsFloatProperty)this.settingsProperty;
        prop = settingsProperty;

        nameLabel.text = settingsProperty.name;
        numberLabel.text = settingsProperty.value.ToString();
        decrementButton.onClick.AddListener(onDecrementClicked);
        incrementButton.onClick.AddListener(onIncrementClicked);
        slider.wholeNumbers = true;
        slider.maxValue = Mathf.RoundToInt(settingsProperty.highBound / settingsProperty.step);
        slider.minValue = Mathf.RoundToInt(settingsProperty.lowBound / settingsProperty.step);
        slider.value = Mathf.RoundToInt(settingsProperty.value / settingsProperty.step);
        slider.onValueChanged.AddListener(onSliderValueChanged);
    }
    //round to nearest step 
    float roundToStep(float number, float step)
    {
        return Mathf.Round(number / step) * step;
    }
    void onDecrementClicked()
    {
        float newValue = roundToStep(prop.value - prop.step, prop.step);
        if(newValue <= prop.lowBound) newValue = prop.lowBound;
        prop.value = newValue;
        numberLabel.text = prop.value.ToString();
        slider.value = Mathf.RoundToInt(newValue / prop.step);
        saveSettings();
    }
    void onIncrementClicked()
    {
        float newValue = roundToStep(prop.value + prop.step, prop.step);
        if (newValue >= prop.highBound) newValue = prop.highBound;
        prop.value = newValue;
        numberLabel.text = prop.value.ToString();
        slider.value = Mathf.RoundToInt(newValue / prop.step);
        saveSettings();
    }
    void onSliderValueChanged(float value)
    {
        float newValue = roundToStep(value * prop.step, prop.step);
        if (newValue >= prop.highBound) newValue = prop.highBound;
        if (newValue <= prop.lowBound) newValue = prop.lowBound;
        prop.value = newValue;
        numberLabel.text = prop.value.ToString();
        //slider.value = Mathf.RoundToInt(newValue / prop.step);
        saveSettings();
    }
}
