using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class Mover : MonoBehaviour
{
    public Vector3 currentVelocity = Vector3.zero;
    Rigidbody rb;
    public void moveWithVelocity(Vector3 velocity)
    {
        rb = GetComponent<Rigidbody>();
        currentVelocity = velocity;
    }
    private void FixedUpdate()
    {
        if(currentVelocity == Vector3.zero) return;
        
        rb.MovePosition(rb.position + currentVelocity * Time.deltaTime);
    }
}
