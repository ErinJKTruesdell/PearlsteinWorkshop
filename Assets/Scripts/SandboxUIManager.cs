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

    bool eventShowing;
    void Start()
    {
        altCtrlInput = GameObject.FindFirstObjectByType<UniversalAltCtrlInput>();

        if(altCtrlInput != null)
        {
            //subscribe to events
            altCtrlInput.InputValueChanged += new UniversalAltCtrlInput.InputValueChangedHandler(ReactToControllerValueChangedEvent);
            outputTypeText.text = altCtrlInput.outputType.ToString();
            controllerTypeLabel.text = "Controller Type: " + altCtrlInput.controllerType.ToString();
        } else
        {
            Debug.LogError("Error! Your sandbox scene is missing a Universal Alt Ctrl Input script.");
        }
            eventIndicator.SetActive(false);
        eventShowing = false;

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
            outputTypeText.text = altCtrlInput.outputType.ToString();
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
