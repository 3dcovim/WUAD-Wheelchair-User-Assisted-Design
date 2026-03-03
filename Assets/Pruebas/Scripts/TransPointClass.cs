using UnityEngine;
using UnityEngine.UI;

public class TransPointClass : MonoBehaviour
{
    public bool isClicked = false;

    public Button Button;

    //Has this button been clicked?
    public void OnClickChange()
    {
        isClicked = isClicked ? false : true;
    }

}
