using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SandboxUIManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    UniversalAltCtrlInput altCtrlInput;

    public TMP_Text valueText;
    public TMP_Text outputTypeText;
    public TMP_Text controllerTypeLabel;

    public GameObject eventIndicator;

    public TMP_Dropdown controllerTypeDropDown;
    public Button controllerTypeButton;

    bool eventShowing;
    void Start()
    {
       
            eventIndicator.SetActive(false);
        eventShowing = false;

        //populate controller type dropdown
        foreach(WorkshopController.CONTROLLER_TYPE controllerType in WorkshopController.CONTROLLER_TYPE.GetValues(typeof(WorkshopController.CONTROLLER_TYPE)))
        {
            controllerTypeDropDown.options.Add(new TMP_Dropdown.OptionData(controllerType.ToString()));
        }

    }

    

    public void LockInControllerType()
    {
        string s = controllerTypeDropDown.options[controllerTypeDropDown.value].text;
        controllerTypeDropDown.enabled = false;
        controllerTypeButton.enabled = false;
        controllerTypeLabel.text = "Controller Type: " + s;
        altCtrlInput = GameObject.FindFirstObjectByType<UniversalAltCtrlInput>();

        altCtrlInput.SelectControllerTypeAndStart(s);

        if (altCtrlInput != null)
        {
            //subscribe to events
            altCtrlInput.InputValueChanged += new UniversalAltCtrlInput.InputValueChangedHandler(ReactToControllerValueChangedEvent);
        }
        else
        {
            Debug.LogError("Error! Your sandbox scene is missing a Universal Alt Ctrl Input script.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (altCtrlInput != null) { 
            valueText.text = altCtrlInput.GetCurrentValue().ToString();
        }
    }

    public void OnOutputToggleClicked()
    {
        if (altCtrlInput != null)
        {
            altCtrlInput.ToggleOutputType();
            outputTypeText.text = altCtrlInput.GetCurrentOutputType().ToString();
        }
    }

    public void ReactToControllerValueChangedEvent(InputValueChangedArgs e)
    {
        //interrupt!
        if (eventShowing)
        {
            eventIndicator.SetActive(false);
            eventShowing = false;
        }
        StartCoroutine(ShowEvent());
    }

    IEnumerator ShowEvent()
    {
        //help for interrupts
        yield return new WaitForEndOfFrame();
        eventIndicator.SetActive(true);
        eventShowing = true;
        yield return new WaitForSeconds(0.5f);
        eventIndicator.SetActive(false);
        eventShowing = false;
    }

}
