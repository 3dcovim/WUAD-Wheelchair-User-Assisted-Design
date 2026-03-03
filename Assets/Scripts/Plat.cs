using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Plat : MonoBehaviour
{
    public void OnMouseDown()
    {
        SceneManager.LoadSceneAsync("Plat");
    }
}
