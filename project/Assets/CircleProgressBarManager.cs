using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircleProgressBarManager : MonoBehaviour
{
    Image progressBarImage;
    [SerializeField] float animateDuration = .5f;
    private void OnEnable()
    {
        progressBarImage = GetComponent<Image>();
        progressBarImage.fillAmount = 0;
    }
    public void setValue(float newValue)
    {
        float oldValue = progressBarImage.fillAmount;
        StartCoroutine(animate(oldValue, newValue));
    }
    public float getValue()
    {
        return progressBarImage.fillAmount;
    }
    IEnumerator animate(float oldValue, float newValue)
    {
        float startTime = Time.time;
        float endTime = startTime + animateDuration;
        while (Time.time <= endTime)
        {
            float t = (Time.time - startTime) / animateDuration;
            progressBarImage.fillAmount = Mathf.Lerp(oldValue, newValue, t);
            yield return null;
        }
    }
}
