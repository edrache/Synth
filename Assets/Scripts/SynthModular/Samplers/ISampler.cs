using UnityEngine;

public interface ISampler
{
    void LoadSample(AudioClip clip, int rootNote);
    void PlayNote(int midiNote);
    void StopNote(int midiNote);
    void SetOctave(int octave);
    int GetOctave();
    void StopAllNotes();
    bool OneShot { get; set; }
} 