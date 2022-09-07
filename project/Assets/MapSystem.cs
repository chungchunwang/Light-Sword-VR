using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class MapSystem : MonoBehaviour, awaitableProcess
{
    public enum Hardness
    {
        Easy,
        Normal,
        Hard,
        Expert,
        ExpertPlus
    }
    public enum NoteType
    {
        LeftNote = 0,
        RightNote = 1,
        Bomb = 3
    }
    public enum CutDirection
    {
        Up,
        Down,
        Left,
        Right,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight,
        Any
    }
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight,
        Any
    }
    public enum ObstacleType
    {
        Full,
        Crouch
    }

    public bool getIsProcessed { get => processed; }

    bool processed = false;
    private static MapSystem instance = null;
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
            StartCoroutine(ProcessMaps());
            instance = this;
        }
    }
    private void Update()
    {

    }
    private IEnumerator ProcessMaps()
    {
        string mapPath = Path.Combine(ProjectSettings.savePath, ProjectSettings.mapsDirPath);
        if (!Directory.Exists(mapPath))
        {
            Directory.CreateDirectory(mapPath);
        }
        string[] directories = Directory.GetDirectories(mapPath);
        foreach (string dir in directories)
        {
            // Process Map File

            string infoFilePath = Path.Combine(dir , "Info.dat");
            if (!File.Exists(infoFilePath)) continue;
            string infoJSON = File.ReadAllText(infoFilePath);
            MapInfo mapInfo;
            mapInfo = JsonUtility.FromJson<MapInfo>(infoJSON);
            mapInfo.folderPath = dir;
            if (mapInfo == null) continue;
            if (mapInfo._version == null || mapInfo._version != "2.0.0") continue;


            //Extensions
            //Full Song Name
            string songName = mapInfo._songName;
            if (mapInfo._songSubName.Length > 0) songName += " | " + mapInfo._songSubName;
            mapInfo.fullSongName = songName;
            
            //Cover Image
            Texture2D coverImageTexture = new Texture2D(2, 2);
            string imgPath = Path.Combine(mapInfo.folderPath,mapInfo._coverImageFilename);
            if (File.Exists(imgPath))
            {
                byte[] coverImageData = File.ReadAllBytes(imgPath);
                coverImageTexture.LoadImage(coverImageData);
            }
            Sprite coverImageSprite = Sprite.Create(coverImageTexture, new Rect(0, 0, coverImageTexture.width, coverImageTexture.height), Vector2.zero, 100);
            mapInfo.coverImage = coverImageSprite;
           
            //Audio
            string songPath = Path.Combine(mapInfo.folderPath, mapInfo._songFilename);
            AudioClip audioClip = null;
            using (UnityWebRequest songRequest = UnityWebRequestMultimedia.GetAudioClip("file://" + songPath, AudioType.OGGVORBIS))
            {
                yield return songRequest.SendWebRequest();

                if (songRequest.result == UnityWebRequest.Result.InProgress)
                {
                    Debug.Log($"In Progress");
                };
                if (songRequest.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log($"Connection Error");
                };
                if (songRequest.result == UnityWebRequest.Result.DataProcessingError)
                {
                    Debug.Log($"Data Processing Error");
                };
                if (songRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log($"Protocol Error");
                };
                if (songRequest.result != UnityWebRequest.Result.Success)
                {
                    audioClip = null;
                    continue;
                };
                if (songRequest.result == UnityWebRequest.Result.Success) audioClip = DownloadHandlerAudioClip.GetContent(songRequest);
            }
            if (audioClip == null) continue;
            mapInfo.song = audioClip;



            List<DifficultyBeatmapSet> sets = mapInfo._difficultyBeatmapSets;
            for (int i = 0; i < sets.Count; i++)
            {
                if (sets[i]._beatmapCharacteristicName != "Standard")
                {
                    sets.RemoveAt(i);
                    i--;
                }
                else
                {
                    foreach (DifficultyBeatmap db in sets[i]._difficultyBeatmaps)
                    {
                        string beatmapFilePath = Path.Combine(mapInfo.folderPath, db._beatmapFilename);
                        if (!File.Exists(beatmapFilePath)) continue;
                        string beatmapJSON = File.ReadAllText(beatmapFilePath);
                        DifficultyBeatmapFile beatmapFile;
                        beatmapFile = JsonUtility.FromJson<DifficultyBeatmapFile>(beatmapJSON);
                        if (beatmapFile == null) continue;
                        beatmapFile.numNotes = 0;
                        beatmapFile.numBombs = 0;
                        foreach (Note n in beatmapFile._notes)
                        {
                            if (n._type == ((int)NoteType.LeftNote) || n._type == ((int)NoteType.RightNote))
                            {
                                beatmapFile.numNotes++;
                            }
                            else
                            {
                                beatmapFile.numBombs++;
                            }
                        }
                        db.difficultyFile = beatmapFile;
                    }
                }
            }

            if (sets.Count > 0) mapsInfo.Add(mapInfo);
        }
        processed = true;
    }

    public List<MapInfo> mapsInfo = new List<MapInfo>();
    public int currentMap = -1;
    public delegate void MapChangeAction();
    public event MapChangeAction mapChanged;
    public Hardness currentHardness = Hardness.Normal;
    public delegate void HardnessChangeAction();
    public event HardnessChangeAction hardnessChanged;
    public void setCurrentMap(int index)
    {
        currentMap = index;
        Enum.TryParse<Hardness>(mapsInfo[currentMap]._difficultyBeatmapSets[0]._difficultyBeatmaps[0]._difficulty, out Hardness parsedEnum);
        setCurrentHardness(parsedEnum);
        if (mapChanged != null) mapChanged();
    }
    public void setCurrentHardness(Hardness hardness)
    {
        currentHardness = hardness;
        if (hardnessChanged != null) hardnessChanged();
    }
}

[System.Serializable]
public class MapInfo
{
    public string _version;
    public string _songName;
    public string _songSubName;
    public string _songAuthorName;
    public string _levelAuthorName;
    public float _beatsPerMinute;
    public float _previewStartTime;
    public float _previewDuration;
    public string _songFilename;
    public string _coverImageFilename;
    public float _songTimeOffset;
    public List<DifficultyBeatmapSet> _difficultyBeatmapSets;

    public string folderPath;
    public string fullSongName;
    public Sprite coverImage;
    public AudioClip song;

}
[System.Serializable]
public class DifficultyBeatmapSet
{
    public string _beatmapCharacteristicName;
    public List<DifficultyBeatmap> _difficultyBeatmaps;
}
[System.Serializable]
public class DifficultyBeatmap
{
    public string _difficulty;
    public int _difficultyRank;
    public string _beatmapFilename;
    public float _noteJumpMovementSpeed;
    public float _noteJumpStartBeatOffset;

    public DifficultyBeatmapFile difficultyFile;
}
[System.Serializable]
public class DifficultyBeatmapFile
{
    public string _version;
    public List<Note> _notes;
    public List<Obstacle> _obstacles;
    public List<Event> _events;

    public int numNotes;
    public int numBombs;
}
[System.Serializable]
public class Note
{
    public int _time;
    public int _lineIndex;
    public int _lineLayer;
    public int _type;
    public int _cutDirection;
}
[System.Serializable]
public class Obstacle
{
    public int _time;
    public int _lineIndex;
    public int _type;
    public int _duration;
    public int _width;
}




//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.Networking;
//using UnityEngine.SceneManagement;

//public class MapSystem : MonoBehaviour, awaitableProcess
//{
//    public enum Hardness
//    {
//        Easy,
//        Normal,
//        Hard,
//        Expert,
//        ExpertPlus
//    }
//    public enum NoteType
//    {
//        LeftNote = 0,
//        RightNote = 1,
//        Bomb = 3
//    }
//    public enum CutDirection
//    {
//        Up,
//        Down,
//        Left,
//        Right,
//        UpLeft,
//        UpRight,
//        DownLeft,
//        DownRight,
//        Any
//    }
//    public enum Direction
//    {
//        Up,
//        Down,
//        Left,
//        Right,
//        UpLeft,
//        UpRight,
//        DownLeft,
//        DownRight,
//        Any
//    }
//    public enum ObstacleType
//    {
//        Full,
//        Crouch
//    }

//    public bool getIsProcessed { get => processed; }

//    bool processed = false;
//    private static MapSystem instance = null;
//    private void Awake()
//    {
//        if (instance != null && instance != this)
//        {
//            Destroy(this.gameObject);
//            return;
//        }
//        DontDestroyOnLoad(gameObject);
//        if (instance == null)
//        {
//            StartCoroutine(ProcessMaps());
//            instance = this;
//        }
//    }
//    private void Update()
//    {

//    }
//    private IEnumerator ProcessMaps()
//    {
//        string mapPath = ProjectSettings.savePath + ProjectSettings.mapsDirPath;
//        if (!Directory.Exists(mapPath))
//        {
//            Directory.CreateDirectory(mapPath);
//        }
//        string[] directories = Directory.GetDirectories(mapPath);
//        foreach (string dir in directories)
//        {
//            // Process Map File

//            string infoFilePath = dir + "/Info.dat";
//            if (!File.Exists(infoFilePath))  break;
//            string infoJSON = File.ReadAllText(infoFilePath);
//            MapInfo mapInfo;
//            mapInfo = JsonUtility.FromJson<MapInfo>(infoJSON);
//            mapInfo.folderPath = dir;
//            if (mapInfo == null)  break;
//            if (mapInfo._version == null || mapInfo._version != "2.0.0")  break;


//            //Extensions
//            //Full Song Name
//            string songName = mapInfo._songName;
//            if (mapInfo._songSubName.Length > 0) songName += " | " + mapInfo._songSubName;
//            mapInfo.fullSongName = songName;
//            //Cover Image
//            Texture2D coverImageTexture = new Texture2D(2, 2);
//            string imgPath = mapInfo.folderPath + "\\" + mapInfo._coverImageFilename;
//            if (File.Exists(imgPath))
//            {
//                byte[] coverImageData = File.ReadAllBytes(imgPath);
//                coverImageTexture.LoadImage(coverImageData);
//            }
//            Sprite coverImageSprite = Sprite.Create(coverImageTexture, new Rect(0, 0, coverImageTexture.width, coverImageTexture.height), Vector2.zero, 100);
//            mapInfo.coverImage = coverImageSprite;
//            //Audio
//            string songPath = mapInfo.folderPath + "\\" + mapInfo._songFilename;
//            AudioClip audioClip;
//            using (UnityWebRequest songRequest = UnityWebRequestMultimedia.GetAudioClip(songPath, AudioType.OGGVORBIS))
//            {
//                yield return songRequest.SendWebRequest();
//                //if (!songRequest.isDone || songRequest.result == UnityWebRequest.Result.InProgress) await Task.Delay(1);
//                Debug.Log(songPath);
//                if (songRequest.result != UnityWebRequest.Result.Success)
//                {
//                    Debug.Log($"Error processing {songPath}");
//                    audioClip = null;
//                };
//                if (songRequest.result == UnityWebRequest.Result.InProgress)
//                {
//                    Debug.Log($"In Progress");
//                };
//                if (songRequest.result == UnityWebRequest.Result.ConnectionError)
//                {
//                    Debug.Log($"Connection Error");
//                };
//                if (songRequest.result == UnityWebRequest.Result.DataProcessingError)
//                {
//                    Debug.Log($"Data Processing Error");
//                };
//                if (songRequest.result == UnityWebRequest.Result.ProtocolError)
//                {
//                    Debug.Log($"Protocol Error");
//                };

//                audioClip = DownloadHandlerAudioClip.GetContent(songRequest);
//            }
//            if (audioClip == null)  break;
//            mapInfo.song = audioClip;



//            List<DifficultyBeatmapSet> sets = mapInfo._difficultyBeatmapSets;
//            for (int i = 0; i < sets.Count; i++)
//            {
//                if (sets[i]._beatmapCharacteristicName != "Standard")
//                {
//                    sets.RemoveAt(i);
//                    i--;
//                }
//                else
//                {
//                    foreach (DifficultyBeatmap db in sets[i]._difficultyBeatmaps)
//                    {
//                        string beatmapFilePath = mapInfo.folderPath + "\\" + db._beatmapFilename;
//                        if (!File.Exists(beatmapFilePath))  break;
//                        string beatmapJSON = File.ReadAllText(beatmapFilePath);
//                        DifficultyBeatmapFile beatmapFile;
//                        beatmapFile = JsonUtility.FromJson<DifficultyBeatmapFile>(beatmapJSON);
//                        if (beatmapFile == null)  break;
//                        beatmapFile.numNotes = 0;
//                        beatmapFile.numBombs = 0;
//                        foreach (Note n in beatmapFile._notes)
//                        {
//                            if (n._type == ((int)NoteType.LeftNote) || n._type == ((int)NoteType.RightNote))
//                            {
//                                beatmapFile.numNotes++;
//                            }
//                            else
//                            {
//                                beatmapFile.numBombs++;
//                            }
//                        }
//                        db.difficultyFile = beatmapFile;
//                    }
//                }
//            }

//            if (sets.Count > 0) mapsInfo.Add(mapInfo);
//        }
//        processed = true;
//    }

//    public List<MapInfo> mapsInfo = new List<MapInfo>();
//    public int currentMap = -1;
//    public delegate void MapChangeAction();
//    public event MapChangeAction mapChanged;
//    public Hardness currentHardness = Hardness.Normal;
//    public delegate void HardnessChangeAction();
//    public event HardnessChangeAction hardnessChanged;
//    public void setCurrentMap(int index)
//    {
//        currentMap = index;
//        Enum.TryParse<Hardness>(mapsInfo[currentMap]._difficultyBeatmapSets[0]._difficultyBeatmaps[0]._difficulty, out Hardness parsedEnum);
//        setCurrentHardness(parsedEnum);
//        if (mapChanged != null) mapChanged();
//    }
//    public void setCurrentHardness(Hardness hardness)
//    {
//        currentHardness = hardness;
//        if (hardnessChanged != null) hardnessChanged();
//    }
//}

//[System.Serializable]
//public class MapInfo
//{
//    public string _version;
//    public string _songName;
//    public string _songSubName;
//    public string _songAuthorName;
//    public string _levelAuthorName;
//    public float _beatsPerMinute;
//    public float _previewStartTime;
//    public float _previewDuration;
//    public string _songFilename;
//    public string _coverImageFilename;
//    public float _songTimeOffset;
//    public List<DifficultyBeatmapSet> _difficultyBeatmapSets;

//    public string folderPath;
//    public string fullSongName;
//    public Sprite coverImage;
//    public AudioClip song;

//}
//[System.Serializable]
//public class DifficultyBeatmapSet
//{
//    public string _beatmapCharacteristicName;
//    public List<DifficultyBeatmap> _difficultyBeatmaps;
//}
//[System.Serializable]
//public class DifficultyBeatmap
//{
//    public string _difficulty;
//    public int _difficultyRank;
//    public string _beatmapFilename;
//    public float _noteJumpMovementSpeed;
//    public float _noteJumpStartBeatOffset;

//    public DifficultyBeatmapFile difficultyFile;
//}
///* Full Serializable Object
// [System.Serializable]
//public class MapInfo
//{
//    public string _version;
//    public string _songName;
//    public string _songSubName;
//    public string _songAuthorName;
//    public string _levelAuthorName;
//    public int _beatsPerMinute;
//    public int _shuffle;
//    public int _shufflePeriod;
//    public int _previewStartTime;
//    public int _previewDuration;
//    public string _songFilename;
//    public string _coverImageFilename;
//    public string _environmentName;
//    public string _allDirectionsEnvironmentName;
//    public int _songTimeOffset;
//    public DifficultyBeatmapSet[] _difficultyBeatmapSets;

//}
//[System.Serializable]
//public class DifficultyBeatmapSet
//{
//    public string _beatmapCharacteristicName;
//    public DifficultyBeatmap[] _difficultyBeatmaps;
//}
//[System.Serializable]
//public class DifficultyBeatmap
//{
//    public string _difficulty;
//    public int _difficultyRank;
//    public string _beatmapFilename;
//    public int _noteJumpMovementSpeed;
//    public int _noteJumpStartBeatOffset;
//}
//public class DifficultyFile
//{
//    public string _version;
//    public List<Note> _notes;
//    public List<Obstacle> _obstacles;
//    public List<Event> _events;
//}
//public class Note
//{
//    public int _time;
//    public int _lineIndex;
//    public int _lineLayer;
//    public int _type;
//    public int _cutDirection;
//}
//public class Obstacle
//{
//    public int _time;
//    public int _lineIndex;
//    public int _type;
//    public int _duration;
//    public int _width;
//}
// */
//[System.Serializable]
//public class DifficultyBeatmapFile
//{
//    public string _version;
//    public List<Note> _notes;
//    public List<Obstacle> _obstacles;
//    public List<Event> _events;

//    public int numNotes;
//    public int numBombs;
//}
//[System.Serializable]
//public class Note
//{
//    public int _time;
//    public int _lineIndex;
//    public int _lineLayer;
//    public int _type;
//    public int _cutDirection;
//}
//[System.Serializable]
//public class Obstacle
//{
//    public int _time;
//    public int _lineIndex;
//    public int _type;
//    public int _duration;
//    public int _width;
//}