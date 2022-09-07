using GameSettings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointSystem : MonoBehaviour
{
    int currentPoints = 0;
    int currentMultiplier = 1;
    int currentMultiplierUpgradeCombo = 0;
    int currentCombo = 0;

    int goodCuts = 0;
    int maxCombo = 0;

    float currentEnergy = 0.5f;

    public int getCurrentPoints { get => currentPoints; }
    public int getCurrentMultiplier { get => currentMultiplier; }
    public int getCurrentCombo { get => currentCombo; }
    public float getCurrentEnergy { get => currentEnergy; }

    GameSystem gameSystem;
    MapSystem mapSystem;
    SettingsSystem settingsSystem;
    [SerializeField] PointIndicatorMenu pointIndicatorMenu;

    bool noFailMode = false;

    private void Start()
    {
        gameSystem = GameObject.FindGameObjectWithTag("Game System").GetComponent<GameSystem>();
        mapSystem = GameObject.FindGameObjectWithTag("Map System").GetComponent<MapSystem>();
        settingsSystem = GameObject.FindGameObjectWithTag("Settings System").GetComponent<SettingsSystem>();

        noFailMode = ((SettingsBoolProperty)settingsSystem.settings.catagories.Find(x => x.name.Equals("Game Settings")).properties.Find(x => x.name.Equals("No Fail"))).value;
        if(noFailMode) currentEnergy = 1;
    }

    public UserPlaythroughStats createStats()
    {
        UserPlaythroughStats stats = new UserPlaythroughStats();
        stats.points = currentPoints;
        stats.goodCuts = goodCuts;
        stats.maxCombo = maxCombo;
        int numNotes = mapSystem.mapsInfo[mapSystem.currentMap]._difficultyBeatmapSets[0]._difficultyBeatmaps.Find(x => x._difficulty.Equals(mapSystem.currentHardness.ToString())).difficultyFile.numNotes;
        stats.rank = UserPlaythroughStats.accuracyToRank((float)goodCuts / (float)numNotes);
        return stats;
    }

    public int getCurrentMultiplierUpgradePercentage()
    {
        if (currentMultiplier >= 8) return 0;
        return currentMultiplierUpgradeCombo / (currentMultiplier * 2);
    }
    public void logPoints(int amount, Note note)
    {
        goodCuts++;
        incrementCurrentEnergyBy(.01f);
        incrementCombo();
        currentPoints += amount * currentMultiplier;
        pointIndicatorMenu.spawnInColumn(amount,note._lineIndex);
    }
    public void logBadCut()
    {
        decrementCurrentEnergyBy(.1f);
        resetCombo();
    }
    public void logMiss()
    {
        decrementCurrentEnergyBy(.15f);
        resetCombo();
    }
    public void logBombHit()
    {
        decrementCurrentEnergyBy(.15f);
    }
    public void logBarrier(float time)
    {
        decrementCurrentEnergyBy(1.3f * time);
    }
    void incrementCurrentEnergyBy(float amount)
    {
        if(noFailMode) return;
        currentEnergy += amount;
        if (currentEnergy > 1) currentEnergy = 1;
    }
    void decrementCurrentEnergyBy(float amount)
    {
        if(noFailMode) return;
        currentEnergy -= amount;
        if (currentEnergy < 0) {
            gameSystem.endGame(); 
        }
    }

    void incrementCombo() {
        currentCombo++;
        if (currentCombo > maxCombo) maxCombo = currentCombo;
        currentMultiplierUpgradeCombo++;
        if (currentMultiplierUpgradeCombo == (currentMultiplier * 2)) upgradeMultiplier();
    }
    void resetCombo()
    {
        currentCombo = 0;
        currentMultiplierUpgradeCombo = 0;
        downgradeMultiplier();
    }

    void upgradeMultiplier()
    {
        if (currentMultiplier >= 8) return;
        currentMultiplier *= 2;
    }
    void downgradeMultiplier()
    {
        if (currentMultiplier <= 1) return;
        currentMultiplier /= 2;
    }
}
