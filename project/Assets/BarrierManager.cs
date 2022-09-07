using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using static UnityEngine.Rendering.DebugUI;

public class BarrierManager : MonoBehaviour
{
    PointSystem pointSystem;
    [SerializeField] AudioMixer audioMixer;
    private void Start()
    {
        pointSystem = GameObject.FindGameObjectWithTag("Point System").GetComponent<PointSystem>();
    }
    private void OnTriggerEnter(Collider other)
    {
        audioMixer.SetFloat("In Game Music Lowpass", 200);
    }
    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "MainCamera")
        {
            pointSystem.logBarrier(Time.fixedDeltaTime);
            //audioMixer.GetFloat("In Game Music Lowpass", out float value);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        audioMixer.SetFloat("In Game Music Lowpass", 22000);
    }

}
