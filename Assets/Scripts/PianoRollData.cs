using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[System.Serializable]
public class NoteData
{
    public int step;
    public int midi;
    public int length = 1; // in steps
}

[CreateAssetMenu(fileName = "PianoRollData", menuName = "Synth/Piano Roll Data")]
public class PianoRollData : ScriptableObject
{
    public List<NoteData> notes = new List<NoteData>();
    public int bpm = 120;
}

public class PianoRollEditorWindow : EditorWindow
{
    private PianoRollData data;
    private NoteData draggingNote = null;
    private Vector2 dragStartPos;
    private bool isResizing = false;

    private const int steps = 32;
    private const int midiMin = 48; // C3
    private const int midiMax = 72; // C5

    private const int cellSize = 20;
    private const int resizeHandleSize = 6;

    private static int playbackStep = -1;

    [MenuItem("Window/Synth/Piano Roll")]
    public static void ShowWindow()
    {
        GetWindow<PianoRollEditorWindow>("Piano Roll");
    }

    public static void SetPlaybackStep(int step)
    {
        playbackStep = step;
        EditorApplication.QueuePlayerLoopUpdate();
    }

    private void OnGUI()
    {
        data = EditorGUILayout.ObjectField("Piano Roll Data", data, typeof(PianoRollData), false) as PianoRollData;
        if (data == null) return;

        data.bpm = EditorGUILayout.IntSlider("BPM", data.bpm, 40, 300);

        Rect gridRect = GUILayoutUtility.GetRect((steps + 1) * cellSize, (midiMax - midiMin + 1) * cellSize);

        for (int midi = midiMin; midi <= midiMax; midi++)
        {
            int row = (midiMax - midi);

            Rect keyCell = new Rect(
                gridRect.x,
                gridRect.y + row * cellSize,
                cellSize,
                cellSize
            );
            EditorGUI.DrawRect(keyCell, IsBlackKey(midi) ? Color.black : Color.white);
            Handles.color = Color.gray;
            Handles.DrawLine(new Vector2(keyCell.x, keyCell.y), new Vector2(keyCell.x + keyCell.width, keyCell.y));
            Handles.DrawLine(new Vector2(keyCell.x, keyCell.yMax), new Vector2(keyCell.x + keyCell.width, keyCell.yMax));

            for (int step = 0; step < steps; step++)
            {
                Rect cell = new Rect(
                    gridRect.x + (step + 1) * cellSize,
                    gridRect.y + row * cellSize,
                    cellSize,
                    cellSize
                );

                Color background = (step % 4 == 0) ? new Color(0.7f, 0.7f, 0.7f) : Color.gray;
                EditorGUI.DrawRect(cell, background);

                if (playbackStep == step)
                {
                    EditorGUI.DrawRect(new Rect(cell.x, gridRect.y, 2, (midiMax - midiMin + 1) * cellSize), Color.red);
                }

                NoteData note = data.notes.Find(n => n.step == step && n.midi == midi);
                if (note != null)
                {
                    Rect noteRect = new Rect(cell.x, cell.y, note.length * cellSize, cellSize);
                    EditorGUI.DrawRect(noteRect, Color.green);

                    Rect resizeHandle = new Rect(noteRect.xMax - resizeHandleSize, noteRect.y, resizeHandleSize, noteRect.height);
                    EditorGUI.DrawRect(resizeHandle, Color.yellow);

                    if (Event.current.type == EventType.MouseDown)
                    {
                        if (resizeHandle.Contains(Event.current.mousePosition))
                        {
                            draggingNote = note;
                            dragStartPos = Event.current.mousePosition;
                            isResizing = true;
                            Event.current.Use();
                        }
                        else if (noteRect.Contains(Event.current.mousePosition))
                        {
                            if (Event.current.button == 1)
                            {
                                data.notes.Remove(note);
                                EditorUtility.SetDirty(data);
                                Repaint();
                                Event.current.Use();
                            }
                            else
                            {
                                draggingNote = note;
                                dragStartPos = Event.current.mousePosition;
                                isResizing = false;
                                Event.current.Use();
                            }
                        }
                    }
                }

                Handles.color = Color.black;
                Handles.DrawLine(new Vector2(cell.x, cell.y), new Vector2(cell.x + cell.width, cell.y));
                Handles.DrawLine(new Vector2(cell.x, cell.y), new Vector2(cell.x, cell.y + cell.height));

                if (step == steps - 1)
                {
                    Handles.DrawLine(new Vector2(cell.xMax, cell.y), new Vector2(cell.xMax, cell.y + cell.height));
                }
            }
        }

        if (Event.current.type == EventType.MouseDrag && draggingNote != null)
        {
            float dragDistance = Event.current.mousePosition.x - dragStartPos.x;
            int delta = Mathf.RoundToInt(dragDistance / cellSize);

            if (isResizing)
            {
                draggingNote.length = Mathf.Max(1, draggingNote.length + delta);
            }
            else
            {
                draggingNote.length = Mathf.Max(1, draggingNote.length);
            }

            dragStartPos = Event.current.mousePosition;
            EditorUtility.SetDirty(data);
            Repaint();
        }

        if (Event.current.type == EventType.MouseUp && draggingNote != null)
        {
            draggingNote = null;
            isResizing = false;
            Event.current.Use();
        }
    }

    private bool IsBlackKey(int midi)
    {
        int note = midi % 12;
        return note == 1 || note == 3 || note == 6 || note == 8 || note == 10;
    }
}
