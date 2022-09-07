using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.IO;
using System;

public class DescriptionManager : MonoBehaviour
{
    MapSystem mapSystem;
    [SerializeField] Image coverImage;
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text songAuthorText;
    [SerializeField] TMP_Text mapAuthorText;
    [SerializeField] TMP_Text bpmText;
    [SerializeField] TMP_Text lengthText;
    [SerializeField] TMP_Text blocksText;
    [SerializeField] TMP_Text wallsText;
    [SerializeField] TMP_Text bombsText;

    MapInfo mapInfo;
    // Start is called before the first frame update
    private void Start()
    {
        mapSystem = GameObject.FindGameObjectWithTag("Map System").GetComponent<MapSystem>();
        mapSystem.mapChanged += updateDisplay;
        mapSystem.hardnessChanged += updateDisplay;
        updateDisplay();
    }
    private void OnDestroy()
    {
        mapSystem.hardnessChanged -= updateDisplay;
        mapSystem.mapChanged -= updateDisplay;
    }
    private void updateDisplay()
    {
        if (mapSystem.currentMap < 0) return;
        mapInfo = mapSystem.mapsInfo[mapSystem.currentMap];

        lengthText.text = TimeSpan.FromSeconds(mapInfo.song.length).ToString(@"mm\:ss");
        coverImage.sprite = mapInfo.coverImage;
        titleText.text = mapInfo.fullSongName;
        songAuthorText.text = mapInfo._songAuthorName;
        mapAuthorText.text = mapInfo._levelAuthorName;

        bpmText.text = mapInfo._beatsPerMinute.ToString();
        blocksText.text = mapInfo._difficultyBeatmapSets[0]._difficultyBeatmaps.Find(x=>x._difficulty.Equals(mapSystem.currentHardness.ToString())).difficultyFile.numNotes.ToString();
        wallsText.text = mapInfo._difficultyBeatmapSets[0]._difficultyBeatmaps.Find(x => x._difficulty.Equals(mapSystem.currentHardness.ToString())).difficultyFile._obstacles.Count.ToString();
        bombsText.text = mapInfo._difficultyBeatmapSets[0]._difficultyBeatmaps.Find(x => x._difficulty.Equals(mapSystem.currentHardness.ToString())).difficultyFile.numBombs.ToString();
    }
}
