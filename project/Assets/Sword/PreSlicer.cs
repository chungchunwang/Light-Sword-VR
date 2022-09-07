using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.XR.Interaction.Toolkit;

public class PreSlicer : MonoBehaviour
{
    [SerializeField] Transform tip;
    [SerializeField] Transform handle;

    [SerializeField] float positionLoggingFrequency = .02f;
    [SerializeField] int positionLoggingSaveCount = 25;

    [SerializeField] XRBaseController controller;

    [SerializeField] GameObject sparksParticleSystem;

    [SerializeField] GameSystem gameSystem;

    
    Queue<Vector3> pastPositions = new Queue<Vector3>();
    Vector3 enterTip, enterHandle, exitTip;
    Vector3[] collisionEnterPoints, collisionExitPoints;

    private void Start()
    {
        StartCoroutine(positionLogger());
        sparksParticleSystem.SetActive(false);
        gameSystem.onPause += onPauseGame;
    }
    private void OnDestroy()
    {
        gameSystem.onPause -= onPauseGame;
    }
    IEnumerator positionLogger()
    {
        while (true)
        {
            if (pastPositions.Count == positionLoggingSaveCount) pastPositions.Dequeue();
            pastPositions.Enqueue(tip.position);
            yield return new WaitForSeconds(positionLoggingFrequency);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Sword"))
        {
            sparksParticleSystem.SetActive(true);
            sparksParticleSystem.transform.position = collision.contacts[0].point;
        }
        if (collision.gameObject.GetComponent<Sliceable>() == null) return;
        exitTip = tip.position;
        enterHandle = handle.position;
        enterTip = pastPositions.Peek();
        collisionEnterPoints = extractContactPointsAsArray(collision);
        controller.SendHapticImpulse(1, .5f);
        collisionExitPoints = extractContactPointsAsArray(collision);

        collision.gameObject.GetComponent<Sliceable>().slice(enterHandle, enterTip, exitTip, pastPositions.ToArray(), getPositionsFromNow, collisionEnterPoints, collisionExitPoints, gameObject.tag);
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ghost")) return;
        controller.SendHapticImpulse(1f, .1f);
        if (collision.gameObject.layer == LayerMask.NameToLayer("Sword")) {
            sparksParticleSystem.transform.position = collision.contacts[0].point;
        }
    }
    void onPauseGame()
    {
        sparksParticleSystem.SetActive(false);
    }
    private Vector3[] extractContactPointsAsArray(Collision collision)
    {
        ContactPoint[] contactPoints = new ContactPoint[collision.contactCount];
        Vector3[] array = new Vector3[collision.contactCount];
        collision.GetContacts(contactPoints);
        for (int i = 0; i < contactPoints.Length; i++)
        {
            array[i] = contactPoints[i].point;
        }
        return array;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Sword"))
        {
            sparksParticleSystem.SetActive(false);
        }
        controller.SendHapticImpulse(1, .1f);
        if (collision.gameObject.GetComponent<Sliceable>() == null) return;
        
    }

    public async Task<Vector3[]> getPositionsFromNow()
    {
        await Task.Delay(Mathf.RoundToInt(positionLoggingSaveCount * positionLoggingFrequency * 1000));
        return pastPositions.ToArray();
    }

}
