using UnityEngine;
using UnityEngine.UI;

public class PositionToSliderConverter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CityNoteProgrammer programmer;
    [SerializeField] private Slider targetSlider;

    [Header("Settings")]
    [Tooltip("Value to divide position by")]
    [SerializeField] private float divisor = 1f;

    private void Start()
    {
        if (programmer == null)
        {
            Debug.LogError("[PositionToSliderConverter] CityNoteProgrammer reference is missing!");
            enabled = false;
            return;
        }

        if (targetSlider == null)
        {
            Debug.LogError("[PositionToSliderConverter] Target Slider reference is missing!");
            enabled = false;
            return;
        }
    }

    public void UpdateSliderValue()
    {
        if (programmer == null || targetSlider == null) return;

        float position = programmer.GetCurrentPosition();
        float newValue = position / divisor;
        targetSlider.value = newValue;
    }

    public void SetDivisor(float newDivisor)
    {
        if (newDivisor == 0)
        {
            Debug.LogError("[PositionToSliderConverter] Divisor cannot be zero!");
            return;
        }
        divisor = newDivisor;
    }
} 