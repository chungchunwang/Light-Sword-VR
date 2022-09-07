using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinMenuManager : MonoBehaviour
{
    [SerializeField] Image songCoverImage;
    [SerializeField] TMP_Text songTitleLabel;
    [SerializeField] TMP_Text songArtistLabel;
    [SerializeField] TMP_Text songDifficultyLabel;
    [SerializeField] TMP_Text goodCutsLabel;
    [SerializeField] TMP_Text numNotesLabel;
    [SerializeField] TMP_Text maxComboLabel;
    [SerializeField] TMP_Text scoreLabel;
    [SerializeField] TMP_Text rankLabel;
    void Start()
    {
        MapSystem mapSystem = GameObject.FindGameObjectWithTag("Map System").GetComponent<MapSystem>();
        MapInfo currentMap = mapSystem.mapsInfo[mapSystem.currentMap];
        songCoverImage.sprite = currentMap.coverImage;
        songTitleLabel.text = currentMap.fullSongName;
        songArtistLabel.text = currentMap._songAuthorName;
        string difficulty = mapSystem.currentHardness.ToString();
        if (mapSystem.currentHardness == MapSystem.Hardness.ExpertPlus) difficulty = "Expert+";
        songDifficultyLabel.text = difficulty;

        int numNotes = mapSystem.mapsInfo[mapSystem.currentMap]._difficultyBeatmapSets[0]._difficultyBeatmaps.Find(x => x._difficulty.Equals(mapSystem.currentHardness.ToString())).difficultyFile.numNotes;
        StatsSystem statsSystem = GameObject.FindGameObjectWithTag("Stats System").GetComponent<StatsSystem>();
        UserPlaythroughStats lastAddition = statsSystem.getLastAddition;
        goodCutsLabel.text = lastAddition.goodCuts.ToString();
        numNotesLabel.text = numNotes.ToString();
        maxComboLabel.text += lastAddition.maxCombo.ToString();
        scoreLabel.text = lastAddition.points.ToString();
        rankLabel.text = lastAddition.rank.ToString();
    }

    public void onRestartClicked()
    {
        SceneManager.LoadScene("GameScene");
    }
    public void onContinueClicked()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
