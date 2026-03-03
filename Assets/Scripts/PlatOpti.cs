using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlatOpti : MonoBehaviour
{
    public void OnMouseDown()
    {
        SceneManager.LoadSceneAsync("PlatOpti");
    }
}
