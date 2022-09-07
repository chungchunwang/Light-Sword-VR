using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXSystem : MonoBehaviour
{
    [SerializeField] AudioClip sliceAudio;
    [SerializeField] AudioClip missAudio;
    [SerializeField] AudioClip badCutAudio;
    AudioSource audioSource;
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void playSliceAudio()
    {
        if (sliceAudio == null) return;
        audioSource.PlayOneShot(sliceAudio);
    }
    public void playMissAudio()
    {
        if(missAudio == null) return;
        audioSource.PlayOneShot(missAudio);
    }
    public void playBadCutAudio()
    {
        if (badCutAudio == null) return;
        audioSource.PlayOneShot(badCutAudio);
    }
}
