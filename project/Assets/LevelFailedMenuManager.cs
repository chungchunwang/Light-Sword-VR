using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelFailedMenuManager : MonoBehaviour
{
    public float spawnAnimationDuration = .5f;
    public float spawnAnimationInitialZOffset = 5f;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }
    public void enableMenu()
    {
        gameObject.SetActive(true);
        StartCoroutine(spawnAnimation());
    }
    IEnumerator spawnAnimation()
    {
        float startTime = Time.time;
        float endTime = startTime + spawnAnimationDuration;
        float initialZPos = transform.position.z;
        while (Time.time <= endTime)
        {
            float t = (Time.time - startTime) / spawnAnimationDuration;
            transform.position = new Vector3(transform.position.x,transform.position.y,Mathf.SmoothStep(initialZPos + spawnAnimationInitialZOffset, initialZPos, t));
            yield return null;
        }
    }
}
