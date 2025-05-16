using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TimelineButton : MonoBehaviour
{
    [SerializeField] private CitySequencer sequencer;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("[TimelineButton] No Button component found!");
            return;
        }

        if (sequencer == null)
        {
            sequencer = FindObjectOfType<CitySequencer>();
            if (sequencer == null)
            {
                Debug.LogError("[TimelineButton] No CitySequencer found in scene!");
                return;
            }
        }

        button.onClick.AddListener(OnButtonClick);
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClick);
        }
    }

    private void OnButtonClick()
    {
        if (sequencer != null)
        {
            sequencer.UpdateSequence();
            Debug.Log("[TimelineButton] Sequence updated");
        }
    }
} 