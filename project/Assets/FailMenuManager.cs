using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FailMenuManager : MonoBehaviour
{
    [SerializeField] Image songCoverImage;
    [SerializeField] TMP_Text songTitleLabel;
    [SerializeField] TMP_Text songArtistLabel;
    [SerializeField] TMP_Text songDifficultyLabel;
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
