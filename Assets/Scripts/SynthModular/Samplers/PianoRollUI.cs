using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using TMPro;

public class PianoRollUI : MonoBehaviour
{
    [Header("References")]
    public PlayableDirector timeline;
    public string trackName;
    public int midiNote = 60;
    public float duration = 1f;
    public float startTime = 0f;

    [Header("UI Elements")]
    public TMP_InputField noteInput;
    public TMP_InputField durationInput;
    public TMP_InputField startTimeInput;
    public Button addNoteButton;
    public TMP_Dropdown trackDropdown;

    private void Start()
    {
        InitializeUI();
        PopulateTrackDropdown();
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
        if (trackDropdown == null || timeline == null) return;

        trackDropdown.ClearOptions();
        var timelineAsset = timeline.playableAsset as TimelineAsset;
        if (timelineAsset == null) return;

        foreach (var track in timelineAsset.GetOutputTracks())
        {
            if (track is IPianoRollTrack)
            {
                trackDropdown.options.Add(new TMP_Dropdown.OptionData(track.name));
            }
        }

        if (trackDropdown.options.Count > 0)
        {
            trackDropdown.value = 0;
            trackName = trackDropdown.options[0].text;
        }

        trackDropdown.onValueChanged.AddListener(OnTrackSelected);
    }

    private void OnTrackSelected(int index)
    {
        if (trackDropdown != null && index < trackDropdown.options.Count)
        {
            trackName = trackDropdown.options[index].text;
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

        if (string.IsNullOrEmpty(trackName))
        {
            Debug.LogError("No track selected!");
            return;
        }

        PianoRollManager.AddNote(timeline, trackName, midiNote, startTime, duration);
    }
} 