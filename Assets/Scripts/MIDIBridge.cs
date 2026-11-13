using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using System;
using UnityEngine;
using System.Threading;

public class MIDIBridge 
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    InputDevice _inputDevice;
    IOutputDevice _outputDevice;

    Thread inputThread;
    bool inputRunning = false;

    int currentValue = 0;
    int currentlyLitInt = 0;

    public void StartMIDIBridge()
    {
        ThreadStart starter = new ThreadStart(InitializeInputDevice);
        inputThread = new Thread(starter);
        inputThread.Start();
        InitializeOutputDevice();
        clearColors();
        currentValue = 0;
    }

    public int GetCurrentValue()
    {
        return currentValue;
    }

    private void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
    {
        var midiDevice = (MidiDevice)sender;
        //Debug.Log($"Event received from '{midiDevice.Name}' at {DateTime.Now}: {e.Event}");
        string eventStr = e.Event.ToString();
        if (eventStr.StartsWith("Note On [0]"))
        {
            string[] parts = eventStr.Split('(');
            string[] partsTwo = parts[1].Split(')');
            string[] partsThree = partsTwo[0].Split(',');
            int noteNum = int.Parse(partsThree[0]); 

            string value = partsThree[0].Trim();
            int val = int.Parse(value);

            currentValue = convertNoteNumToValue(val);
            currentlyLitInt = val;
            NoteOnEvent blorp = new NoteOnEvent(new Melanchall.DryWetMidi.Common.SevenBitNumber((byte)val), new Melanchall.DryWetMidi.Common.SevenBitNumber((byte)60));
            _outputDevice.SendEvent(blorp);


        } else if (eventStr.StartsWith("Note Off"))
        {
            currentValue = 0;
            NoteOnEvent blorp = new NoteOnEvent(new Melanchall.DryWetMidi.Common.SevenBitNumber((byte)currentlyLitInt), new Melanchall.DryWetMidi.Common.SevenBitNumber(0));
            currentlyLitInt = 0;
            _outputDevice.SendEvent(blorp);


        }

    }

    //converts a note number to a value 0-63
    int convertNoteNumToValue(int noteNumToConvert)
    {
        int row = Mathf.FloorToInt(noteNumToConvert / 10f) - 1;
        int column = Mathf.FloorToInt(noteNumToConvert % 10f) -1;

        int toReturn = (row * 8) + column;
        return toReturn;
    }

    public void Disposal()
    {

        inputRunning = false;
        (_inputDevice as IDisposable)?.Dispose();
        (_outputDevice as IDisposable)?.Dispose();
        Debug.Log("Objects disposed. Safe to close.");
    }

    public void Shutdown()
    {
        inputRunning = false;
        (_inputDevice as IDisposable)?.Dispose();
        (_outputDevice as IDisposable)?.Dispose();
    }

 
    public void DisposeOutput()
    {
        Debug.Log("Disposing output.");
        (_outputDevice as IDisposable)?.Dispose();
    }

    public void DisposeInput()
    {

        inputRunning = false;
        (_inputDevice as IDisposable)?.Dispose();
    }

    public void setLEDColor(int row, int column, int colorCode)
    {
        int locationCode = (8 - row) * 10 + column + 1;

        NoteOnEvent blorp = new NoteOnEvent(new Melanchall.DryWetMidi.Common.SevenBitNumber((byte)locationCode), new Melanchall.DryWetMidi.Common.SevenBitNumber((byte)colorCode));
        _outputDevice.SendEvent(blorp);

    }

    public void InitializeInputDevice()
    {
        _inputDevice = InputDevice.GetByIndex(0);
        _inputDevice.EventReceived += OnEventReceived;
        _inputDevice.StartEventsListening();

        inputRunning = true;
        while (inputRunning)
        {
            //Connection();
        }

        _inputDevice.StopEventsListening();

    }

    private void InitializeOutputDevice()
    {

        var allOutputDevices = OutputDevice.GetAll();
        _outputDevice = OutputDevice.GetByIndex(1);
    }

    public void clearColors()
    {
        //    DisposeInput();
        //  InitializeOutputDevice();
        int positionVal = 11;
        for (int i = 0; i < 64; i++)
        {
            int colorVal = i;

            positionVal += 1;
            if (positionVal % 10 > 8)
            {
                positionVal += 2;
            }


            NoteOnEvent blorp = new NoteOnEvent(new Melanchall.DryWetMidi.Common.SevenBitNumber((byte)positionVal), new Melanchall.DryWetMidi.Common.SevenBitNumber(0));
            _outputDevice.SendEvent(blorp);

        }
        //  DisposeOutput();
    }
}

