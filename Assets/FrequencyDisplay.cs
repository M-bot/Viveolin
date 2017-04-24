using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FrequencyDisplay : MonoBehaviour
{

    public Text freqText;
    public GameObject control;
    AudioSource aus;
    TestInput ti;

    void Start()
    {
        aus = control.GetComponent<AudioSource>();
        ti = control.GetComponent<TestInput>();
    }

    private void Update()
    {
        for (int i = 0; i < TestInput.freqTable.Length; i++)
        {
            if (aus.pitch * ti.pitchUnit <= TestInput.freqTable[i] && i > 0)
            {
                freqText.text = TestInput.notesTable[i - 1] + " " + TestInput.freqTable[i - 1];
                break;
            }
            else if (aus.pitch * ti.pitchUnit < TestInput.freqTable[i] && i == 0)
                break;
        }
    }
}