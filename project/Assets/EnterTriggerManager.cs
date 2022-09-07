using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterTriggerManager : MonoBehaviour
{
    float entered = 100000000000000f;
    [SerializeField] GameObject negative;
    public float getEntered { get => entered;}

    private void OnTriggerEnter(Collider c)
    {
        entered = Time.time;
        negative.SetActive(false);
    }
}
