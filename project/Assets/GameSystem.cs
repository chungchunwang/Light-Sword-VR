using GameSettings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI;

public class GameSystem : MonoBehaviour
{
    AudioSource audioSource;
    MapSystem mapSystem;
    StatsSystem statsSystem;
    PointSystem pointSystem;
    DifficultyBeatmap currentMap;
    [SerializeField] GameObject spawnGrid;
    [SerializeField] GameObject pauseMenu;
    SpawnGridManager spawnGridManager;

    [SerializeField] float blockTravelTime = 5f;
    [SerializeField] InputActionReference pauseButton;
    [SerializeField] GameObject gameControllers, menuControllers, preSliceGameControllers;

    [SerializeField] LevelFailedMenuManager levelFailedMenuManager;

    GameObject blocksParent;
    GameObject debrisParent;

    public delegate void onPauseAction();
    public event onPauseAction onPause;

    public delegate void onContinueAction();
    public event onContinueAction onContinue;

    SettingsSystem settingsSystem;

    float startTime;
    float beatInSeconds;
    // Start is called before the first frame update
    void Start()
    {
        settingsSystem = GameObject.FindGameObjectWithTag("Settings System").GetComponent<SettingsSystem>();
        blocksParent = GameObject.FindGameObjectWithTag("Blocks");
        debrisParent = GameObject.FindGameObjectWithTag("Debris");
        activateGameControllers();
        pauseMenu.SetActive(false);
        pauseButton.action.performed += pauseGameActionCallback;
        startTime = Time.time;
        audioSource = GetComponent<AudioSource>();
        mapSystem = GameObject.FindGameObjectWithTag("Map System").GetComponent<MapSystem>();
        statsSystem = GameObject.FindGameObjectWithTag("Stats System").GetComponent<StatsSystem>();
        pointSystem = GameObject.FindGameObjectWithTag("Point System").GetComponent<PointSystem>();
        

        beatInSeconds = 60f / (float)mapSystem.mapsInfo[mapSystem.currentMap]._beatsPerMinute;
        currentMap = mapSystem.mapsInfo[mapSystem.currentMap]._difficultyBeatmapSets[0]._difficultyBeatmaps.Find(x => x._difficulty == mapSystem.currentHardness.ToString());
        audioSource.clip = mapSystem.mapsInfo[mapSystem.currentMap].song;
        spawnGrid.transform.position = new Vector3(0, spawnGrid.transform.position.y, (float)currentMap._noteJumpMovementSpeed * blockTravelTime);
        spawnGridManager = spawnGrid.GetComponent<SpawnGridManager>();
        StartCoroutine(playMusic());
        StartCoroutine(noteCycle());
        StartCoroutine(obstacleCycle());
    }
    private void OnDestroy()
    {
        pauseButton.action.performed -= pauseGameActionCallback;
    }

    IEnumerator noteCycle()
        {
            for(int i = 0; i< currentMap.difficultyFile._notes.Count; i++)
            {
                Note firstNote = currentMap.difficultyFile._notes[i];
                float currentTime = Time.time - startTime;
                float activationTime = firstNote._time * beatInSeconds - currentTime;
                //look for simultaneous notes
                int j = i + 1;
                while (j<currentMap.difficultyFile._notes.Count && currentMap.difficultyFile._notes[j]._time == firstNote._time) j++;
                yield return new WaitForSeconds(activationTime);
                for(int k = i; k< j; k++)
                {
                    Note currSpawnNote = currentMap.difficultyFile._notes[k];
                    spawnGridManager.spawnOnGridAndMoveForward(currSpawnNote, currentMap._noteJumpMovementSpeed, blockTravelTime*2);
                }
            
            }
    }
    IEnumerator obstacleCycle()
    {
        for (int i = 0; i < currentMap.difficultyFile._obstacles.Count; i++)
        {
            Obstacle firstObstacle = currentMap.difficultyFile._obstacles[i];
            float currentTime = Time.time - startTime;
            float activationTime = firstObstacle._time * beatInSeconds - currentTime;
            //look for simultaneous notes
            int j = i + 1;
            while (j < currentMap.difficultyFile._obstacles.Count && currentMap.difficultyFile._obstacles[j]._time == firstObstacle._time) j++;
            yield return new WaitForSeconds(activationTime);
            for (int k = i; k < j; k++)
            {
                Obstacle currSpawnObstacle = currentMap.difficultyFile._obstacles[k];
                spawnGridManager.spawnObstacleOnGridAndMoveForward(currentMap._noteJumpMovementSpeed, currSpawnObstacle._duration, currSpawnObstacle._width, currSpawnObstacle._lineIndex, (MapSystem.ObstacleType)currSpawnObstacle._type, blockTravelTime*2+currSpawnObstacle._duration);
            }

        }
    }

    IEnumerator playMusic()
    {
        yield return new WaitForSeconds(blockTravelTime);
        audioSource.Play();
        yield return new WaitForSeconds(audioSource.clip.length);
        finishGame();
    }
    void pauseGameActionCallback(InputAction.CallbackContext context)
    {
        pauseGame();
    }
    public void pauseGame()
    {
        if (onPause != null) onPause();
        blocksParent.SetActive(false);
        debrisParent.SetActive(false);
        activateMenuControllers();
        Time.timeScale = 0;
        AudioListener.pause = true;
        pauseMenu.SetActive(true);
    }
    public void continueGame()
    {
        if(onContinue != null) onContinue();
        blocksParent.SetActive(true);
        debrisParent.SetActive(true);
        activateGameControllers();
        Time.timeScale = 1;
        AudioListener.pause = false;
        pauseMenu.SetActive(false);
    }
    public void goToMenu()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;
        SceneManager.LoadScene("MenuScene");
    }
    public void restartGame()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;
        SceneManager.LoadScene("GameScene");
    }
    bool isEnd = false;
    public void endGame()
    {
        if (isEnd) return;
        StartCoroutine(endGameRoutine());
    }
    public void finishGame()
    {
        StartCoroutine(finishGameRoutine());
    }
    IEnumerator endGameRoutine()
    {
        StopCoroutine(playMusic());
        StopCoroutine(noteCycle());
        StopCoroutine(obstacleCycle());
        levelFailedMenuManager.enableMenu();
        blocksParent.SetActive(false);
        debrisParent.SetActive(false);
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("FailedScene");
    }
    IEnumerator finishGameRoutine()
    {
        yield return new WaitForSeconds(1);
        StopCoroutine(playMusic());
        StopCoroutine(noteCycle());
        StopCoroutine(obstacleCycle());
        //Log Stats
        UserPlaythroughStats stats = pointSystem.createStats();
        statsSystem.addStats(stats,mapSystem.mapsInfo[mapSystem.currentMap]._songName);

        SceneManager.LoadScene("WinScene");
    }
    
    void activateGameControllers()
    {
        var slicerProperty = settingsSystem.settings.catagories.Find(x => x.name == "Game Settings").properties.Find(x => x.name == "Pre-Slice");
        if (((SettingsBoolProperty)slicerProperty).value)
        {
            gameControllers.SetActive(false);
            preSliceGameControllers.SetActive(true);
            menuControllers.SetActive(false);
        }
        gameControllers.SetActive(true);
        preSliceGameControllers.SetActive(false);
        menuControllers.SetActive(false);
    }
    void activateMenuControllers()
    {
        gameControllers.SetActive(false);
        preSliceGameControllers.SetActive(false);
        menuControllers.SetActive(true);
    }
}
