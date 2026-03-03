using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class OutInChangePoint : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        XRRig userController = other.gameObject.GetComponentInParent<XRRig>();
        if (userController != null)
        {
            SceneManager.LoadSceneAsync("Interior Scene");
        }

    }
}
