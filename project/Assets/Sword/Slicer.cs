using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.XR.Interaction.Toolkit;

public class Slicer : MonoBehaviour
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
    //Logs the position of the tip of the sword every positionLoggingFrequency seconds.
    IEnumerator positionLogger()
    {
        while (true)
        {
            //We maintain a queue of the latest positions. If the queue is full, remove the oldest position.
            if (pastPositions.Count == positionLoggingSaveCount) pastPositions.Dequeue(); 
            pastPositions.Enqueue(tip.position); //Add the current position to the queue.
            yield return new WaitForSeconds(positionLoggingFrequency); //Wait positionLoggingFrequency for the next position logging event.
        }
    }
    //This method extracts the contact points from a collision and returns them as an array of Vector3s.
    private Vector3[] extractContactPointsAsArray(Collision collision) 
    {
        ContactPoint[] contactPoints = new ContactPoint[collision.contactCount]; //Create an array of ContactPoints to store the contact points.
        Vector3[] array = new Vector3[collision.contactCount]; //Create an array of Vector3s to store the output.
        collision.GetContacts(contactPoints); //Get the contact points from the collision.
        for (int i = 0; i < contactPoints.Length; i++)
        {
            array[i] = contactPoints[i].point; //Extract the Vector3 for each contact point and add it to the output array.
        }
        return array;
    }
    //This method is called when the sword initially collides with something.
    private void OnCollisionEnter(Collision collision) 
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ghost")) return; //We want to completely ignore anything on the Ghost layer, as they should be invisible.
        controller.SendHapticImpulse(1, .1f); //Otherwise, we send haptic feedback to show that the sword is slicing something.
        if (collision.gameObject.layer == LayerMask.NameToLayer("Sword")) //If the sword collides with another sword, we want to show sparks.
        {
            sparksParticleSystem.SetActive(true); //Show the sparks.
            sparksParticleSystem.transform.position = collision.contacts[0].point; //Move the sparks to the point of collision.
        }
        if (collision.gameObject.GetComponent<Sliceable>() == null) return; //If the object we collided with is not sliceable, we don't need to do anything.
        enterTip = tip.position; //Store the position of the tip of the sword as it enters the sliceable object.
        enterHandle = handle.position; //Store the postion of the handle of the sword as it enters the sliceable object.
        collisionEnterPoints = extractContactPointsAsArray(collision); //Store the contact positions as the sword enters the sliceable object.
    }
    //This method is called whenever the sword continues to collide with something.
    private void OnCollisionStay(Collision collision) 
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ghost")) return; //We want to completely ignore anything on the Ghost layer, as they should be invisible.
        controller.SendHapticImpulse(1f, .1f); //Otherwise, we send haptic feedback to show that the sword is slicing something.
        if (collision.gameObject.layer == LayerMask.NameToLayer("Sword")) { //If the swords are intersecting each other, we should update the position of the sparks.
            sparksParticleSystem.transform.position = collision.contacts[0].point; //Move the sparks to the point of collision.
        }
    }
    //This method is called whenever the sword stops colliding with something.
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ghost")) return; //We want to completely ignore anything on the Ghost layer, as they should be invisible.
        controller.SendHapticImpulse(1, .1f);//Otherwise, we send haptic feedback to show that the sword is slicing something.
        if (collision.gameObject.layer == LayerMask.NameToLayer("Sword")) //If the swords are no longer intersecting with one another, then we should no longer have the sparks.
        {
            sparksParticleSystem.SetActive(false); //Hide the sparks.
        }
        if (collision.gameObject.GetComponent<Sliceable>() == null) return; //If the object we collided with is not sliceable, we don't need to do anything.
        exitTip = tip.position; //Store the position of the tip of the sword as it exits the sliceable object.
        collisionExitPoints = extractContactPointsAsArray(collision); //Get the collision points for when the sword exits the sliceable.
        //We send all of the following collision data we have acquired to the sliceable for it to chop up its mesh.
        collision.gameObject.GetComponent<Sliceable>().slice(enterHandle, enterTip, exitTip, pastPositions.ToArray(), getPositionsFromNow, collisionEnterPoints, collisionExitPoints, gameObject.tag); 
        //getPositionsFromNow is an async task that returns future positions.
    }
    void onPauseGame()
    {
        sparksParticleSystem.SetActive(false);
    }

    public async Task<Vector3[]> getPositionsFromNow()
    {
        await Task.Delay(Mathf.RoundToInt(positionLoggingSaveCount * positionLoggingFrequency * 1000));
        return pastPositions.ToArray();
    }

}
