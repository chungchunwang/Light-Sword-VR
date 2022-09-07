using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergySliderManager : MonoBehaviour
{
    PointSystem pointSystem;
    Slider slider;
    [SerializeField] float animationDuration = .2f;
    float value = 0;
    void Start()
    {
        slider = GetComponent<Slider>();
        pointSystem = GameObject.FindGameObjectWithTag("Point System").GetComponent<PointSystem>();
        slider.value = value;
    }
    void Update()
    {
        if (pointSystem.getCurrentEnergy == value) return;
        value = slider.value;
        //animate to position
        float currentSliderPos = slider.value;
        float actualSliderValue = pointSystem.getCurrentEnergy;
        StartCoroutine(animate(currentSliderPos, actualSliderValue));
    }
    IEnumerator animate(float oldValue, float newValue)
    {
        float startTime = Time.time;
        float endTime = startTime + animationDuration;
        while (Time.time <= endTime)
        {
            float t = (Time.time - startTime) / animationDuration;
            slider.value = Mathf.Lerp(oldValue, newValue, t);
            yield return null;
        }
    }
}
