using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProcessAwaitHandler : MonoBehaviour
{
    List<awaitableProcess> processes;
    private void Start()
    {
        processes = new List<awaitableProcess>();
        processes.Add(GameObject.FindGameObjectWithTag("Map System").GetComponent<MapSystem>());
        processes.Add(GameObject.FindGameObjectWithTag("Stats System").GetComponent<StatsSystem>());
    }
    void Update()
    {
        bool allProcessed = true;
        foreach (awaitableProcess process in processes)
        {
            if (!process.getIsProcessed)
            {
                allProcessed = false;
                break;
            }
        }
        if (allProcessed)
        {
            SceneManager.LoadScene("MenuScene");
        }
    }
}
public interface awaitableProcess
{
    public bool getIsProcessed { get;}
}