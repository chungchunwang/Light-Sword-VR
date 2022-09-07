using GameSettings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SettingsCatagorySelectButton : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] TMP_Text textLabel;
    [HideInInspector] public SettingsViewer settingsViewer;
    [HideInInspector] public SettingsCatagory settingsCatagory;
    private void Start()
    {
        settingsViewer = GetComponentInParent<SettingsViewer>();
        button.onClick.AddListener(onButtonClick);
        textLabel.text = settingsCatagory.name;
    }
    private void OnDisable()
    {
        button.onClick.RemoveListener(onButtonClick);
    }
    public void onButtonClick()
    {
        settingsViewer.changeCatagory(settingsCatagory);
    }
}
