using System;
using System.Collections.Generic;
using System.IO.Ports;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static WorkshopControllerContainer;

public class UniversalAltCtrlInput : MonoBehaviour
{
    //to support event-based input rather than continuous polling
    public event InputValueChangedHandler InputValueChanged;
    public delegate void InputValueChangedHandler(InputValueChangedArgs e);

    //allows developers to select whether they want digital or analog output types. All controller classes support both, but behavior is different.
    public enum OUTPUT_TYPE { Digital, Analog };

    [Tooltip("Select the controller that will be connected here.")]
    WorkshopController.CONTROLLER_TYPE controllerType;

    [Tooltip("Whether to output digital (0 or 1) or analog (0 to 1023) values. Some controllers only support digital values, and will only output 0 or 1023 when set to Analog. Refer to your controller's documentation for reference.")]
    OUTPUT_TYPE outputType;


    //utility variables
    WorkshopControllerContainer currentController;

    int currentValue;
    int prevValue;
    bool isStarted;

    Dictionary<string, WorkshopController.CONTROLLER_TYPE> controllerTypeConversion;

    private void Start()
    {
        isStarted = false;
        controllerTypeConversion = new Dictionary<string, WorkshopController.CONTROLLER_TYPE>();
        foreach (WorkshopController.CONTROLLER_TYPE controllerType in WorkshopController.CONTROLLER_TYPE.GetValues(typeof(WorkshopController.CONTROLLER_TYPE)))
        {
            controllerTypeConversion.Add(controllerType.ToString(), controllerType);
        }
    }

    public WorkshopController.CONTROLLER_TYPE GetControllerType()
    {
        return controllerType;
    }

    public List<string> GetControllerTypeNames()
    {
        List<string> types = new List<string>();

        foreach (string key in controllerTypeConversion.Keys)
        {
            types.Add(key);
        }

        return types;
    }

    public OUTPUT_TYPE GetCurrentOutputType()
    {
        return outputType;
    }

    void StartControllerInput()
    {
        //create instance of controller parent class, which constructs an instance of the selected controller class.
        currentController = new WorkshopControllerContainer(controllerType, gameObject);

        //initialize values
        currentValue = 0;
        prevValue = 0;
        isStarted = true;
    }

    public void SelectControllerTypeAndStart(string controllerTypeStr)
    {
        if (controllerTypeConversion.ContainsKey(controllerTypeStr))
        {
            controllerType = controllerTypeConversion[controllerTypeStr];
            StartControllerInput();
        }
    }

    public void SelectControllerTypeWithoutStarting(string controllerTypeStr)
    {

        if (controllerTypeConversion.ContainsKey(controllerTypeStr))
        {
            controllerType = controllerTypeConversion[controllerTypeStr];
        }
    }

    public void StartSelectedController()
    {


        StartControllerInput();

    }

    //Update polls for input, updates values, and throws an event if the value changes regardless of output type.
    void Update()
    {
        if (isStarted && currentController != null)
        {
            //poll for input, depending on what kind of input/output is desired
            int currentValContainer = currentValue;
            int nextValContainer = currentController.PollForValue(outputType);
            if (nextValContainer != -1)
            {
                prevValue = currentValContainer;
                currentValue = nextValContainer;

                //throw an event if the value has changed. 
                if (prevValue != currentValue && InputValueChanged != null)
                {
                    InputValueChanged(new InputValueChangedArgs(prevValue, currentValue));
                }
            }
        }
    }

    //supports continuous polling
    public int GetCurrentValue()
    {
        return currentValue;
    }


    //series of utility methods to do this via a UI toggle bc I'm lazy
    public void SelectOutputType(OUTPUT_TYPE NewOutputType)
    {
        outputType = NewOutputType;
    }

    public void ToggleOutputType()
    {
        if (outputType == OUTPUT_TYPE.Digital)
        {
            SelectOutputType(OUTPUT_TYPE.Analog);
        }
        else
        {
            SelectOutputType(OUTPUT_TYPE.Digital);
        }
    }

    private void OnApplicationQuit()
    {
        if (currentController != null)
        {
            currentController.Shutdown();
        }
    }
}

public class InputValueChangedArgs : EventArgs
{
    public InputValueChangedArgs(int priorValue, int currentValue)
    {
        this.priorValue = priorValue;
        this.currentValue = currentValue;
    }

    private int priorValue;
    private int currentValue;

    public int getPriorValue()
    {
        return priorValue;
    }

    public int getCurrentValue()
    {
        return currentValue;
    }

}

public class WorkshopControllerContainer
{
    WorkshopController controller;

    public WorkshopControllerContainer(WorkshopController.CONTROLLER_TYPE controllerType, GameObject altCtrlInputObj)
    {
        switch (controllerType)
        {
            case WorkshopController.CONTROLLER_TYPE.RubberChicken:
                controller = new RubberChicken(altCtrlInputObj);
                break;
            case WorkshopController.CONTROLLER_TYPE.MannequinHand:
                controller = new MannequinHand(altCtrlInputObj);
                break;
            case WorkshopController.CONTROLLER_TYPE.Tentacle:
                controller = new Tentacle(altCtrlInputObj);
                break;
            case WorkshopController.CONTROLLER_TYPE.KachunkLever:
                controller = new KachunkLever(altCtrlInputObj);
                break;
            case WorkshopController.CONTROLLER_TYPE.BigOleButton:
                controller = new BigOleButton(altCtrlInputObj);
                break;
            case WorkshopController.CONTROLLER_TYPE.MIDIPad:
                controller = new MIDIPad(altCtrlInputObj);
                break;
            default:
                controller = new RubberChicken(altCtrlInputObj);
                break;

        }
    }

    public int PollForValue(UniversalAltCtrlInput.OUTPUT_TYPE outputType)
    {
        return controller.PollForValue(outputType);
    }

    public void Shutdown()
    {
        controller.Shutdown();
    }

}

public abstract class WorkshopController
{
    public enum CONTROLLER_TYPE { RubberChicken, MannequinHand, Tentacle, KachunkLever, BigOleButton, MIDIPad };
    public CONTROLLER_TYPE controllerType;

    protected GameObject altCtrlInput;
    protected SerialController mySerialController;

    private int readTimeout = 10000;
    private int writeTimeout = 10000;
    int baudRate = 9600;
    public abstract int PollForValue(UniversalAltCtrlInput.OUTPUT_TYPE outputType);
    public abstract void initializeController();

    public abstract void Shutdown();

    /// <summary>
    /// Utility function to poll for a COM port rather than having to program this manually. Assumes that serial data starts with a "ALTCTRL" code
    /// </summary>
    /// <returns>Integer representing the expected port number, or -1 if no suitable port input found</returns>
    public int PollForComPort()
    {
        int toReturn = -1;


        for (int portNum = 1; portNum < 256; portNum++)
        {
            string comPortCode = "COM" + portNum.ToString();


            try
            {
                //port ssetup
                SerialPort serialPort = new SerialPort(comPortCode, baudRate);
                serialPort.ReadTimeout = readTimeout;
                serialPort.WriteTimeout = writeTimeout;

                //attempt to open and read port
                serialPort.Open();

                for (int i = 0; i < 5; i++) //repeat a couple times in case there are timing or formatting errors or other such things
                {
                    String serialOutput = serialPort.ReadLine();

                    //if succeeds, check for "ALTCTRL" code at start of serial input
                    if (serialOutput.StartsWith("ALTCTRL"))
                    {
                        serialPort.Close(); //don't forget to close the port or bad stuff will happen! 
                        return portNum;
                    }
                }
            }
            catch (Exception e)
            {
                // Debug.Log(e);

                //if fails, move on/do nothing
            }


        }

        return toReturn;
    }


    public int ParseArduinoOutput(String serialStr)
    {
        if (serialStr != null && serialStr.StartsWith("ALTCTRL"))
        {
            String value = serialStr.Substring(7);
            if (value != null)
            {
                return int.Parse(value);
            }
        }
        return -1;
    }

    public int AnalogToDigital(int toConvert)
    {
        if (toConvert == -1)
        {
            return -1;
        }
        if (toConvert > 512)
        {
            return 1;
        }
        return 0;
    }

}

public class RubberChicken : WorkshopController
{

    public RubberChicken(GameObject altCtrlInputObject)
    {
        controllerType = CONTROLLER_TYPE.RubberChicken;
        altCtrlInput = altCtrlInputObject;
        initializeController();
    }

    public override void initializeController()
    {
        int port = PollForComPort();
        if (port != -1)
        {
            mySerialController = altCtrlInput.AddComponent<SerialController>();

            mySerialController.portName = "COM" + port.ToString();
            mySerialController.StartSerialController();
        }
    }

    public override int PollForValue(UniversalAltCtrlInput.OUTPUT_TYPE outputType)
    {
        if (mySerialController != null)
        {
            int polledValue = ParseArduinoOutput(mySerialController.ReadSerialMessage());
            //parse the message
            //convert if necessary
            if (outputType == UniversalAltCtrlInput.OUTPUT_TYPE.Digital)
            {
                return AnalogToDigital(polledValue);
            }
            else
            {
                return polledValue;
            }
        }
        return 0;
    }

    public override void Shutdown()
    {
        mySerialController.StopSerialController();
    }
}

public class MannequinHand : WorkshopController
{
    public MannequinHand(GameObject altCtrlInputObject)
    {
        controllerType = CONTROLLER_TYPE.MannequinHand;
        altCtrlInput = altCtrlInputObject;
        initializeController();
    }

    public override void initializeController()
    {
        int port = PollForComPort();
        if (port != -1)
        {
            mySerialController = altCtrlInput.AddComponent<SerialController>();

            mySerialController.portName = "COM" + port.ToString();
            mySerialController.StartSerialController();
        }
    }

    public override int PollForValue(UniversalAltCtrlInput.OUTPUT_TYPE outputType)
    {
        if (mySerialController != null)
        {
            int polledValue = ParseArduinoOutput(mySerialController.ReadSerialMessage());
            //parse the message
            //convert if necessary
            if (outputType == UniversalAltCtrlInput.OUTPUT_TYPE.Digital)
            {
                return AnalogToDigital(polledValue);
            }
            else
            {
                return polledValue;
            }
        }
        return 0;
    }

    public override void Shutdown()
    {
        mySerialController.StopSerialController();
    }
}

public class Tentacle : WorkshopController
{
    public Tentacle(GameObject altCtrlInputObject)
    {
        controllerType = CONTROLLER_TYPE.Tentacle;
        altCtrlInput = altCtrlInputObject;
        initializeController();
    }

    public override void initializeController()
    {
        int port = PollForComPort();
        if (port != -1)
        {
            mySerialController = altCtrlInput.AddComponent<SerialController>();

            mySerialController.portName = "COM" + port.ToString();
            mySerialController.StartSerialController();
        }
    }

    public override int PollForValue(UniversalAltCtrlInput.OUTPUT_TYPE outputType)
    {
        if (mySerialController != null)
        {
            int polledValue = ParseArduinoOutput(mySerialController.ReadSerialMessage());
            //parse the message
            //convert if necessary
            if (outputType == UniversalAltCtrlInput.OUTPUT_TYPE.Digital)
            {
                return AnalogToDigital(polledValue);
            }
            else
            {
                return polledValue;
            }
        }
        return 0;
    }

    public override void Shutdown()
    {
        mySerialController.StopSerialController();
    }
}

public class KachunkLever : WorkshopController
{
    public KachunkLever(GameObject altCtrlInputObject)
    {
        controllerType = CONTROLLER_TYPE.KachunkLever;
        altCtrlInput = altCtrlInputObject;
        initializeController();
    }

    public override void initializeController()
    {
        int port = PollForComPort();
        if (port != -1)
        {
            mySerialController = altCtrlInput.AddComponent<SerialController>();

            mySerialController.portName = "COM" + port.ToString();
            mySerialController.StartSerialController();
        }
    }

    public override int PollForValue(UniversalAltCtrlInput.OUTPUT_TYPE outputType)
    {

        //this is kind of a special case of analog with values 0 (down), 1 (floating), 2 (up). Digital version only registers up or down.
        if (mySerialController != null)
        {
            int polledValue = ParseArduinoOutput(mySerialController.ReadSerialMessage());

            if (outputType == UniversalAltCtrlInput.OUTPUT_TYPE.Digital)
            {
                //digital output
                if (polledValue != 1)
                {
                    return 1;
                }
                return 0;
            }
            else
            {
                return polledValue;
            }
        }
        return 0;
    }

    public override void Shutdown()
    {
        mySerialController.StopSerialController();
    }
}

public class BigOleButton : WorkshopController
{
    public BigOleButton(GameObject altCtrlInputObject)
    {
        controllerType = CONTROLLER_TYPE.BigOleButton;
        altCtrlInput = altCtrlInputObject;
        initializeController();
    }

    public override void initializeController()
    {
        int port = PollForComPort();
        if (port != -1)
        {
            mySerialController = altCtrlInput.AddComponent<SerialController>();

            mySerialController.portName = "COM" + port.ToString();
            mySerialController.StartSerialController();
        }
    }

    public override int PollForValue(UniversalAltCtrlInput.OUTPUT_TYPE outputType)
    {
        if (mySerialController != null)
        {
            int polledValue = ParseArduinoOutput(mySerialController.ReadSerialMessage());

            return polledValue;

        }
        return 0;
    }

    public override void Shutdown()
    {
        mySerialController.StopSerialController();
    }
}

public class MIDIPad : WorkshopController //THIS ONE IS NOT ARDUINO-BASED
{

    private MIDIBridge midiBridge;
    public MIDIPad(GameObject altCtrlInputObject)
    {
        controllerType = CONTROLLER_TYPE.MIDIPad;
        altCtrlInput = altCtrlInputObject;
        initializeController();
    }

    public override void initializeController()
    {
        midiBridge = new MIDIBridge();
        midiBridge.StartMIDIBridge();
    }

    public override int PollForValue(UniversalAltCtrlInput.OUTPUT_TYPE outputType)
    {
        int polledValue = midiBridge.GetCurrentValue();

        if (outputType == UniversalAltCtrlInput.OUTPUT_TYPE.Digital)
        {
            if (polledValue > 31)
            {
                return 1;
            }
            else return 0;
        }
        else
        {
            return polledValue; //returns 0-63
        }
    }

    public override void Shutdown()
    {
        midiBridge.Shutdown();
    }
}