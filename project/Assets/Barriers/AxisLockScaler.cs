using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AxisLockScaler : MonoBehaviour
{
    Vector3 initialParentScale;
    [SerializeField] bool lockX = false;
    [SerializeField] bool lockY = false;
    [SerializeField] bool lockZ = false;
    // Start is called before the first frame update
    void Awake()
    {
        StartCoroutine(scaleLoop());
    }
    IEnumerator scaleLoop()
    {
        initialParentScale = transform.parent.localScale;
        while (true)
        {
            Vector3 currentParentScale = transform.parent.localScale;
            yield return new WaitUntil(() => transform.parent.localScale != currentParentScale);
            float xScale = transform.localScale.x;
            float yScale = transform.localScale.y;
            float zScale = transform.localScale.z;
            if(lockX) xScale *= currentParentScale.x / transform.parent.localScale.x;
            if(lockY) yScale *= currentParentScale.y / transform.parent.localScale.y;
            if(lockZ) zScale *= currentParentScale.z / transform.parent.localScale.z;
            transform.localScale = new Vector3(xScale, yScale, zScale);
        }
    }
}
