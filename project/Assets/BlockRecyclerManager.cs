using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockRecyclerManager : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Sliceable>() == null) return;
        Destroy(collision.gameObject);
    }
}
