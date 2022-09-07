using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ComboAndPointsMenuManager : MonoBehaviour
{
    PointSystem pointSystem;
    [SerializeField] TMP_Text pointsLabel;
    [SerializeField] TMP_Text comboLabel;

    void OnEnable()
    {
        pointSystem = GameObject.FindGameObjectWithTag("Point System").GetComponent<PointSystem>();
        //StartCoroutine(updateLoop());
    }
    private void Update()
    {
        if (pointSystem.getCurrentPoints.ToString().Equals(pointsLabel.text) && pointSystem.getCurrentCombo.ToString().Equals(comboLabel.text)) return;
        pointsLabel.text = pointSystem.getCurrentPoints.ToString();
        comboLabel.text = pointSystem.getCurrentCombo.ToString();
    }
    void OnDisable()
    {
    }
}
