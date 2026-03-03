using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    //This list includes every group of buttons (for each floor) in the minimap
    public static List<GameObject> MinimapPointPositions = new List<GameObject>();

    public Camera MainCamera;
    public GameObject Minimap1F;
    public GameObject Minimap2F;

    //Variables that will copy the transform component of the cameras that will be used to move the minimap camera from one place to another.
    private GameObject MinimapCamera2FInst;
    private GameObject MinimapCamera1FInst;

    public float factorWidth = 0.5f;
    public float factorHeight = 0.4f;
    public float MinimapDepth = 1f;

    public  int CameraFloor = 0;
    private float PositionX;
    private float PositionY;
    private Vector3 WorldPoint;

    private void Awake()
    {
        Object[] MinimapColliders = GameObject.FindGameObjectsWithTag("PointMapCollider");
        foreach (GameObject MinimapCollider in MinimapColliders)
        {
            GameObject Object = (GameObject)MinimapCollider;

            MinimapPointPositions.Add(Object);
        }

        MinimapCamera1FInst = Instantiate(Minimap1F, Minimap1F.transform.position, Minimap1F.transform.rotation);
        MinimapCamera2FInst = Instantiate(Minimap2F, Minimap2F.transform.position, Minimap2F.transform.rotation);
        MinimapCamera1FInst.gameObject.SetActive(false);
        MinimapCamera2FInst.gameObject.SetActive(false);
        Minimap2F.SetActive(false);

        MinimapPointPositions[1].SetActive(false);

    }

    void OnEnable()
    {
        MainCamera = Camera.main;

    }

    private void Start()
    {
        this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        MinimapTransition();
    }

    //Call to change the minimap camera from the actual floor to the other one.
    public void MinimapCameraChange()
    {
        if (CameraFloor == 0)
        {
            Minimap1F.transform.position = MinimapCamera2FInst.transform.position;
            Minimap1F.transform.rotation = MinimapCamera2FInst.transform.rotation;
            MinimapPointPositions[0].SetActive(false);
            MinimapPointPositions[1].SetActive(true);

            CameraFloor = 1;
        }
        else if(CameraFloor == 1)
        {
            Minimap1F.transform.position = MinimapCamera1FInst.transform.position;
            Minimap1F.transform.rotation = MinimapCamera1FInst.transform.rotation;
            MinimapPointPositions[1].SetActive(false);
            MinimapPointPositions[0].SetActive(true);

            CameraFloor = 0;
        }
        
        
    }

    //if actived and enabled, the minimap will follow the center of the main camera.
    void MinimapTransition()
    {
        Event CurrentEvent = Event.current;

        PositionY = MainCamera.scaledPixelWidth * factorWidth;
        PositionX = MainCamera.scaledPixelHeight * factorHeight;

        WorldPoint = new Vector3(PositionX, PositionY, MinimapDepth);

        this.gameObject.transform.position = MainCamera.ScreenToWorldPoint(WorldPoint);

        this.transform.LookAt(MainCamera.transform);
        this.transform.Rotate(0, 180, 0);
    }
}
