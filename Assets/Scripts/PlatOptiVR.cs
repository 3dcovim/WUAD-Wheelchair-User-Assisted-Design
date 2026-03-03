using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlatOptiVR : MonoBehaviour
{
    public void OnMouseDown()
    {
        SceneManager.LoadSceneAsync("PlatOptiVR");
    }
}
