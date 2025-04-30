using UnityEngine;

public interface ISynthController
{
    void PlayNote(int midiNote);
    void StopNote(int midiNote);
} 