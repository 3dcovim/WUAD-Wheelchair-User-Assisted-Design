using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManagerPlayerTranslation : MonoBehaviour
{
    //The following lists include every room in each floor, every button in the minimap, and every point to move to in the scene.

    public static List<GameObject> Recintos1FList = new List<GameObject>();
    public static List<GameObject> Recintos2FList = new List<GameObject>();
    public static List<Button> CanvasInter1FList = new List<Button>();
    public static List<Button> CanvasInter2FList = new List<Button>();
    public static List<TransPointClass> TransPoint1FList = new List<TransPointClass>();
    public static List<TransPointClass> TransPoint2FList = new List<TransPointClass>(); 

    public GameObject Player;

    private int WhileCount = 0;                             //This variable will be increased until it reaches the selected button in the minimap                                   

    private void Awake()
    {
        TransPoint1FList.Clear();
        TransPoint2FList.Clear();
        Recintos1FList.Clear();
        Recintos2FList.Clear();

        //Each list will get all available element added

        Object[] Recintos1F = GameObject.FindGameObjectsWithTag("Recinto1F");
        Object[] Recintos2F = GameObject.FindGameObjectsWithTag("Recinto2F");
        Object[] MinimapInters1F = GameObject.FindGameObjectsWithTag("MinimapInter1F");
        Object[] MinimapInters2F = GameObject.FindGameObjectsWithTag("MinimapInter2F");

        foreach (GameObject Recinto1F in Recintos1F)
        {
            GameObject Object = (GameObject)Recinto1F;

            Recintos1FList.Add(Object);
        }

        foreach (GameObject Recinto2F in Recintos2F)
        {
            GameObject Object = (GameObject)Recinto2F;

            Recintos2FList.Add(Object);
        }

        foreach (GameObject MinimapInter1F in MinimapInters1F)
        {
            GameObject Object = (GameObject)MinimapInter1F;

            CanvasInter1FList.Add(Object.GetComponent<Button>());
        }

        foreach (GameObject MinimapInter2F in MinimapInters2F)
        {
            GameObject Object = (GameObject)MinimapInter2F;

            CanvasInter2FList.Add(Object.GetComponent<Button>());
        }


        for (int i = 0; i < Recintos1FList.Count; ++i)
        {
            TransPoint1FList.Add(Recintos1FList[i].GetComponentInChildren<TransPointClass>());
            TransPoint1FList[i].Button = CanvasInter1FList[i];
        }

        for (int i = 0; i < Recintos2FList.Count; ++i)
        {
            TransPoint2FList.Add(Recintos2FList[i].GetComponentInChildren<TransPointClass>());
            TransPoint2FList[i].Button = CanvasInter2FList[i];
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
    }

    //Once the user selects a place to move to , this function will check which button has been selected and it will transport the user to the asociated point
    public void TranslatePlayer1F()
    {

        while (WhileCount < TransPoint1FList.Count && !TransPoint1FList[WhileCount].isClicked)
        {
            WhileCount++;
        }

        Player.transform.position = TransPoint1FList[WhileCount].transform.position;

        TransPoint1FList[WhileCount].OnClickChange();

        WhileCount = 0;

    }

    public void TranslatePlayer2F()
    {

        while (WhileCount < TransPoint2FList.Count && !TransPoint2FList[WhileCount].isClicked)
        {
            WhileCount++;
        }

        Player.transform.position = TransPoint2FList[WhileCount].transform.position;

        TransPoint2FList[WhileCount].OnClickChange();

        WhileCount = 0;

    }
}
