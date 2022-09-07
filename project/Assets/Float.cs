using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Float : MonoBehaviour
{
    [SerializeField] public float period;
    [SerializeField] public float displacement;
    [SerializeField, Tooltip("In terms of PI."), Range(0, 2)] public float sineOffset;
    Vector3 position;
    // Start is called before the first frame update
    void Start()
    {
        position = transform.position;
    }
    void Update()
    {
        transform.position = position + displacement * Mathf.Sin(Time.time / period * Mathf.PI * 2 + sineOffset) * Vector3.up;
    }
}
