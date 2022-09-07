using System.Collections;
using Lean.Pool;
using UnityEngine;

public class TimedDespawner : MonoBehaviour
{
    float lifespan;
    public void startTimer(float lifespan)
    {
        this.lifespan = lifespan;
        StartCoroutine(despawnTimer());
    }
    IEnumerator despawnTimer()
    {
        yield return new WaitForSeconds(lifespan);
        LeanPool.Despawn(gameObject);
    }
}
