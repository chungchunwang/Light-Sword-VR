using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoManager : MonoBehaviour
{
    [SerializeField] GameObject EmptyInfo;
    [SerializeField] GameObject ActiveInfo;
    MapSystem mapSystem;
    private void OnEnable()
    {
        mapSystem = GameObject.FindGameObjectWithTag("Map System").GetComponent<MapSystem>();
        toggleInfoActive();
        mapSystem.mapChanged += toggleInfoActive;
    }
    private void OnDestroy()
    {
        mapSystem.mapChanged -= toggleInfoActive;
    }
    void toggleInfoActive()
    {
        if (mapSystem.currentMap == -1)
        {
            ActiveInfo.SetActive(false);
            EmptyInfo.SetActive(true);
        }
        else
        {
            ActiveInfo.SetActive(true);
            EmptyInfo.SetActive(false);
        }
    }
}
