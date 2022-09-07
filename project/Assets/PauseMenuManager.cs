using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] Image songCoverImage;
    [SerializeField] TMP_Text songTitleLabel;
    [SerializeField] TMP_Text songArtistLabel;
    [SerializeField] TMP_Text songDifficultyLabel;

    [SerializeField] GameSystem gameSystem;
    void Awake()
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
        gameSystem.restartGame();
    }
    public void onMenuClicked()
    {
        gameSystem.goToMenu();
    }
    public void onContinueClicked()
    {
        gameSystem.continueGame();
    }
}
