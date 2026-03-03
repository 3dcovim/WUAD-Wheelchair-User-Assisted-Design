using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class TranslatePlayerEvent : MonoBehaviour
{
    public UnityEvent onPointerCollision;

    public void OnTriggerEnter(Collider other)
    {
        XRRig userController = other.GetComponentInParent<XRRig>();

        if (userController != null)
        {
            onPointerCollision.Invoke();
        }

    }
}
