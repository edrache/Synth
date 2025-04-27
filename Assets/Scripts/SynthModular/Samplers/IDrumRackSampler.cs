using UnityEngine;

public interface IDrumRackSampler
{
    void SetSample(Note note, AudioClip clip);
    void PlayNote(Note note);
    void StopNote(Note note);
    void StopAllNotes();
    bool OneShot { get; set; }
} 