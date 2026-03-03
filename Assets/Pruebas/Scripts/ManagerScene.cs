using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class ManagerScene : MonoBehaviour
{
    public static List<XRGrabInteractable> MarkersList = new List<XRGrabInteractable>();
    public List<MarkerMonoBehaviour> MarkersSceneList = new List<MarkerMonoBehaviour>();

    [SerializeField, Tooltip("Main UI Canvas")]
    public GameObject UIToolPrincipal;
    [SerializeField, Tooltip("Minimap Canvas")]
    public GameObject MinimapCanvas;
    [SerializeField, Tooltip("Marker's instantation offset")]
    public Vector3 MarkerOffset = new Vector3(0, 0.5f, 0);

    public InputDevice RightDevice;
    public InputDevice LeftDevice;
    public GameObject Keyboard;
    private KeyboardMain KeyboardComp;
    public GameObject InstPlace;
    public int MarkerCount = 0;
    private bool primaryButtonRightDeviceValue;                   //Has the primaryButton been triggered?
    private bool secondaryButtonRightDeviceValue;                 //Has the secondaryButton been triggered?
    private bool primaryButtonLeftDeviceValue;                    //Has the primaryButton been triggered?
    private bool secondaryButtonLeftDeviceValue;                  //Has the secondaryButton been triggered?
    private bool ReboundsOK = true;                               //Rebounds are happening no longer
    private Text MinimapMarkerText;
    private Text MinimapFloorText;
    private Minimap Minimap;
    private Text[] MinimapTexts;
    public MarkerMonoBehaviour[] MarkersScenesubList;



    private void OnEnable()
    {
        MinimapCanvas = GameObject.FindGameObjectWithTag("MinimapCanvas");
    }

    void Start()
    {
        var RightHandDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, RightHandDevices);
        RightDevice = RightHandDevices[0];

        var LeftHandDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, LeftHandDevices);
        LeftDevice = LeftHandDevices[0];

        Object[] subListObjects = Resources.LoadAll("Prefabs", typeof(XRGrabInteractable));

        foreach(XRGrabInteractable subListObject in subListObjects)
        {
            XRGrabInteractable Object = (XRGrabInteractable)subListObject;

            MarkersList.Add(Object);
        }

        if(MinimapCanvas != null)
        {
            Minimap = MinimapCanvas.GetComponent<Minimap>();
            MinimapTexts = MinimapCanvas.GetComponentsInChildren<Text>();
            MinimapMarkerText = MinimapTexts[0];
            MinimapFloorText = MinimapTexts[1];

            MinimapMarkerText.text = ("Nº de indicadores = " + MarkerCount);
            MinimapFloorText.text = ("Planta " + Minimap.CameraFloor);
        }

        KeyboardComp = Keyboard.GetComponent<KeyboardMain>();
    }

    private void Update()
    {
        OpenMainUI();

        if(MinimapCanvas != null)
        {
            OpenMinimap();

            ChangeMinimapLevel();
        }


    }

    //If the correct button has been selected, then the MainUi will be opened
    public void OpenMainUI()
    {
        if (RightDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out primaryButtonRightDeviceValue) && primaryButtonRightDeviceValue && ReboundsOK)
        {
            ReboundsOK = false;

            if (UIToolPrincipal.gameObject.activeSelf) UIToolPrincipal.gameObject.SetActive(false);
            else UIToolPrincipal.gameObject.SetActive(true);

            StartCoroutine("Rebound");
        }
    }

    //If the correct button has been selected, then the Minimap will be opened
    public void OpenMinimap()
    {
        if (RightDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out secondaryButtonRightDeviceValue) && secondaryButtonRightDeviceValue && ReboundsOK)
        {
            ReboundsOK = false;

            if (MinimapCanvas.gameObject.activeSelf) MinimapCanvas.gameObject.SetActive(false);
            else 
            {
                MinimapCanvas.gameObject.SetActive(true);
                UpdateUI();
            } 

            StartCoroutine("Rebound");
        }
    }

    //If the correct button has been selected, then the Minimap will change from one floor view to another
    public void ChangeMinimapLevel()
    {
        if (LeftDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out secondaryButtonLeftDeviceValue) && secondaryButtonLeftDeviceValue && ReboundsOK)
        {

            if (MinimapCanvas.activeSelf)
            {
                ReboundsOK = false;
                Minimap.MinimapCameraChange();
                MinimapFloorText.text = ("Planta " + Minimap.CameraFloor);
                StartCoroutine("Rebound");
            }

        }

    }

    //The marker count will be updated as well as the text itself
    public void UpdateMarkerCount(bool OpSignal)
    {
        if (OpSignal) MarkerCount++;
        else MarkerCount--;

        if(Minimap != null && Minimap.gameObject.activeSelf)
        UpdateUI();
    }

    public void UpdateUI()
    {
        MinimapMarkerText.text = ("Nº de indicadores = " + MarkerCount);
    }

    //Call of instantiating a marker has been received, along with the requiered marker index
    public void InstantiateMarker(int index)
    {
        Instantiate(MarkersList[index].gameObject, InstPlace.transform.position + MarkerOffset, InstPlace.transform.rotation);
        MarkersScenesubList = GameObject.FindObjectsOfType<MarkerMonoBehaviour>();
        MarkersSceneList.Add(MarkersScenesubList[0]);
        Keyboard.SetActive(true);
        KeyboardComp.ActualTextOutput = MarkersSceneList[MarkersSceneList.Count-1].MarkerTextOutput;
        //KeyboardComp.UpdateTMPTarget();
    }

    public void IndexMarker0()
    {
        this.SendMessage("InstantiateMarker", 0, SendMessageOptions.DontRequireReceiver);
    }

    public void IndexMarker1()
    {
        this.SendMessage("InstantiateMarker", 1, SendMessageOptions.DontRequireReceiver);
    }
    public void IndexMarker2()
    {
        this.SendMessage("InstantiateMarker", 2, SendMessageOptions.DontRequireReceiver);
    }
    public void IndexMarker3()
    {
        this.SendMessage("InstantiateMarker", 3, SendMessageOptions.DontRequireReceiver);
    }

    //Coroutine
    IEnumerator Rebound()
    {
        yield return new WaitForSeconds(0.5f);
        ReboundsOK = true;
    }
}
