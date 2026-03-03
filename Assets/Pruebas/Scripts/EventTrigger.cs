using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class EventTrigger : MonoBehaviour
{
    public UnityEvent onCollision;

    //Used to check if the user has entered the trigger collider.
    public void OnTriggerEnter(Collider other)
    {
        XRRig userController = other.GetComponentInParent<XRRig>();

        if (userController != null)
        {
            onCollision.Invoke();        
        }

    }

}
