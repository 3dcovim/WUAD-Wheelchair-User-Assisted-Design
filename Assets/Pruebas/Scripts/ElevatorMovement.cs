using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;

public class ElevatorMovement : MonoBehaviour
{
    public UnityEvent inElevator;
    public InputDevice Device;
    public float factorVel = 0.01f;                  //Velocity
    private float factorDist = 0.004f;              //Closing Distance
    private GameObject RightDoor;
    private GameObject LeftDoor;
    private bool triggerValue;
    private bool userIsBot = true;                  //User is in bot side and has reached the bot side event
    private bool userIsTop = false;                 //User is in top side and has reached the top side event
    private bool userHasEntered = false;            //User has entered the elevator
    private bool hasReachedTop = false;             //Elevator has reached top side
    private bool hasReachedBot = true;              //Elevator has reached bot side
    private bool isWaiting = true;                  //Elevator is waiting for the user to reach an event
    private bool elevatorActionTriggered = false;   //User has triggered the moving elevator action event;
    private bool isClosed = true;                   //Elevator's doors are closed
    private bool isOpen = false;                    //Elevator's doors are open
    private bool isOpening = false;                 //Elevator's doors are being opened
    private bool isMoving = false;                  //Elevator's doors are being closed
    private Vector3 elevatorIniPos;
    private Vector3 elevatorFinPos;
    private Vector3 deltaPosElevator;
    private Vector3 RightDoorFinPos = new Vector3(-1.7f, 0, 0);
    private Vector3 LeftDoorFinPos = new Vector3(1.7f, 0, 0);
    private Vector3 RightDoorIniPos;
    private Vector3 LeftDoorIniPos;
    private Vector3 deltaPosRightDoor;
    private Vector3 deltaPosLeftDoor;

    private void Start()
    {
        var RightHandDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, RightHandDevices);
        Device = RightHandDevices[0];


        RightDoor = this.gameObject.transform.GetChild(0).gameObject;
        LeftDoor = this.gameObject.transform.GetChild(1).gameObject;
        RightDoorIniPos = RightDoor.transform.localPosition;
        LeftDoorIniPos = LeftDoor.transform.localPosition;
        elevatorIniPos = this.transform.position;
        elevatorFinPos = elevatorIniPos + new Vector3(0, 3.35f, 0);
    }

    private void FixedUpdate()
    {

        CheckActionTrigger();

        //Elevator heads to top
        if (userIsBot && !isWaiting)
        {
            if ((isClosed || isOpening) && !isMoving) OpenDoors();
            else if (isOpen && elevatorActionTriggered)
            {
                CloseDoors();
                isMoving = true;
            }
            else if (isClosed && !hasReachedTop) ElevGoesUp();
            else if (hasReachedTop)
            {
                OpenDoors();
                isMoving = false;
            }
                

        }

        //Elevator heads to bot
        else if (userIsTop && !isWaiting)
        {
            if ((isClosed || isOpening) && !isMoving) OpenDoors();
            else if (isOpen && elevatorActionTriggered)
            {
                CloseDoors();
                isMoving = true;
            }
            else if (isClosed && !hasReachedBot) ElevGoesDown();
            else if (hasReachedBot)
            {
                OpenDoors();
                isMoving = false;
            }
        }

        //Elevator is waiting
        else
        {
            CloseDoors();
        }
        
    }

    public void OpenDoors()
    {
        isOpening = true;

        deltaPosRightDoor = RightDoorFinPos - RightDoor.transform.localPosition;
        if (Mathf.Abs(deltaPosRightDoor.x) > factorDist)
            RightDoor.transform.Translate(-Vector3.right * Time.deltaTime * 0.5f);

        deltaPosLeftDoor = LeftDoorFinPos - LeftDoor.transform.localPosition;
        if (Mathf.Abs(deltaPosLeftDoor.x) > factorDist)
            LeftDoor.transform.Translate(Vector3.right * Time.deltaTime * 0.5f);

        deltaPosRightDoor = RightDoorFinPos - RightDoor.transform.localPosition;
        deltaPosLeftDoor = LeftDoorFinPos - LeftDoor.transform.localPosition;

        if (Mathf.Abs(deltaPosRightDoor.x) < factorDist && Mathf.Abs(deltaPosLeftDoor.x) < factorDist)
        {
            isClosed = false;
            isOpen = true;
            isOpening = false;
        }
    }

    public void CloseDoors()
    {

        deltaPosRightDoor = -RightDoor.transform.localPosition + RightDoorIniPos;
        if (Mathf.Abs(deltaPosRightDoor.x) > factorDist)
            RightDoor.transform.Translate(Vector3.right * Time.deltaTime * 0.5f);

        deltaPosLeftDoor = -LeftDoor.transform.localPosition + LeftDoorIniPos;
        if (Mathf.Abs(deltaPosLeftDoor.x) > factorDist)
            LeftDoor.transform.Translate(-Vector3.right * Time.deltaTime * 0.5f);

        deltaPosRightDoor = -RightDoor.transform.localPosition + RightDoorIniPos;
        deltaPosLeftDoor = -LeftDoor.transform.localPosition + LeftDoorIniPos;

        if (Mathf.Abs(deltaPosRightDoor.x) < factorDist && Mathf.Abs(deltaPosLeftDoor.x) < factorDist)
        {
            isClosed = true;
            isOpen = false;
        }

    }

    public void ElevGoesUp()
    {
            deltaPosElevator = elevatorFinPos - this.transform.position;
            if (Mathf.Abs(deltaPosElevator.y) > factorDist)
                this.transform.Translate(Vector3.up * Time.deltaTime * factorVel);
            else
            {
                hasReachedTop = true;
                hasReachedBot = false;
                elevatorActionTriggered = false;
                isMoving = false;
            }
    }
    
    public void ElevGoesDown()
    {
            deltaPosElevator = elevatorIniPos - this.transform.position;
            if (Mathf.Abs(deltaPosElevator.y) > factorDist)
                this.transform.Translate(-Vector3.up * Time.deltaTime * factorVel);
            else
            {
                hasReachedTop = false;
                hasReachedBot = true;
                elevatorActionTriggered = false;
                isMoving = false;
            }
    }

    public void IsTriggered()
    {
        if (hasReachedBot)
        {
            if (!userIsBot)
            {
                userIsBot = true;
                userIsTop = false;
                isWaiting = true;
            }
            else isWaiting = false;
        }
        if (hasReachedTop)
        {
            if (!userIsTop)
            {
                userIsBot = false;
                userIsTop = true;
                isWaiting = true;
            }
            else isWaiting = false;
        }

        userHasEntered = false;
    }

    public void OnTriggerStay(Collider other)
    {
        XRRig userController = other.GetComponentInParent<XRRig>();
        if (userController != null)
        {
            userHasEntered = true;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        XRRig userController = other.GetComponentInParent<XRRig>();
        if (userController != null)
        {
            inElevator.Invoke();
        }

    }

    public void OnTriggerExit(Collider other)
    {
        XRRig userController = other.GetComponentInParent<XRRig>();
        if (userController != null)
        {
            inElevator.Invoke();
        }
    }
    public void CheckActionTrigger()
    {
        if (Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerValue) && triggerValue && userHasEntered)
        {
            elevatorActionTriggered = true;
        }
  
    }

}

