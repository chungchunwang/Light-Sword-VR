using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapScrollSelectionManager : MonoBehaviour
{
    MapSystem mapSystem;
    [SerializeField] GameObject scrollViewMapItemPrefab;
    [SerializeField] GameObject content;
    // Start is called before the first frame update
    void Start()
    {
        mapSystem = GameObject.FindGameObjectWithTag("Map System").GetComponent<MapSystem>();
        if (mapSystem.mapsInfo.Count == 0) return;
        for(int i = 0; i< content.transform.childCount; i++){
            Destroy(content.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < mapSystem.mapsInfo.Count; i++)
        {
            MapInfo mapInfo = mapSystem.mapsInfo[i];
            GameObject newMapItem = Instantiate(scrollViewMapItemPrefab);
            newMapItem.GetComponent<ScrollViewMapItemManager>().populateFields(i,mapInfo.coverImage,mapInfo.fullSongName,mapInfo._songAuthorName);
            newMapItem.transform.SetParent(content.transform, false);
        }
    }
}
