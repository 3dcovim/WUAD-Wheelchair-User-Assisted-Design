using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KeyboardMain : MonoBehaviour
{
    public ManagerScene ManagerScene;
    public TextMeshPro ActualTextOutput;
    public int PreviousCount;

    public void UpdateTMPTarget()
    {
        //ActualTextOutput = ManagerScene.MarkersSceneList[ManagerScene.MarkersSceneList.Count - 1].MarkerTextOutput;
    }

}
