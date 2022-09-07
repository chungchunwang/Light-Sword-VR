using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Button))]
public class HardnessButtonManager : MonoBehaviour
{
    [SerializeField] MapSystem.Hardness hardness;
    [SerializeField, ColorUsage(true)] Color selectColor;
    [SerializeField, ColorUsage(true)] Color deselectColor;
    Image buttonBackground;
    Button button;
    MapSystem mapSystem;
    void Start()
    {
        mapSystem = GameObject.FindGameObjectWithTag("Map System").GetComponent<MapSystem>();
        buttonBackground = GetComponent<Image>();
        button = GetComponent<Button>();
        updateButtonSettings();
        mapSystem.hardnessChanged += onCurrentHardnessChanged;
        mapSystem.mapChanged += onCurrentMapChanged;
    }
    private void OnDestroy()
    {
        mapSystem.hardnessChanged -= onCurrentHardnessChanged;
        mapSystem.mapChanged -= onCurrentMapChanged;
    }

    private void updateButtonSettings()
    {
        if (mapSystem.currentMap < 0) return;
        button.interactable = mapSystem.mapsInfo[mapSystem.currentMap]._difficultyBeatmapSets[0]._difficultyBeatmaps.Find(x => x._difficulty.Equals(hardness.ToString())) != null;
        if (mapSystem.currentHardness == hardness)
        {
            buttonBackground.color = selectColor;
        }
        else buttonBackground.color = deselectColor;
    }

    public void onPress()
    {
        mapSystem.setCurrentHardness(hardness);
    }
    void onCurrentHardnessChanged()
    {
        updateButtonSettings();
    }
    void onCurrentMapChanged()
    {
        updateButtonSettings();
    }
    void Update()
    {
        
    }
}
