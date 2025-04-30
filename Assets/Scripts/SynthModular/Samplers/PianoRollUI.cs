using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using TMPro;
using System.Collections.Generic;

public class PianoRollUI : MonoBehaviour
{
    [Header("References")]
    public PlayableDirector timeline;
    public int trackIndex = 0;
    public int midiNote = 60;
    public float duration = 1f;
    public float startTime = 0f;

    [Header("UI Elements")]
    public TMP_InputField noteInput;
    public TMP_InputField durationInput;
    public TMP_InputField startTimeInput;
    public Button addNoteButton;
    public TMP_Dropdown trackDropdown;

    private List<TrackAsset> pianoRollTracks = new List<TrackAsset>();

    private void Start()
    {
        FindMissingReferences();
        InitializeUI();
        PopulateTrackDropdown();
    }

    private void FindMissingReferences()
    {
        // Find timeline if not assigned
        if (timeline == null)
        {
            timeline = FindObjectOfType<PlayableDirector>();
            if (timeline != null)
            {
                Debug.Log($"Found PlayableDirector: {timeline.name}");
            }
            else
            {
                Debug.LogError("No PlayableDirector found in scene!");
            }
        }

        // Find UI elements if not assigned
        if (noteInput == null)
        {
            noteInput = GetComponentInChildren<TMP_InputField>();
            if (noteInput != null)
            {
                Debug.Log($"Found note input field: {noteInput.name}");
            }
        }

        if (durationInput == null)
        {
            var inputs = GetComponentsInChildren<TMP_InputField>();
            foreach (var input in inputs)
            {
                if (input != noteInput)
                {
                    durationInput = input;
                    Debug.Log($"Found duration input field: {durationInput.name}");
                    break;
                }
            }
        }

        if (startTimeInput == null)
        {
            var inputs = GetComponentsInChildren<TMP_InputField>();
            foreach (var input in inputs)
            {
                if (input != noteInput && input != durationInput)
                {
                    startTimeInput = input;
                    Debug.Log($"Found start time input field: {startTimeInput.name}");
                    break;
                }
            }
        }

        if (addNoteButton == null)
        {
            addNoteButton = GetComponentInChildren<Button>();
            if (addNoteButton != null)
            {
                Debug.Log($"Found add note button: {addNoteButton.name}");
            }
        }

        if (trackDropdown == null)
        {
            trackDropdown = GetComponentInChildren<TMP_Dropdown>();
            if (trackDropdown != null)
            {
                Debug.Log($"Found track dropdown: {trackDropdown.name}");
            }
        }
    }

    private void InitializeUI()
    {
        if (noteInput != null)
        {
            noteInput.text = midiNote.ToString();
            noteInput.onValueChanged.AddListener(OnNoteChanged);
        }

        if (durationInput != null)
        {
            durationInput.text = duration.ToString();
            durationInput.onValueChanged.AddListener(OnDurationChanged);
        }

        if (startTimeInput != null)
        {
            startTimeInput.text = startTime.ToString();
            startTimeInput.onValueChanged.AddListener(OnStartTimeChanged);
        }

        if (addNoteButton != null)
        {
            addNoteButton.onClick.AddListener(OnAddNoteClicked);
        }
    }

    private void PopulateTrackDropdown()
    {
        if (trackDropdown == null || timeline == null)
        {
            Debug.LogError("Timeline or dropdown reference is missing!");
            return;
        }

        trackDropdown.ClearOptions();
        pianoRollTracks.Clear();
        var timelineAsset = timeline.playableAsset as TimelineAsset;
        if (timelineAsset == null)
        {
            Debug.LogError("Timeline asset is not a TimelineAsset!");
            return;
        }

        Debug.Log("Available tracks in timeline:");
        foreach (var track in timelineAsset.GetOutputTracks())
        {
            Debug.Log($"- Track: {track.name}, Type: {track.GetType().Name}, Is IPianoRollTrack: {track is IPianoRollTrack}");
        }

        int trackNumber = 1;
        foreach (var track in timelineAsset.GetOutputTracks())
        {
            if (track is IPianoRollTrack)
            {
                pianoRollTracks.Add(track);
                trackDropdown.options.Add(new TMP_Dropdown.OptionData($"Track {trackNumber}"));
                Debug.Log($"Added PianoRoll track: {track.name} as Track {trackNumber}");
                trackNumber++;
            }
        }

        if (trackDropdown.options.Count > 0)
        {
            trackDropdown.value = 0;
            trackIndex = 0;
            Debug.Log($"Total PianoRoll tracks found: {pianoRollTracks.Count}");
        }
        else
        {
            Debug.LogWarning("No PianoRoll tracks found in the timeline!");
        }

        trackDropdown.onValueChanged.AddListener(OnTrackSelected);
    }

    private void OnTrackSelected(int index)
    {
        if (trackDropdown != null && index < pianoRollTracks.Count)
        {
            trackIndex = index;
        }
    }

    private void OnNoteChanged(string value)
    {
        if (int.TryParse(value, out int result))
        {
            midiNote = result;
        }
    }

    private void OnDurationChanged(string value)
    {
        if (float.TryParse(value, out float result))
        {
            duration = result;
        }
    }

    private void OnStartTimeChanged(string value)
    {
        if (float.TryParse(value, out float result))
        {
            startTime = result;
        }
    }

    private void OnAddNoteClicked()
    {
        if (timeline == null)
        {
            Debug.LogError("Timeline reference is missing!");
            return;
        }

        if (pianoRollTracks.Count == 0)
        {
            Debug.LogError("No PianoRoll tracks found in the timeline!");
            return;
        }

        if (trackIndex < 0 || trackIndex >= pianoRollTracks.Count)
        {
            Debug.LogError($"Invalid track index: {trackIndex}. Available tracks: {pianoRollTracks.Count}");
            return;
        }

        var track = pianoRollTracks[trackIndex];
        if (track == null)
        {
            Debug.LogError("Selected track is null!");
            return;
        }

        PianoRollManager.AddNote(timeline, track.name, midiNote, startTime, duration);
    }
} 