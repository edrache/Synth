using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyPrefab : MonoBehaviour
{
    public Button button;
    public TMP_Text noteText;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
        if (noteText == null)
            noteText = GetComponentInChildren<TMP_Text>();
    }
} 