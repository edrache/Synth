using UnityEngine;
using System.Collections.Generic;

public class NoteButtonsManager : MonoBehaviour
{
    [SerializeField] private GameObject noteButtonPrefab;
    [SerializeField] private Transform buttonsContainer;
    [SerializeField] private float buttonSpacing = 10f;
    [SerializeField] private float buttonWidth = 60f;
    [SerializeField] private float buttonHeight = 60f;

    private List<NoteButtonUI> noteButtons = new List<NoteButtonUI>();

    private void Start()
    {
        CreateNoteButtons();
    }

    private void CreateNoteButtons()
    {
        // Clear existing buttons
        foreach (var button in noteButtons)
        {
            if (button != null)
            {
                Destroy(button.gameObject);
            }
        }
        noteButtons.Clear();

        // Create buttons for each note
        string[] noteNames = { "C", "D", "E", "F", "G", "A", "B" };
        for (int i = 0; i < noteNames.Length; i++)
        {
            GameObject buttonObj = Instantiate(noteButtonPrefab, buttonsContainer);
            NoteButtonUI button = buttonObj.GetComponent<NoteButtonUI>();
            
            if (button != null)
            {
                button.Initialize(i + 1, noteNames[i]);
                noteButtons.Add(button);

                // Position the button
                RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = new Vector2(buttonWidth, buttonHeight);
                    rectTransform.anchoredPosition = new Vector2(i * (buttonWidth + buttonSpacing), 0);
                }
            }
        }
    }
} 