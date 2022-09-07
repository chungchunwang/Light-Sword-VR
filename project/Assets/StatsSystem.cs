using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StatsSystem : MonoBehaviour, awaitableProcess
{
    private static StatsSystem instance = null;
    private bool processed = false;
    UserStats userStats;
    string path;

    UserPlaythroughStats lastAddition = null;

    public bool getIsProcessed { get => processed;}
    public UserPlaythroughStats getLastAddition { get => lastAddition;}

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        if (instance == null)
        {
            path = Path.Combine(ProjectSettings.savePath , ProjectSettings.statsFilePath);
            userStats = processStats();
            instance = this;
            processed = true;
        }
    }
    UserStats processStats()
    {
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }
        if (!File.Exists(path)) return new UserStats();
        string statsJSON = File.ReadAllText(path);
        UserStats userStats = JsonUtility.FromJson<UserStats>(statsJSON);
        if (userStats == null) return new UserStats();
        return userStats;
    }
    public void saveStats(UserStats stats)
    {
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }
        string statsJSON = JsonUtility.ToJson(userStats);
        File.WriteAllText(path, statsJSON);
    }
    public void addStats(UserPlaythroughStats userPlaythroughStats, string song) {
        lastAddition = userPlaythroughStats;
        UserSongStats userSongStats = userStats.songStats.Find(x=>song == x.songName);
        if (userSongStats == null)
        {
            userSongStats = new UserSongStats();
            userSongStats.songName = song;
            userStats.songStats.Add(userSongStats);
        }
        userSongStats.playthroughs.Add(userPlaythroughStats);
        saveStats(userStats);
    }
    public UserSongStats getStatsForSong(string song)
    {
        UserSongStats userSongStats = userStats.songStats.Find(x => song == x.songName);
        if (userSongStats == null)
        {
            userSongStats = new UserSongStats();
            userSongStats.songName = song;
            userStats.songStats.Add(userSongStats);
            saveStats(userStats);
        }
        return userSongStats;
    }
    public int getSongHighScore(string song)
    {
        UserSongStats userSongStats = getStatsForSong(song);
        int highScore = 0;
        foreach(UserPlaythroughStats playthrough in userSongStats.playthroughs)
        {
            if (playthrough.points > highScore)
            {
                highScore = playthrough.points;
            }
        }
        return highScore;
    }
    public int getSongMaxCombo(string song)
    {
        UserSongStats userSongStats = getStatsForSong(song);
        int maxCombo = 0;
        foreach (UserPlaythroughStats playthrough in userSongStats.playthroughs)
        {
            if (playthrough.maxCombo > maxCombo)
            {
                maxCombo = playthrough.maxCombo;
            }
        }
        return maxCombo;
    }
    public UserPlaythroughStats.Rank getSongMaxRank(string song)
    {
        UserSongStats userSongStats = getStatsForSong(song);
        UserPlaythroughStats.Rank maxRank = 0;
        foreach (UserPlaythroughStats playthrough in userSongStats.playthroughs)
        {
            if ((int) playthrough.rank > (int) maxRank)
            {
                maxRank = playthrough.rank;
            }
        }
        return maxRank;
    }
}
[Serializable]
public class UserStats
{
    public UserStats()
    {
        songStats = new List<UserSongStats>();
    }
    public List<UserSongStats> songStats;
}
[Serializable]
public class UserSongStats
{
    public string songName;
    public List<UserPlaythroughStats> playthroughs;
    public UserSongStats()
    {
        playthroughs = new List<UserPlaythroughStats>();
    }
}
[Serializable]
public class UserPlaythroughStats
{
    public int maxCombo;
    public int points;
    public int goodCuts;
    public Rank rank;
    public enum Rank
    {
        SSS = 8,
        SS = 7,
    S = 6,
    A = 5,
    B = 4,
    C = 3,
    D = 2,
    E = 1,
    NoRank = 0
    }
    static public Rank accuracyToRank(float accuracy)
    {
        if (accuracy >= 1f) return Rank.SSS;
        if (accuracy >= .9f) return Rank.SS;
        if (accuracy >= 8f) return Rank.S;
        if (accuracy >= .65f) return Rank.A;
        if (accuracy >= .5f) return Rank.B;
        if (accuracy >= .35f) return Rank.C;
        if (accuracy >= .2f) return Rank.D;
        return Rank.E;
    }
}
