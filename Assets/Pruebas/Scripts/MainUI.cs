using UnityEngine;


public class MainUI : MonoBehaviour
{
    UIChecking[] UIButtonslist;

    public GameObject InstPlace;
    public Camera MainCamera;

    public float factorWidth = 0.5f;
    public float factorHeight = 0.5f;
    public float MainUIDepth = 1.6f;
    private float PositionX;
    private float PositionY;
    private Vector3 WorldPoint;


    private void OnEnable()
    {
        MainCamera = Camera.main;

        UIButtonslist = FindObjectsOfType<UIChecking>();

        AllUIDeactivate();

        InstantiateMainUI();

    }

    private void Start()
    {
        this.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        this.transform.LookAt(MainCamera.transform);            //Main UI will always look at user.
        this.transform.Rotate(0, 180, 0);

    }

    //When enabled, all UI elements related to this GO will be deactivated.
    public void AllUIDeactivate()
    {

        for (var i = 0; i < UIButtonslist.Length; ++i)
        {
            if (UIButtonslist[i].gameObject.activeSelf)
                UIButtonslist[i].gameObject.SetActive(false);
        }
    }


    //When enabled, the Main UI will be instantiated in the center of the screen.
    public void InstantiateMainUI()
    {
        Event CurrentEvent = Event.current;

        PositionX = MainCamera.pixelWidth * factorWidth;
        PositionY = MainCamera.pixelHeight * factorHeight;

        WorldPoint = new Vector3(PositionX, PositionY, MainUIDepth);

        this.gameObject.transform.position = MainCamera.ScreenToWorldPoint(WorldPoint);
    }

}
