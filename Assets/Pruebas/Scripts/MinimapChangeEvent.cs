using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class MinimapChangeEvent : MonoBehaviour
{
    public UnityEvent onCollision;

    public void OnTriggerEnter(Collider other)
    {
        XRRig userController = other.GetComponentInParent<XRRig>();

        if (userController != null)
        {
            onCollision.Invoke();
        }

    }

}

