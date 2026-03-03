using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIChecking : MonoBehaviour
{
    public void UIActivate()
    {
        if (this.gameObject.activeSelf) this.gameObject.SetActive(false);
        else this.gameObject.SetActive(true);

    }
}