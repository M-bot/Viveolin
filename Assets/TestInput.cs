using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(AudioSource))]
public class TestInput : MonoBehaviour
{

    private SteamVR_TrackedController device;
    private SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device fullDevice;
    private PrimitiveType _currentPrimitiveType = PrimitiveType.Sphere;


    public static double[] freqTable = new double[] {
        110.00, 116.54, 123.47, 130.81, 138.59, 146.83, 155.56, 164.81, 174.61, 185.00,
        196.00, 207.65, 220.00, 233.08, 246.94, 261.63, 277.18, 293.66, 311.13, 329.63,
        349.23, 369.99, 392.00, 415.30, 440.00, 466.16, 493.88, 523.25, 554.37, 587.33,
        622.25, 659.25, 698.46, 739.99, 783.99, 830.61, 880.00, 932.33, 987.77, 1046.50,
        1108.73, 1174.66, 1244.51, 1318.51, 1396.91, 1479.98, 1567.98, 1661.22, 1760.00
    };

    public static string[] notesTable = new string[]
    {
        "A2", "A#2", "B2", "C3", "C#3", "D3", "Eb3", "E3", "F3",
        "F#3", "G3", "G#3", "A3", "A#3", "B3", "C4", "C#4", "D4",
        "Eb4", "E4", "F4", "F#4", "G4", "G#4", "A4", "A#4", "B4",
        "C5", "C#5", "D5", "Eb5", "E5", "F5", "F#5", "G5", "G#5",
        "A5", "A#5", "B5", "C6", "C#6", "D6", "Eb6", "E6", "F6",
        "F#6", "G6", "G#6", "A6"
    };

    public double pitchUnit = 329.63;
    public float pitchOffset = 2;
    
    List<GameObject> indicators = new List<GameObject>();
    List<GameObject> notes = new List<GameObject>();

    // Use this for initialization
    void Start()
    {
        device = GetComponent<SteamVR_TrackedController>();
        trackedObject = GetComponent<SteamVR_TrackedObject>();

        for (int x = 0; x < freqTable.Length; x++)
        {
            // aus.pitch = Mathf.Pow(2, transform.position.y - 1) * pitchOffset;
            float y = Mathf.Log((float)(freqTable[x] / pitchUnit),2) + 1;
            var currentIndicator = GameObject.CreatePrimitive(_currentPrimitiveType);
            currentIndicator.transform.position = new Vector3(0, y, 0);
            currentIndicator.transform.localScale = new Vector3(0.05f, 0.01f, 0.05f);
            if (notesTable[x].Contains("#") || notesTable[x].Contains("b"))
            {
                currentIndicator.GetComponent<Renderer>().material.color = Color.black;
            }
            else if(notesTable[x].Contains("C"))
            {
                currentIndicator.GetComponent<Renderer>().material.color = Color.red;
            }
            indicators.Add(currentIndicator);
        }
        
    }

    private void OnEnable()
    {
        device = GetComponent<SteamVR_TrackedController>();
        device.TriggerClicked += HandleTriggerClicked;
        device.TriggerUnclicked += HandleTriggerUnclicked;
    }

    private void OnDisable()
    {
        device.TriggerClicked -= HandleTriggerClicked;
        device.TriggerUnclicked -= HandleTriggerUnclicked;
    }

    void HandleTriggerClicked(object sender, ClickedEventArgs e)
    {
        fullDevice = SteamVR_Controller.Input((int)trackedObject.index);
    }

    void HandleTriggerUnclicked(object sender, ClickedEventArgs e)
    {
        AudioSource aus = GetComponent<AudioSource>();

    }

    private int numOfClicks = 0;
    // LineRenderer line;
    List<LineRenderer> lineList = new List<LineRenderer>();
    private float startY = 0;
    // List<int> timers = new List<int>();
    List<List<int>> timerList = new List<List<int>>();
    List<List<Vector3>> pointList = new List<List<Vector3>>();

    private void Update()
    {

        for (int x = 0; x < indicators.Count; x++)
        {
            indicators[x].transform.position = new Vector3(transform.position.x + -transform.right.x * 0.1f,
                indicators[x].transform.position.y,
                transform.position.z + -transform.right.z * 0.1f);
            if (notesTable[x].Contains("#") || notesTable[x].Contains("b"))
            {
                indicators[x].transform.position = new Vector3(transform.position.x + transform.right.x * 0.1f,
                indicators[x].transform.position.y,
                transform.position.z + transform.right.z * 0.1f);
            }
        }

        for(int x = 0; x < notes.Count; x++)
        {
            notes[x].transform.position = new Vector3(transform.position.x + -transform.right.x * 0.1f,
                notes[x].transform.position.y,
                transform.position.z + -transform.right.z * 0.1f);
        }
        
        AudioSource aus = GetComponent<AudioSource>();
        aus.volume -= (float)(0.3f * Time.deltaTime);
        if (aus.volume == 0)
        {
            aus.volume = 0;
        }


        for (int i = 0; i < timerList.Count; i++)
        {
            // decrement timers for keyframes already added to line
            for (int j = 0; j < timerList[i].Count; j++)
            {
                timerList[i][j]--;
            }
            // check if oldest timer has expired
            if (timerList[i].Count > 0 && timerList[i][0] < 0)
            {
                timerList[i].RemoveAt(0);
                pointList[i].RemoveAt(0);
                lineList[i].positionCount = pointList[i].Count;
                lineList[i].SetPositions(pointList[i].ToArray());
            }
        }
        // clean up dead lines
        for (int i = lineList.Count - 1; i >= 0; i--)
        {
            if (lineList[i].positionCount == 0)
            {
                Destroy(lineList[i].gameObject);
                lineList.RemoveAt(i);
                timerList.RemoveAt(i);
                pointList.RemoveAt(i);
            }
        }
        
        

        if (fullDevice == null)
        {

        }
        else if (fullDevice.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            GameObject go = new GameObject();
            lineList.Add(go.AddComponent<LineRenderer>());


            lineList[lineList.Count - 1].widthCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0.1f) });

            timerList.Add(new List<int>());
            timerList[timerList.Count - 1].Add(60);

            pointList.Add(new List<Vector3>());
            pointList[pointList.Count - 1].Add(transform.position);
            lineList[lineList.Count - 1].positionCount = pointList[pointList.Count - 1].Count;
            lineList[lineList.Count - 1].SetPositions(pointList[pointList.Count - 1].ToArray());

            lineList[lineList.Count - 1].material = new Material(Shader.Find("Particles/Additive"));

            Keyframe[] keyframes = { new Keyframe(0.0f, 0), new Keyframe(0.5f, 0.1f * pointList[pointList.Count - 1].Count), new Keyframe(1f, 0) };
            lineList[lineList.Count - 1].widthCurve = new AnimationCurve(keyframes);

            //lineList[lineList.Count - 1].startWidth = fullDevice.velocity.magnitude * 0.1f; 
            //lineList[lineList.Count - 1].endWidth = fullDevice.velocity.magnitude * 0.1f;

            startY = transform.position.y;

            numOfClicks = 0;
        }
        else if (fullDevice.GetTouch(SteamVR_Controller.ButtonMask.Trigger))
        {
            if (!fullDevice.GetTouch(SteamVR_Controller.ButtonMask.Grip))
            {
                aus.pitch = Mathf.Pow(2, transform.position.y - 1) * pitchOffset;
                for (int i = 0; i < freqTable.Length; i++)
                {
                    if (aus.pitch < (float)(freqTable[i] / pitchUnit) && i > 0)
                    {
                        aus.pitch = (float)(freqTable[i - 1] / pitchUnit);
                        break;
                    }
                    else if (aus.pitch < (float)(freqTable[i] / pitchUnit) && i == 0)
                        break;
                }
            }
            if (lineList.Count > 0)
            {
                pointList[pointList.Count - 1].Add(transform.position);
                lineList[lineList.Count - 1].positionCount = pointList[pointList.Count - 1].Count;
                lineList[lineList.Count - 1].SetPositions(pointList[pointList.Count - 1].ToArray());
                timerList[timerList.Count - 1].Add(60);

                Gradient gradient = new Gradient();

                gradient.SetKeys(
                        new GradientColorKey[] { new GradientColorKey(new Color(startY / 2, 0.0f, 0.0f), 0.0f), new GradientColorKey(new Color(0.0f, 0.0f, transform.position.y / 2), 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
                );

                lineList[lineList.Count - 1].colorGradient = gradient;
                numOfClicks++;
            }
            fullDevice.TriggerHapticPulse((ushort)(3999 * Mathf.Log10(Mathf.Abs(fullDevice.velocity.magnitude) + 1f)));
            aus.volume = Mathf.Log10(Mathf.Abs(fullDevice.velocity.magnitude * 5 / aus.pitch) + 1f) * 0.5f;
        }
    }

}