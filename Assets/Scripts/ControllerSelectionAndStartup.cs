using NUnit.Framework;
using UnityEngine;

public class ControllerSelectionAndStartup : MonoBehaviour
{


    UniversalAltCtrlInput altCtrlInput;
    bool controllerTypeSelected = false;

    private void Start()
    {
        altCtrlInput = GameObject.FindFirstObjectByType<UniversalAltCtrlInput>();
        controllerTypeSelected = false;
    }

    //A List<string> of controller type strings is available by accessing UniversalAltCtrlInput.GetControllerTypeNames()

    /// <summary>
    /// Selects a controller type without starting to read from the connected controller.
    /// </summary>
    /// <param name="controllerTypeName">The string name of the controller type. A List<string> of controller type strings is available by accessing UniversalAltCtrlInput.GetControllerTypeNames()</param>
    public void SelectController(string controllerTypeName)
    {
        if (altCtrlInput != null)
        {

            altCtrlInput.SelectControllerTypeWithoutStarting(controllerTypeName);
            controllerTypeSelected = true;
        }
    }

    /// <summary>
    /// Once a controller type has been selected, starts reading from the selected controller. If no controller type has been selected, does nothing.
    /// </summary>
    public void StartSelectedController()
    {
        if (altCtrlInput != null && controllerTypeSelected)
        {
            altCtrlInput.StartSelectedController();
        }
    }

    /// <summary>
    /// Selects a controller type and starts to read from the connected controller. A single call that does what calling SelectController() and StartController() together does.
    /// </summary>
    /// <param name="controllerTypeName">The string name of the controller type. A List<string> of controller type strings is available by accessing UniversalAltCtrlInput.GetControllerTypeNames()</param>
    public void SelectControllerAndStart(string controllerTypeName)
    {
        if (altCtrlInput != null)
        {
            altCtrlInput.SelectControllerTypeWithoutStarting(controllerTypeName);
            controllerTypeSelected = true;
            altCtrlInput.StartSelectedController();
        }
    }
}
