using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowController : MonoBehaviour
{
    [SerializeField] private GameObject followObject;
    private Transform followTarget;
    private Rigidbody rigidBody;
    [SerializeField] Quaternion rotationOffset;


    // Start is called before the first frame update
    void Start()
    {
        followTarget = followObject.transform;
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.position = followTarget.position;
        rigidBody.rotation = followTarget.rotation;
    }

    void FixedUpdate()
    {
        rigidBody.MovePosition(followTarget.position);
        rigidBody.MoveRotation(Quaternion.Euler(followTarget.rotation.eulerAngles + rotationOffset.eulerAngles));
    }
}
