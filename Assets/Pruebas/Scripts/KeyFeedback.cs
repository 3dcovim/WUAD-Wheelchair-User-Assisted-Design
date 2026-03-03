using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KeyFeedback : MonoBehaviour
{
    public bool keyHit = false;
    public bool keyCanBeHitAgain = false;

    private KeyboardMain KeyboardMain;
    private float originalYPosition;

    // Start is called before the first frame update
    void Start()
    {
        KeyboardMain = GetComponentInParent<KeyboardMain>();

        originalYPosition = transform.position.y;
        
    }


    // Update is called once per frame
    void Update()
    {
        if(keyHit)
        {
            keyHit = false;
            keyCanBeHitAgain = false;
            transform.position += new Vector3(0, -0.03f, 0);
        }
        if (transform.position.y < originalYPosition) transform.position += new Vector3(0, 0.005f, 0);
        else keyCanBeHitAgain = true;

    }

    

    public void UpdateTextBox()
    {

        if (this.name == "Space")
            KeyboardMain.ActualTextOutput.text += " ";
        else if (this.name == "Punto")
            KeyboardMain.ActualTextOutput.text += ".";
        else if (this.name == "Coma")
            KeyboardMain.ActualTextOutput.text += ",";
        else if (this.name == "Borrar")
            KeyboardMain.ActualTextOutput.text = KeyboardMain.ActualTextOutput.text.Substring(0, KeyboardMain.ActualTextOutput.text.Length - 1);
        else
            KeyboardMain.ActualTextOutput.text += this.name;
    }
}
