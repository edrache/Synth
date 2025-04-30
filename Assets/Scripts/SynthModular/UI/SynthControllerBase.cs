using UnityEngine;

public abstract class SynthControllerBase : MonoBehaviour
{
    public abstract void PlayNote(int midiNote);
    public abstract void StopNote(int midiNote);
} 