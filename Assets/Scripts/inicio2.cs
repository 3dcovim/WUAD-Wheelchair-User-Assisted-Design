using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class inicio2 : MonoBehaviour
{
    // Start is called before the first frame update
    public void OnMouseDown()
    {
        SceneManager.LoadSceneAsync("SecondOption");
    }
}
