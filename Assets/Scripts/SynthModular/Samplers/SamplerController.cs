using UnityEngine;
using System.Collections.Generic;

public class SamplerController : MonoBehaviour
{
    private ISampler sampler;
    private Dictionary<KeyCode, int> keyToNote;

    private void Awake()
    {
        sampler = GetComponent<ISampler>();
        if (sampler == null)
        {
            Debug.LogError("SamplerController requires ISampler component!");
            enabled = false;
            return;
        }

        keyToNote = new()
        {
            { KeyCode.A, 60 }, // C
            { KeyCode.W, 61 }, // C#
            { KeyCode.S, 62 }, // D
            { KeyCode.E, 63 }, // D#
            { KeyCode.D, 64 }, // E
            { KeyCode.F, 65 }, // F
            { KeyCode.T, 66 }, // F#
            { KeyCode.G, 67 }, // G
            { KeyCode.Y, 68 }, // G#
            { KeyCode.H, 69 }, // A
            { KeyCode.U, 70 }, // A#
            { KeyCode.J, 71 }, // B
            { KeyCode.K, 72 }  // C (octave up)
        };
    }

    private void Update()
    {
        if (!Application.isPlaying) return;

        // Handle octave changes
        if (Input.GetKeyDown(KeyCode.Z))
            sampler.SetOctave(sampler.GetOctave() - 1);
        if (Input.GetKeyDown(KeyCode.X))
            sampler.SetOctave(sampler.GetOctave() + 1);

        // Handle note input
        foreach (var kvp in keyToNote)
        {
            int midi = kvp.Value + sampler.GetOctave() * 12;

            if (Input.GetKeyDown(kvp.Key))
            {
                sampler.PlayNote(midi);
            }

            if (Input.GetKeyUp(kvp.Key))
            {
                sampler.StopNote(midi);
            }
        }
    }
} 