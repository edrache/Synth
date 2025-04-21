using UnityEngine;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine.Timeline;

[CustomTimelineEditor(typeof(PianoRollTrack))]
public class PianoRollTrackEditor : TrackEditor
{
    public override TrackDrawOptions GetTrackOptions(TrackAsset track, Object binding)
    {
        var options = base.GetTrackOptions(track, binding);
        options.trackColor = new Color(0.2f, 0.8f, 0.2f);
        return options;
    }

    public override void OnCreate(TrackAsset track, TrackAsset copiedFrom)
    {
        base.OnCreate(track, copiedFrom);
        
        var pianoRollTrack = track as PianoRollTrack;
        if (pianoRollTrack != null)
        {
            // Znajdź pierwszy ModularSynth w scenie
            var synth = Object.FindObjectOfType<ModularSynth>();
            if (synth != null)
            {
                pianoRollTrack.TargetSynthName = synth.name;
                Debug.Log($"Assigned ModularSynth '{synth.name}' to PianoRollTrack");
            }
        }
    }

    public override void OnTrackChanged(TrackAsset track)
    {
        base.OnTrackChanged(track);
        
        var pianoRollTrack = track as PianoRollTrack;
        if (pianoRollTrack != null)
        {
            // Sprawdź czy przypisany syntezator nadal istnieje
            if (!string.IsNullOrEmpty(pianoRollTrack.TargetSynthName))
            {
                var synth = Object.FindObjectOfType<ModularSynth>();
                if (synth == null || synth.name != pianoRollTrack.TargetSynthName)
                {
                    Debug.LogWarning($"ModularSynth '{pianoRollTrack.TargetSynthName}' not found in scene!");
                }
            }
        }
    }
}

[CustomEditor(typeof(PianoRollTrack))]
public class PianoRollTrackInspector : Editor
{
    public override void OnInspectorGUI()
    {
        var track = target as PianoRollTrack;
        if (track == null) return;

        EditorGUI.BeginChangeCheck();
        
        var director = TimelineEditor.inspectedDirector;
        if (director == null)
        {
            EditorGUILayout.HelpBox("Select a Timeline asset to edit the track.", MessageType.Info);
            return;
        }

        var currentSynthName = track.TargetSynthName;
        var synths = Object.FindObjectsOfType<ModularSynth>();
        var synthNames = new string[synths.Length + 1];
        synthNames[0] = "None";
        for (int i = 0; i < synths.Length; i++)
        {
            synthNames[i + 1] = synths[i].name;
        }

        var selectedIndex = 0;
        for (int i = 0; i < synthNames.Length; i++)
        {
            if (synthNames[i] == currentSynthName)
            {
                selectedIndex = i;
                break;
            }
        }

        var newIndex = EditorGUILayout.Popup("Target Synth", selectedIndex, synthNames);
        
        if (EditorGUI.EndChangeCheck() && newIndex != selectedIndex)
        {
            Undo.RecordObject(track, "Change Target Synth");
            track.TargetSynthName = newIndex == 0 ? string.Empty : synthNames[newIndex];
            EditorUtility.SetDirty(track);
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
        }

        if (string.IsNullOrEmpty(track.TargetSynthName))
        {
            EditorGUILayout.HelpBox("No ModularSynth assigned! Notes will not play.", MessageType.Warning);
            
            if (GUILayout.Button("Find ModularSynth in Scene"))
            {
                var synth = Object.FindObjectOfType<ModularSynth>();
                if (synth != null)
                {
                    Undo.RecordObject(track, "Assign Found ModularSynth");
                    track.TargetSynthName = synth.name;
                    EditorUtility.SetDirty(track);
                    TimelineEditor.Refresh(RefreshReason.ContentsModified);
                }
                else
                {
                    EditorUtility.DisplayDialog("No ModularSynth Found", 
                        "No ModularSynth component found in the scene. Please add one first.", 
                        "OK");
                }
            }
        }
    }
}

[CustomEditor(typeof(PianoRollClip))]
public class PianoRollClipEditor : Editor
{
    private static readonly string[] noteNames = new string[]
    {
        "Rest", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
    };

    public override void OnInspectorGUI()
    {
        var clip = target as PianoRollClip;
        if (clip == null) return;

        EditorGUI.BeginChangeCheck();

        // -1 oznacza ciszę (rest)
        int noteIndex = clip.note == -1 ? 0 : (clip.note % 12) + 1;
        int octave = clip.note == -1 ? 0 : (clip.note / 12) - 1;
        
        EditorGUILayout.BeginHorizontal();
        noteIndex = EditorGUILayout.Popup("Note", noteIndex, noteNames);
        if (noteIndex > 0) // Jeśli nie jest to cisza
        {
            octave = EditorGUILayout.IntField("Octave", octave);
        }
        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            if (noteIndex == 0) // Cisza
            {
                clip.note = -1;
            }
            else
            {
                clip.note = (octave + 1) * 12 + (noteIndex - 1);
            }
            EditorUtility.SetDirty(clip);
        }

        EditorGUILayout.LabelField("Duration", clip.duration.ToString("F2") + "s");
        EditorGUILayout.LabelField("Start Time", clip.startTime.ToString("F2") + "s");
    }
} 