using UnityEngine;
using System.Collections.Generic;

public class DrumRackController : MonoBehaviour
{
    private IDrumRackSampler sampler;
    private Dictionary<KeyCode, Note> keyToNote;

    private void Awake()
    {
        sampler = GetComponent<IDrumRackSampler>();
        if (sampler == null)
        {
            Debug.LogError("DrumRackController requires IDrumRackSampler component!");
            enabled = false;
            return;
        }

        keyToNote = new()
        {
            { KeyCode.A, Note.C },
            { KeyCode.W, Note.CSharp },
            { KeyCode.S, Note.D },
            { KeyCode.E, Note.DSharp },
            { KeyCode.D, Note.E },
            { KeyCode.F, Note.F },
            { KeyCode.T, Note.FSharp },
            { KeyCode.G, Note.G },
            { KeyCode.Y, Note.GSharp },
            { KeyCode.H, Note.A },
            { KeyCode.U, Note.ASharp },
            { KeyCode.J, Note.B }
        };
    }

    private void Update()
    {
        if (!Application.isPlaying) return;

        foreach (var kvp in keyToNote)
        {
            if (Input.GetKeyDown(kvp.Key))
            {
                sampler.PlayNote(kvp.Value);
            }

            if (Input.GetKeyUp(kvp.Key))
            {
                sampler.StopNote(kvp.Value);
            }
        }
    }
} 