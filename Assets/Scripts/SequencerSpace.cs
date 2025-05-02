using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum MusicalScale
{
    Chromatic,
    Major,
    Minor,
    PentatonicMajor,
    PentatonicMinor
}

public enum NoteName
{
    C = 0,
    Cs = 1,
    D = 2,
    Ds = 3,
    E = 4,
    F = 5,
    Fs = 6,
    G = 7,
    Gs = 8,
    A = 9,
    As = 10,
    B = 11
}

public class SequencerSpace : MonoBehaviour
{
    [Header("Timeline Settings")]
    [SerializeField] private PlayableDirector timeline;
    [Tooltip("Numer ścieżki na Timeline (licząc od 0)")]
    [SerializeField] private int trackIndex = 0;
    
    [Header("Space Settings")]
    [Tooltip("Długość timeline w sekundach odpowiadająca szerokości przestrzeni")]
    public float timelineLength = 4f;
    [Tooltip("Bazowa nuta MIDI dla Z = 0")]
    [SerializeField] private int baseMidiNote = 60; // C4
    [Tooltip("Ile półtonów odpowiada jednostce na osi Z")]
    [SerializeField] private float semitonesPerUnit = 1f;
    [Tooltip("Warstwa obiektów do śledzenia")]
    [SerializeField] private LayerMask objectsLayer;
    
    [Header("Note Mapping Settings")]
    [Tooltip("Skala muzyczna, w której pojawiają się nuty")]
    public MusicalScale scale = MusicalScale.Major;
    [Tooltip("Dźwięk podstawowy skali (tonacja)")]
    [SerializeField] private NoteName rootNote = NoteName.C;
    [Tooltip("Najniższa oktawa (włącznie)")]
    public int minOctave = 3;
    [Tooltip("Najwyższa oktawa (włącznie)")]
    public int maxOctave = 5;
    [Tooltip("Długość nuty w sekundach dla każdego obiektu")]
    public float noteDuration = 0.25f;

    private float lastTimelineTime = 0f;
    private List<GameObject> trackedObjects = new List<GameObject>();
    private bool sequenceNeedsUpdate = false;

    private static readonly int[][] scaleIntervals = new int[][]
    {
        // Chromatic
        new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 },
        // Major
        new int[] { 0, 2, 4, 5, 7, 9, 11 },
        // Minor
        new int[] { 0, 2, 3, 5, 7, 8, 10 },
        // Pentatonic Major
        new int[] { 0, 2, 4, 7, 9 },
        // Pentatonic Minor
        new int[] { 0, 3, 5, 7, 10 }
    };

    private void Start()
    {
        if (timeline == null)
        {
            Debug.LogError("Timeline nie został przypisany!");
            enabled = false;
            return;
        }

        // Znajdź wszystkie obiekty w przestrzeni sekwencera
        UpdateTrackedObjects();
    }

    private void Update()
    {
        float currentTime = (float)timeline.time;
        // Sprawdź czy Timeline zakończył przebieg (np. wrócił do początku)
        if (currentTime < lastTimelineTime)
        {
            UpdateTrackedObjects(); // aktualizuj listę obiektów tylko na końcu przebiegu Timeline
            if (sequenceNeedsUpdate)
            {
                UpdateSequence();
                sequenceNeedsUpdate = false;
            }
        }
        lastTimelineTime = currentTime;

        // Sprawdź czy któryś z obiektów się poruszył
        CheckObjectsMovement();
    }

    private void UpdateTrackedObjects()
    {
        trackedObjects.Clear();
        var boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null) return;
        Vector3 boxCenter = boxCollider.transform.TransformPoint(boxCollider.center);
        Vector3 boxSize = Vector3.Scale(boxCollider.size, boxCollider.transform.lossyScale);

        Debug.Log($"Szukam obiektów w obszarze: pozycja={boxCenter}, rozmiar={boxSize}, warstwa={objectsLayer}");
        
        Collider[] colliders = Physics.OverlapBox(
            boxCenter,
            boxSize * 0.5f,
            boxCollider.transform.rotation,
            objectsLayer
        );

        Debug.Log($"Znaleziono {colliders.Length} colliderów w obszarze");
        
        foreach (var collider in colliders)
        {
            if (collider.attachedRigidbody != null)
            {
                trackedObjects.Add(collider.gameObject);
                Debug.Log($"Dodano obiekt: {collider.gameObject.name}, pozycja={collider.gameObject.transform.position}, warstwa={collider.gameObject.layer}");
            }
            else
            {
                Debug.Log($"Pominięto obiekt {collider.gameObject.name} - brak Rigidbody");
            }
        }
        Debug.Log($"Łącznie śledzonych obiektów: {trackedObjects.Count}");
    }

    private void CheckObjectsMovement()
    {
        foreach (var obj in trackedObjects)
        {
            if (obj != null)
            {
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (!rb.isKinematic && rb.linearVelocity.magnitude > 0.01f)
                {
                    sequenceNeedsUpdate = true;
                    return;
                }
            }
        }
    }

    private void UpdateSequence()
    {
        if (timeline == null) return;

        var timelineAsset = timeline.playableAsset as TimelineAsset;
        if (timelineAsset == null) return;

        // Pobierz wszystkie ścieżki typu IPianoRollTrack
        var pianoRollTracks = timelineAsset.GetOutputTracks()
            .Where(t => t is IPianoRollTrack)
            .ToList();

        Debug.Log($"Znaleziono {pianoRollTracks.Count} ścieżek PianoRoll");

        if (trackIndex >= pianoRollTracks.Count)
        {
            Debug.LogError($"Nie znaleziono ścieżki o indeksie {trackIndex}! Dostępnych ścieżek: {pianoRollTracks.Count}");
            return;
        }

        var track = pianoRollTracks[trackIndex];
        var pianoRollTrack = track as IPianoRollTrack;
        
        Debug.Log($"Używam ścieżki: {track.name}");

        // Usuń istniejące klipy
        var existingClips = track.GetClips().ToList();
        Debug.Log($"Usuwam {existingClips.Count} istniejących klipów");
        foreach (var clip in existingClips)
        {
            pianoRollTrack.DeleteClip(clip);
        }

        // Dodaj nowe klipy na podstawie pozycji obiektów
        Debug.Log($"Przetwarzam {trackedObjects.Count} obiektów");
        var boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null) return;
        Vector3 boxCenter = boxCollider.transform.TransformPoint(boxCollider.center);
        float width = boxCollider.size.x;
        foreach (var obj in trackedObjects)
        {
            if (obj != null)
            {
                Vector3 pos = obj.transform.position - boxCenter;
                float timePosition = (pos.x / width) * timelineLength + (timelineLength / 2f);
                int midiNote = GetNearestScaleNote(baseMidiNote, pos.z * semitonesPerUnit);
                
                Debug.Log($"Obiekt {obj.name}: pozycja={pos}, czas={timePosition:F2}s, nuta={midiNote} ({GetNoteName(midiNote)})");
                
                // Sprawdź czy obiekt jest w dozwolonym zakresie
                if (timePosition >= 0 && timePosition < timelineLength)
                {
                    var clip = pianoRollTrack.CreateClip();
                    if (clip == null)
                    {
                        Debug.LogError($"Nie udało się utworzyć klipu dla obiektu {obj.name}!");
                        continue;
                    }
                    if (clip.asset == null)
                    {
                        Debug.LogError($"Klip {clip.displayName} nie ma przypisanego assetu!");
                        continue;
                    }
                    var samplerClip = clip.asset as SamplerPianoRollClip;
                    if (samplerClip == null)
                    {
                        Debug.LogError($"Asset klipu nie jest typu SamplerPianoRollClip! Typ: {clip.asset.GetType().Name}");
                        continue;
                    }
                    clip.start = timePosition;
                    clip.duration = noteDuration;
                    samplerClip.midiNote = midiNote;
                    samplerClip.duration = noteDuration;
                    samplerClip.startTime = timePosition;
                    samplerClip.velocity = 0.8f;
                    clip.displayName = $"Note {midiNote} at {timePosition:F2}s";
                    Debug.Log($"Dodano klip: {clip.displayName}");
                }
                else
                {
                    Debug.Log($"Obiekt {obj.name} poza zakresem czasu: {timePosition:F2}s");
                }
            }
        }

        timeline.RebuildGraph();
        Debug.Log("Zakończono aktualizację sekwencji");
    }

    private int GetNearestScaleNote(int midiBase, float z)
    {
        int[] intervals = scaleIntervals[(int)scale];
        int notesPerOctave = intervals.Length;
        int totalNotes = (maxOctave - minOctave + 1) * notesPerOctave;
        var boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null) return midiBase;
        float zSize = boxCollider.size.z;
        float zMin = -zSize / 2f;
        float zMax = zSize / 2f;
        // z: pozycja względem środka collidera
        float zNorm = Mathf.Clamp((z - zMin) / (zMax - zMin), 0f, 0.9999f); // 0...1
        int noteIndex = Mathf.FloorToInt(zNorm * totalNotes);
        noteIndex = Mathf.Clamp(noteIndex, 0, totalNotes - 1);
        int octave = minOctave + (noteIndex / notesPerOctave);
        int interval = intervals[noteIndex % notesPerOctave];
        int root = (int)rootNote;
        return (octave + 1) * 12 + root + interval;
    }

    private void OnDrawGizmos()
    {
        var boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null) return;
        Gizmos.color = Color.cyan;
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = boxCollider.transform.localToWorldMatrix;
        Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        Gizmos.matrix = oldMatrix;

        // Rysuj znaczniki czasu
        Gizmos.color = Color.yellow;
        for (int i = 0; i <= 4; i++)
        {
            float x = (i / 4f) * boxCollider.size.x - (boxCollider.size.x / 2f);
            Vector3 basePos = boxCollider.transform.TransformPoint(boxCollider.center);
            Gizmos.DrawLine(
                basePos + new Vector3(x, 0, -boxCollider.size.z / 2f),
                basePos + new Vector3(x, 0, boxCollider.size.z / 2f)
            );
#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                basePos + new Vector3(x, 0.5f, 0),
                $"{(i * timelineLength / 4f):F2}s"
            );
#endif
        }

        // Rysuj znaczniki nut zgodnie ze skalą i zakresem oktaw
        Gizmos.color = Color.green;
        int[] intervals = scaleIntervals[(int)scale];
        int notesPerOctave = intervals.Length;
        int totalNotes = (maxOctave - minOctave + 1) * notesPerOctave;
        float zStep = boxCollider.size.z / (float)totalNotes;
        Vector3 basePosZ = boxCollider.transform.TransformPoint(boxCollider.center);
        for (int i = 0; i < totalNotes; i++)
        {
            int octave = minOctave + (i / notesPerOctave);
            int interval = intervals[i % notesPerOctave];
            int root = (int)rootNote;
            int midiNote = (octave + 1) * 12 + root + interval;
            float z = -boxCollider.size.z / 2f + i * zStep;
            Gizmos.DrawLine(
                basePosZ + new Vector3(-boxCollider.size.x / 2f, 0, z),
                basePosZ + new Vector3(boxCollider.size.x / 2f, 0, z)
            );
#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                basePosZ + new Vector3(-boxCollider.size.x / 2f - 0.5f, 0.5f, z),
                GetNoteName(midiNote)
            );
#endif
        }
    }

    private string GetNoteName(int midiNote)
    {
        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        int noteIndex = midiNote % 12;
        int octave = (midiNote / 12) - 1;
        return $"{noteNames[noteIndex]}{octave}";
    }

#if UNITY_EDITOR
    [ContextMenu("Stwórz kostki w rogach")]
    private void CreateCornerCubes()
    {
        var boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null) return;
        Vector3 basePos = boxCollider.transform.TransformPoint(boxCollider.center);
        float xHalf = boxCollider.size.x / 2f;
        float zHalf = boxCollider.size.z / 2f;
        Vector3 topLeft = basePos + new Vector3(-xHalf, 0, zHalf);
        Vector3 topRight = basePos + new Vector3(xHalf, 0, zHalf);
        Vector3 bottomLeft = basePos + new Vector3(-xHalf, 0, -zHalf);
        Vector3 bottomRight = basePos + new Vector3(xHalf, 0, -zHalf);

        CreateCubeAtPosition(topLeft, "TopLeft");
        CreateCubeAtPosition(topRight, "TopRight");
        CreateCubeAtPosition(bottomLeft, "BottomLeft");
        CreateCubeAtPosition(bottomRight, "BottomRight");

        Debug.Log("Utworzono kostki w rogach przestrzeni sekwencera");
    }

    private void CreateCubeAtPosition(Vector3 position, string name)
    {
        // Stwórz nowy GameObject z kostką
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = $"SequencerCube_{name}";
        cube.transform.position = position;
        cube.transform.localScale = Vector3.one;

        // Dodaj Rigidbody
        Rigidbody rb = cube.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;

        // Ustaw warstwę
        cube.layer = gameObject.layer;

        // Ustaw rodzica
        cube.transform.parent = transform;

        // Zaznacz nowo utworzony obiekt
        Selection.activeGameObject = cube;
    }
#endif
} 