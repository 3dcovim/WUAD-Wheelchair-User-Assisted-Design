using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class inicio3 : MonoBehaviour
{
    public void OnMouseDown()
    {
        SceneManager.LoadSceneAsync("ThirdOption");
    }
}
