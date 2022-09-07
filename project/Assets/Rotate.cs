using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] public float period;
    [SerializeField] public float displacement;
    [SerializeField, Tooltip("In terms of PI."), Range(0, 2)] public float sineOffset;
    enum Axis {X,Y,Z}
    [SerializeField] Axis axis;
    Quaternion rotation;
    // Start is called before the first frame update
    void Start()
    {
        rotation = transform.rotation;
    }
    void Update()
    {
        Vector3 rotDisplacement = new Vector3(0, 0, 0);
        if (axis == Axis.X) rotDisplacement = Vector3.right;
        if (axis == Axis.Y) rotDisplacement = Vector3.up;
        if (axis == Axis.Z) rotDisplacement = Vector3.forward;
        transform.rotation = Quaternion.Euler(rotation.eulerAngles + displacement * Mathf.Sin(Time.time / period * Mathf.PI * 2 + sineOffset) * rotDisplacement);
    }
}
