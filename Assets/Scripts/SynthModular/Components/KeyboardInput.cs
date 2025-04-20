using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(ModularSynthComponent))]
public class KeyboardInput : MonoBehaviour
{
    private ModularSynthComponent synth;
    private int octaveOffset = 0;
    private Dictionary<KeyCode, int> activeVoices = new Dictionary<KeyCode, int>();

    // Mapowanie klawiszy do nut (QWERTY)
    private readonly KeyCode[] whiteKeys = new KeyCode[]
    {
        KeyCode.A,  // C
        KeyCode.S,  // D
        KeyCode.D,  // E
        KeyCode.F,  // F
        KeyCode.G,  // G
        KeyCode.H,  // A
        KeyCode.J   // B
    };

    private readonly KeyCode[] blackKeys = new KeyCode[]
    {
        KeyCode.W,  // C#
        KeyCode.E,  // D#
        KeyCode.T,  // F#
        KeyCode.Y,  // G#
        KeyCode.U   // A#
    };

    private readonly MusicUtils.Note[] whiteNotes = new MusicUtils.Note[]
    {
        MusicUtils.Note.C,
        MusicUtils.Note.D,
        MusicUtils.Note.E,
        MusicUtils.Note.F,
        MusicUtils.Note.G,
        MusicUtils.Note.A,
        MusicUtils.Note.B
    };

    private readonly MusicUtils.Note[] blackNotes = new MusicUtils.Note[]
    {
        MusicUtils.Note.CSharp,
        MusicUtils.Note.DSharp,
        MusicUtils.Note.FSharp,
        MusicUtils.Note.GSharp,
        MusicUtils.Note.ASharp
    };

    private void Start()
    {
        synth = GetComponent<ModularSynthComponent>();
    }

    private void Update()
    {
        // Obsługa białych klawiszy
        for (int i = 0; i < whiteKeys.Length; i++)
        {
            if (Input.GetKeyDown(whiteKeys[i]))
            {
                float freq = MusicUtils.GetFrequency(whiteNotes[i], 4 + octaveOffset);
                int voiceId = synth.NoteOn(freq);
                activeVoices[whiteKeys[i]] = voiceId;
                Debug.Log($"Note On: {whiteNotes[i]}{4 + octaveOffset}, Frequency: {freq:F2} Hz");
            }
            if (Input.GetKeyUp(whiteKeys[i]))
            {
                if (activeVoices.TryGetValue(whiteKeys[i], out int voiceId))
                {
                    synth.NoteOff(voiceId);
                    activeVoices.Remove(whiteKeys[i]);
                }
            }
        }

        // Obsługa czarnych klawiszy
        for (int i = 0; i < blackKeys.Length; i++)
        {
            if (Input.GetKeyDown(blackKeys[i]))
            {
                float freq = MusicUtils.GetFrequency(blackNotes[i], 4 + octaveOffset);
                int voiceId = synth.NoteOn(freq);
                activeVoices[blackKeys[i]] = voiceId;
                Debug.Log($"Note On: {blackNotes[i]}{4 + octaveOffset}, Frequency: {freq:F2} Hz");
            }
            if (Input.GetKeyUp(blackKeys[i]))
            {
                if (activeVoices.TryGetValue(blackKeys[i], out int voiceId))
                {
                    synth.NoteOff(voiceId);
                    activeVoices.Remove(blackKeys[i]);
                }
            }
        }

        // Zmiana oktawy
        if (Input.GetKeyDown(KeyCode.Z))
        {
            octaveOffset = Mathf.Max(octaveOffset - 1, -2);
            Debug.Log($"Octave: {4 + octaveOffset}");
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            octaveOffset = Mathf.Min(octaveOffset + 1, 2);
            Debug.Log($"Octave: {4 + octaveOffset}");
        }
    }
} 