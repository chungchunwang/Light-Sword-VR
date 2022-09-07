using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScrollViewMapItemManager : MonoBehaviour
{
    [SerializeField] Image songArtImage;
    [SerializeField] TMP_Text titleLabel;
    [SerializeField] TMP_Text authorLabel;
    MapSystem mapSystem;
    int index;
    private void Start()
    {
        mapSystem = GameObject.FindGameObjectWithTag("Map System").GetComponent<MapSystem>();
    }
    public void populateFields(int index, Sprite art, string title, string author)
    {
        this.index = index;
        songArtImage.sprite = art;
        titleLabel.text = title;
        authorLabel.text = author;
    }
    public void onPress()
    {
        mapSystem.setCurrentMap(index);
    }
}
