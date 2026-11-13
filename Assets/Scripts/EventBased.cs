using UnityEngine;

public class EventBased : MonoBehaviour
{
    ControllerSelectionAndStartup controllerSelect;
    UniversalAltCtrlInput altCtrlInput;

    [Tooltip("Select the controller that will be connected here.")]
    public WorkshopController.CONTROLLER_TYPE controllerType;

    [Tooltip("Whether to output digital (0 or 1) or analog (0 to 1023) values. Some controllers only support digital values, and will only output 0 or 1023 when set to Analog. Refer to your controller's documentation for reference.")]
    public UniversalAltCtrlInput.OUTPUT_TYPE outputType;


    // By default, these scripts use SelectControllerAndStart. If you prefer to call these separately, ControllerSelectionAndStartup also offers the selection and start functions inidividually.
    void Start()
    {
        controllerSelect = GetComponent<ControllerSelectionAndStartup>();
        altCtrlInput = GameObject.FindFirstObjectByType<UniversalAltCtrlInput>();
        if (controllerSelect != null && altCtrlInput != null)
        {
            controllerSelect.SelectControllerAndStart(controllerType.ToString());
            altCtrlInput.InputValueChanged += new UniversalAltCtrlInput.InputValueChangedHandler(ReactToControllerValueChangedEvent);
        }
        else
        {
            Debug.LogError("You are missing either a controller selection component on your listener or a UniversalAltCtrl script in your scene.");
        }
    }

    void ReactToControllerValueChangedEvent(InputValueChangedArgs e)
    {
        int priorValue = e.getPriorValue();
        int currentValue = e.getCurrentValue();
        
    }
}
