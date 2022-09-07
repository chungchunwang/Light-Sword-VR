using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MapFolderDisplayManager : MonoBehaviour
{
    TMP_Text display;
    void Start()
    {
        display = GetComponent<TMP_Text>();
        display.text = ProjectSettings.savePath + ProjectSettings.mapsDirPath;
    }
}
