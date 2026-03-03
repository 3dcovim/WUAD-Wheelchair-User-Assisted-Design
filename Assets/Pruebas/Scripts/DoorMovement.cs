using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorMovement : MonoBehaviour
{
    // the door from this object
    public GameObject Door;
    // the door copied and rotated/moved to be the opened door
    public GameObject DoorOpen;
    // this will be a copy of the original door so that we have some numbers to work with.
    private GameObject DoorClosed;
    // this controls if the door is opened or closed.
    public bool isOpened = false;

    // this is the movement rate (if movemnt is applied to the door)
    public float moveSpeed = 3;
    // this is the rotation rate (if rotation is applied to the door)
    public float rotationSpeed = 90;

    void Start()
    {
        // copy the door to keep its position
        DoorClosed = Instantiate(Door, Door.transform.position, Door.transform.rotation);
        // hide both the open and closed door
        DoorClosed.SetActive(false);
        DoorOpen.SetActive(false);
    }

    void Update()
    {
        // every frame, move the door towards the Open/Closed door
        var target = isOpened ? DoorOpen : DoorClosed;
        // these actually do the moving/rotating
        //Door.transform.position = Vector3.MoveTowards(Door.transform.position, target.transform.position, moveSpeed * Time.deltaTime);
        if(Mathf.Abs(target.transform.localRotation.y) - Mathf.Abs(Door.transform.localRotation.y) > 0.01f || Mathf.Abs(Door.transform.localRotation.y)- Mathf.Abs(target.transform.localRotation.y) > 0.01f)
        Door.transform.rotation = Quaternion.RotateTowards(Door.transform.rotation, target.transform.rotation, rotationSpeed * Time.deltaTime);
    }

    public void CheckDoor()
    {
        //Checking isOpened var
        if (isOpened) isOpened = false;
        else isOpened = true;
    }

}