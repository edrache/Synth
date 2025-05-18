using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CurveSliderController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CitySequencer targetSequencer;
    [SerializeField] private Slider sliderPrefab;
    [SerializeField] private Transform slidersContainer;
    [SerializeField] private TMP_Text valueTextPrefab;

    [Header("Settings")]
    [SerializeField] private bool controlOctaveCurve = true;
    [SerializeField] private float updateDuration = 0.2f;

    private List<Slider> sliders = new List<Slider>();
    private List<TMP_Text> valueTexts = new List<TMP_Text>();
    private const int POINTS_COUNT = 33; // 0 to 16 with 0.5 step

    private void Start()
    {
        if (targetSequencer == null)
        {
            Debug.LogError("[CurveSliderController] Target sequencer is not assigned!");
            return;
        }

        if (sliderPrefab == null)
        {
            Debug.LogError("[CurveSliderController] Slider prefab is not assigned!");
            return;
        }

        if (slidersContainer == null)
        {
            Debug.LogError("[CurveSliderController] Sliders container is not assigned!");
            return;
        }

        // Verify that container has HorizontalLayoutGroup
        if (slidersContainer.GetComponent<HorizontalLayoutGroup>() == null)
        {
            Debug.LogError("[CurveSliderController] Sliders container must have HorizontalLayoutGroup component!");
            return;
        }

        GenerateSliders();
        UpdateSliderValues();
    }

    private void GenerateSliders()
    {
        // Clear existing sliders
        foreach (var slider in sliders)
        {
            if (slider != null)
            {
                Destroy(slider.gameObject);
            }
        }
        sliders.Clear();
        valueTexts.Clear();

        // Generate new sliders
        for (int i = 0; i < POINTS_COUNT; i++)
        {
            // Create slider
            Slider slider = Instantiate(sliderPrefab, slidersContainer);
            slider.name = $"Slider_{i}";
            
            // Set slider range based on curve type
            if (controlOctaveCurve)
            {
                slider.minValue = -12f;
                slider.maxValue = 12f;
            }
            else
            {
                slider.minValue = 0f;
                slider.maxValue = 1f;
            }

            // Create value text
            TMP_Text valueText = Instantiate(valueTextPrefab, slider.transform);
            valueText.alignment = TextAlignmentOptions.Center;

            // Add listener
            int index = i; // Capture index for lambda
            slider.onValueChanged.AddListener((value) => OnSliderValueChanged(index, value));

            sliders.Add(slider);
            valueTexts.Add(valueText);
        }
    }

    private void OnSliderValueChanged(int index, float value)
    {
        if (targetSequencer == null) return;

        if (controlOctaveCurve)
        {
            targetSequencer.SetOctavePoint(index, value, updateDuration);
        }
        else
        {
            targetSequencer.SetVelocityPoint(index, value, updateDuration);
        }

        UpdateValueText(index, value);
    }

    private void UpdateSliderValues()
    {
        if (targetSequencer == null) return;

        float[] values;
        if (controlOctaveCurve)
        {
            values = targetSequencer.GetAllOctavePointValues();
        }
        else
        {
            values = targetSequencer.GetAllVelocityPointValues();
        }

        for (int i = 0; i < Mathf.Min(values.Length, sliders.Count); i++)
        {
            sliders[i].value = values[i];
            UpdateValueText(i, values[i]);
        }
    }

    private void UpdateValueText(int index, float value)
    {
        if (index < 0 || index >= valueTexts.Count) return;
        
        if (controlOctaveCurve)
        {
            valueTexts[index].text = $"{value:F1}";
        }
        else
        {
            valueTexts[index].text = $"{value:F2}";
        }
    }

    public void SetControlOctaveCurve(bool controlOctave)
    {
        if (controlOctaveCurve == controlOctave) return;

        controlOctaveCurve = controlOctave;
        
        // Update slider ranges
        foreach (var slider in sliders)
        {
            if (controlOctaveCurve)
            {
                slider.minValue = -12f;
                slider.maxValue = 12f;
            }
            else
            {
                slider.minValue = 0f;
                slider.maxValue = 1f;
            }
        }

        UpdateSliderValues();
    }

    private void OnDestroy()
    {
        // Clean up
        foreach (var slider in sliders)
        {
            if (slider != null)
            {
                slider.onValueChanged.RemoveAllListeners();
            }
        }
    }
} 