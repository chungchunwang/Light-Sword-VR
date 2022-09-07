using System.Collections;
using TMPro;
using UnityEngine;

public class PointIndicatorManager : MonoBehaviour
{
    public bool active = false;
    TMP_Text pointLabel;
    public float spawnAnimationDuration = .5f;
    public float spawnAnimationInitialYOffset = -.2f;
    public float spawnAnimationInitialOpacity = 0;

    // Start is called before the first frame update
    void Start()
    {
        pointLabel = GetComponent<TMP_Text>();
        gameObject.SetActive(false);
    }

    public void display(int point)
    {   if (active) return;
        active = true;
        gameObject.SetActive(true);
        StartCoroutine(spawnAnimation(point));
    }
    IEnumerator spawnAnimation(int point)
    {
        pointLabel.text = point.ToString();
        float startTime = Time.time;
        float endTime = startTime + spawnAnimationDuration;
        float finalYPos = transform.position.y;
        float finalOpacity = pointLabel.faceColor.a;
        while (Time.time <= endTime)
        {
            float t = (Time.time - startTime) / spawnAnimationDuration;
            transform.position = new Vector3(transform.position.x, Mathf.SmoothStep(finalYPos + spawnAnimationInitialYOffset, finalYPos, t), transform.position.z);
            pointLabel.alpha = Mathf.SmoothStep(spawnAnimationInitialOpacity, finalOpacity, t);
            yield return null;
        }
        active = false;
        gameObject.SetActive(false);
    }
}
