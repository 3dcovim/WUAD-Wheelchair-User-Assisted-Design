using UnityEngine;

public class CameraElevMove : MonoBehaviour
{
    private Rigidbody Rigidbody;
    private bool inElevator = false;

    private void Start()
    {
        Rigidbody = this.GetComponent<Rigidbody>();
        Rigidbody.isKinematic = true;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (inElevator)
        {
            Rigidbody.isKinematic = false;
            Rigidbody.useGravity = true;
        }
        else
        {
            Rigidbody.isKinematic = true;
            Rigidbody.useGravity = false;
        }
                  
    }

    public void CheckState()
    {
        inElevator = inElevator ? false: true;
    }

}

