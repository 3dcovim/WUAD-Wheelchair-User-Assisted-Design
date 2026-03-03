using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class InOutChangePoint : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        XRRig userController = other.gameObject.GetComponentInParent<XRRig>();
        if (userController != null)
        {
            SceneManager.LoadSceneAsync("Demo Scene");

        }

    }
}
