using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragmentManager : MonoBehaviour
{
    MeshRenderer meshRenderer;
    float fadeSpeed = 0f;
    float currentOpacity = 1;
    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }
    public void startFadeWithLifespanRecursive(float lifespan)
    {
        fadeSpeed = 1/lifespan;
        if(transform.childCount > 0)
        {
            for(int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.AddComponent<FragmentManager>().startFadeWithLifespanRecursive(lifespan);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (fadeSpeed <= Mathf.Epsilon) return;
        currentOpacity = currentOpacity - fadeSpeed * Time.deltaTime;
        if(currentOpacity <= 0)
        {
            Destroy(gameObject);
        }
        meshRenderer.material.SetFloat("_Opacity", currentOpacity);
    }
}
