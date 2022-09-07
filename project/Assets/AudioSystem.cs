using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.UI;
using GameSettings;

public class AudioSystem : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer;
    SettingsSystem settingsSystem;
    private static AudioSystem instance = null;
    private void Awake()
    {
        if (instance != null && instance != this) Destroy(this.gameObject);
        else instance = this;
        DontDestroyOnLoad(this.gameObject);
        settingsSystem = GameObject.FindGameObjectWithTag("Settings System").GetComponent<SettingsSystem>();
        onSettingsChanged();
        settingsSystem.onSettingsChanged += onSettingsChanged;
    }
    private void OnDestroy()
    {
        settingsSystem.onSettingsChanged -= onSettingsChanged;
    }
    void onSettingsChanged()
    {
        var positionProperties = settingsSystem.settings.catagories.Find(x => x.name == "Audio").properties;
        var masterVol = ((SettingsFloatProperty)positionProperties.Find(x => x.name == "Master Volume")).value;
        var sfxVol = ((SettingsFloatProperty)positionProperties.Find(x => x.name == "SFX Volume")).value;
        var inGameVol = ((SettingsFloatProperty)positionProperties.Find(x => x.name == "In Game Music Volume")).value;
        var menuVol = ((SettingsFloatProperty)positionProperties.Find(x => x.name == "Menu Music Volume")).value;
        setMasterVolume(masterVol);
        setSFXVolume(sfxVol);
        setInGameMusicVolume(inGameVol);
        setMenuMusicVolume(menuVol);
    }
    public void setMasterVolume(float volumeIndex)
    {
        if (volumeIndex < float.Epsilon) volumeIndex = 0.00001f;
        audioMixer.SetFloat("Master Volume", Mathf.Log10(volumeIndex) * 20);
    }
    public void setMenuMusicVolume(float volumeIndex)
    {
        if (volumeIndex < float.Epsilon) volumeIndex = 0.00001f;
        audioMixer.SetFloat("Menu Music Volume", Mathf.Log10(volumeIndex) * 20);
    }
    public void setInGameMusicVolume(float volumeIndex)
    {
        if (volumeIndex < float.Epsilon) volumeIndex = 0.00001f;
        audioMixer.SetFloat("In Game Music Volume", Mathf.Log10(volumeIndex) * 20);
    }
    public void setSFXVolume(float volumeIndex)
    {
        if (volumeIndex < float.Epsilon) volumeIndex = 0.00001f;
        audioMixer.SetFloat("SFX Volume", Mathf.Log10(volumeIndex) * 20);
    }
}

