using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SequencerSpace : MonoBehaviour
{
    [Header("Timeline Settings")]
    [SerializeField] private PlayableDirector timeline;
    [Tooltip("Numer ścieżki na Timeline (licząc od 0)")]
    [SerializeField] private int trackIndex = 0;
    
    [Header("Space Settings")]
    [Tooltip("Szerokość przestrzeni sekwencera (oś X)")]
    [SerializeField] private float spaceWidth = 10f;
    [Tooltip("Ile sekund na timeline odpowiada szerokości przestrzeni")]
    [SerializeField] private float timelineLength = 4f;
    [Tooltip("Ile półtonów odpowiada jednostce na osi Z")]
    [SerializeField] private float semitonesPerUnit = 1f;
    [Tooltip("Bazowa nuta MIDI dla Z = 0")]
    [SerializeField] private int baseMidiNote = 60; // C4
    [Tooltip("Warstwa obiektów do śledzenia")]
    [SerializeField] private LayerMask objectsLayer;

    private float lastTimelineTime = 0f;
    private List<GameObject> trackedObjects = new List<GameObject>();
    private bool sequenceNeedsUpdate = false;

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
        // Sprawdź czy timeline zakończył odtwarzanie
        float currentTime = (float)timeline.time;
        if (currentTime < lastTimelineTime)
        {
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
        Vector3 boxCenter = transform.position;
        Vector3 boxSize = new Vector3(spaceWidth, 20f, 20f);

        Debug.Log($"Szukam obiektów w obszarze: pozycja={boxCenter}, rozmiar={boxSize}, warstwa={objectsLayer}");
        
        Collider[] colliders = Physics.OverlapBox(
            boxCenter,
            boxSize * 0.5f,
            Quaternion.identity,
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
        foreach (var obj in trackedObjects)
        {
            if (obj != null)
            {
                Vector3 pos = obj.transform.position - transform.position;
                float timePosition = (pos.x / spaceWidth) * timelineLength;
                int midiNote = Mathf.RoundToInt(baseMidiNote + (pos.z * semitonesPerUnit));
                
                Debug.Log($"Obiekt {obj.name}: pozycja={pos}, czas={timePosition:F2}s, nuta={midiNote} ({GetNoteName(midiNote)})");
                
                // Sprawdź czy obiekt jest w dozwolonym zakresie
                if (timePosition >= 0 && timePosition < timelineLength)
                {
                    var clip = pianoRollTrack.CreateClip();
                    if (clip != null)
                    {
                        clip.start = timePosition;
                        clip.duration = 0.25f;

                        var samplerClip = clip.asset as SamplerPianoRollClip;
                        if (samplerClip != null)
                        {
                            samplerClip.midiNote = midiNote;
                            samplerClip.duration = 0.25f;
                            samplerClip.startTime = timePosition;
                            samplerClip.velocity = 0.8f;
                            clip.displayName = $"Note {midiNote} at {timePosition:F2}s";
                            Debug.Log($"Dodano klip: {clip.displayName}");
                        }
                        else
                        {
                            Debug.LogError($"Nie można utworzyć SamplerPianoRollClip dla obiektu {obj.name}");
                        }
                    }
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

    private void OnDrawGizmos()
    {
        // Rysuj granice przestrzeni sekwencera
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(spaceWidth, 0.1f, 20f)
        );

        // Rysuj znaczniki czasu
        Gizmos.color = Color.yellow;
        for (int i = 0; i <= 4; i++)
        {
            float x = (i / 4f) * spaceWidth - (spaceWidth / 2f);
            Gizmos.DrawLine(
                transform.position + new Vector3(x, 0, -10f),
                transform.position + new Vector3(x, 0, 10f)
            );
            
#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + new Vector3(x, 0.5f, 0),
                $"{i}s"
            );
#endif
        }

        // Rysuj znaczniki nut
        Gizmos.color = Color.green;
        for (int i = -10; i <= 10; i++)
        {
            float z = i;
            int note = baseMidiNote + Mathf.RoundToInt(i * semitonesPerUnit);
            Gizmos.DrawLine(
                transform.position + new Vector3(-spaceWidth/2f, 0, z),
                transform.position + new Vector3(spaceWidth/2f, 0, z)
            );
            
#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + new Vector3(-spaceWidth/2f - 0.5f, 0.5f, z),
                GetNoteName(note)
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
        // Oblicz pozycje rogów
        Vector3 topLeft = transform.position + new Vector3(-spaceWidth/2f, 0, 10f);
        Vector3 topRight = transform.position + new Vector3(spaceWidth/2f, 0, 10f);
        Vector3 bottomLeft = transform.position + new Vector3(-spaceWidth/2f, 0, -10f);
        Vector3 bottomRight = transform.position + new Vector3(spaceWidth/2f, 0, -10f);

        // Stwórz kostki w rogach
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