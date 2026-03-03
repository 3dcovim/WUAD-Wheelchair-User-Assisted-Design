using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit;

public class MarkerMonoBehaviour : MonoBehaviour
{
    public ManagerScene ManagerScene;
    public bool NoKinematicAfterGrabbedVar = false;
    private Canvas Icon;
    private Rigidbody Rigidbody;
    private XRGrabInteractable GrabInteractable;
    private bool alreadySelected = false;                       //Indicates whether the object has been selected or not
    public TextMeshPro MarkerTextOutput;

    // Start is called before the first frame update
    void Start()
    {
        ManagerScene = FindObjectOfType<ManagerScene>();
        Rigidbody = this.GetComponent<Rigidbody>();
        GrabInteractable = this.GetComponent<XRGrabInteractable>();
        Icon = this.GetComponentInChildren<Canvas>();
        MarkerTextOutput = GetComponentInChildren<TextMeshPro>();

        Rigidbody.isKinematic = true;

        ManagerScene.UpdateMarkerCount(true);
        MarkerTextOutput.text = "";
    }

    private void Update()
    {
        if (NoKinematicAfterGrabbedVar) NoKinematicAfterGrabbed();
        else AlwaysKinematic();

        Icon.transform.eulerAngles = new Vector3(90, 0, 0);
    }

    public void NoKinematicAfterGrabbed()
    {
        //The Object will be kinematic when instantiated. After being selected, it will behave as usual.
        
        if (GrabInteractable.isSelected)
        {
            Rigidbody.isKinematic = true;
            alreadySelected = true;
            ManagerScene.Keyboard.SetActive(false);

        }
        else if (!GrabInteractable.isSelected && !alreadySelected)
        {
            Rigidbody.isKinematic = true;
        }
        else    Rigidbody.isKinematic = false;
        
    }

    public void AlwaysKinematic()
    {
        if (GrabInteractable.isSelected)
        {
            Rigidbody.isKinematic = true;
            alreadySelected = true;
            ManagerScene.Keyboard.SetActive(false);

        }
    }
    private void OnDestroy()
    {
        ManagerScene.UpdateMarkerCount(false);
    }
}
