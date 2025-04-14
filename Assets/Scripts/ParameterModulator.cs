using UnityEngine;
using UnityEngine.UI;

public class ParameterModulator : MonoBehaviour
{
    [System.Serializable]
    public enum ParameterType
    {
        // Filter parameters
        Cutoff,
        Resonance,
        
        // Chorus parameters
        ChorusAmount,
        ChorusRate,
        ChorusDepth,
        ChorusFeedback,
        
        // Delay parameters
        DelayAmount,
        DelayFeedback,
        DelayWidth,
        
        // Envelope parameters
        AttackTime,
        DecayTime,
        SustainLevel,
        ReleaseTime,

        // Octave modulation
        OctaveShift
    }

    [System.Serializable]
    public enum DurationType
    {
        QuarterBar,    // 1/4 taktu (4 kroki)
        HalfBar,       // 1/2 taktu (8 kroków)
        OneBar,        // 1 takt (16 kroków)
        TwoBars,       // 2 takty (32 kroki)
        FourBars       // 4 takty (64 kroki)
    }

    [Header("Target")]
    [SerializeField] private VCO targetVCO;

    [Header("Modulation Settings")]
    [SerializeField] private ParameterType parameterType;
    [SerializeField] private float minValue;
    [SerializeField] private float maxValue;
    [SerializeField] private AnimationCurve modulationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private DurationType durationType = DurationType.OneBar;
    [SerializeField] private bool isLooping = false;
    [SerializeField] private bool isActive = true;

    [Header("UI References")]
    [SerializeField] private Button startStopButton;
    [SerializeField] private Toggle loopToggle;
    [SerializeField] private Toggle activeToggle;
    [SerializeField] private Image progressIndicator;

    private float currentTime;
    private float totalDuration;
    private bool isRunning;
    private float originalValue;

    private void Awake()
    {
        ValidateReferences();
        SetupUIListeners();
    }

    private void ValidateReferences()
    {
        if (targetVCO == null)
        {
            Debug.LogError("ParameterModulator: Brak przypisanego VCO!");
            enabled = false;
            return;
        }

        if (startStopButton == null)
            Debug.LogError("ParameterModulator: Brak przypisanego przycisku Start/Stop!");
        if (loopToggle == null)
            Debug.LogError("ParameterModulator: Brak przypisanego toggle'a Loop!");
        if (activeToggle == null)
            Debug.LogError("ParameterModulator: Brak przypisanego toggle'a Active!");
    }

    private void SetupUIListeners()
    {
        if (startStopButton != null)
            startStopButton.onClick.AddListener(ToggleModulation);
        if (loopToggle != null)
            loopToggle.onValueChanged.AddListener(SetLooping);
        if (activeToggle != null)
            activeToggle.onValueChanged.AddListener(SetActive);
    }

    private void Start()
    {
        originalValue = GetParameterValue();
    }

    private void Update()
    {
        if (!isRunning || !isActive) return;

        UpdateModulation();
        UpdateUI();
    }

    private void UpdateModulation()
    {
        float beatLength = 60f / targetVCO.bpm;
        float barLength = beatLength * 4f;
        
        totalDuration = barLength * GetDurationMultiplier();
        currentTime += Time.deltaTime;

        if (currentTime >= totalDuration)
        {
            if (isLooping)
            {
                currentTime = 0f;
            }
            else
            {
                StopModulation();
                return;
            }
        }

        float normalizedTime = currentTime / totalDuration;
        float curveValue = modulationCurve.Evaluate(normalizedTime);

        if (parameterType == ParameterType.OctaveShift)
        {
            // Dla modulacji oktaw używamy wartości z krzywej bezpośrednio
            // Zaokrąglamy do najbliższej liczby całkowitej
            int octaveShift = Mathf.RoundToInt(curveValue * 8 - 4); // Zakres od -4 do +4 oktaw
            targetVCO.globalOctaveShift = octaveShift;
        }
        else
        {
            // Standardowa modulacja dla pozostałych parametrów
            float modulatedValue = Mathf.Lerp(minValue, maxValue, curveValue);
            SetParameterValue(modulatedValue);
        }
    }

    private void UpdateUI()
    {
        if (progressIndicator != null)
        {
            float progress = currentTime / totalDuration;
            progressIndicator.fillAmount = progress;
        }
    }

    private float GetDurationMultiplier()
    {
        switch (durationType)
        {
            case DurationType.QuarterBar: return 0.25f;
            case DurationType.HalfBar: return 0.5f;
            case DurationType.OneBar: return 1f;
            case DurationType.TwoBars: return 2f;
            case DurationType.FourBars: return 4f;
            default: return 1f;
        }
    }

    private float GetParameterValue()
    {
        switch (parameterType)
        {
            case ParameterType.Cutoff:
                return targetVCO.baseCutoff;
            case ParameterType.Resonance:
                return targetVCO.resonance;
            case ParameterType.ChorusAmount:
                return targetVCO.chorusAmount;
            case ParameterType.ChorusRate:
                return targetVCO.chorusRate;
            case ParameterType.ChorusDepth:
                return targetVCO.chorusDepth;
            case ParameterType.ChorusFeedback:
                return targetVCO.chorusFeedback;
            case ParameterType.DelayAmount:
                return targetVCO.delayAmount;
            case ParameterType.DelayFeedback:
                return targetVCO.delayFeedback;
            case ParameterType.DelayWidth:
                return targetVCO.delayWidth;
            case ParameterType.AttackTime:
                return targetVCO.attackTime;
            case ParameterType.DecayTime:
                return targetVCO.decayTime;
            case ParameterType.SustainLevel:
                return targetVCO.sustainLevel;
            case ParameterType.ReleaseTime:
                return targetVCO.releaseTime;
            case ParameterType.OctaveShift:
                return targetVCO.globalOctaveShift;
            default:
                return 0f;
        }
    }

    private void SetParameterValue(float value)
    {
        switch (parameterType)
        {
            case ParameterType.Cutoff:
                targetVCO.baseCutoff = value;
                break;
            case ParameterType.Resonance:
                targetVCO.resonance = value;
                break;
            case ParameterType.ChorusAmount:
                targetVCO.chorusAmount = value;
                break;
            case ParameterType.ChorusRate:
                targetVCO.chorusRate = value;
                break;
            case ParameterType.ChorusDepth:
                targetVCO.chorusDepth = value;
                break;
            case ParameterType.ChorusFeedback:
                targetVCO.chorusFeedback = value;
                break;
            case ParameterType.DelayAmount:
                targetVCO.delayAmount = value;
                break;
            case ParameterType.DelayFeedback:
                targetVCO.delayFeedback = value;
                break;
            case ParameterType.DelayWidth:
                targetVCO.delayWidth = value;
                break;
            case ParameterType.AttackTime:
                targetVCO.attackTime = value;
                break;
            case ParameterType.DecayTime:
                targetVCO.decayTime = value;
                break;
            case ParameterType.SustainLevel:
                targetVCO.sustainLevel = value;
                break;
            case ParameterType.ReleaseTime:
                targetVCO.releaseTime = value;
                break;
            case ParameterType.OctaveShift:
                targetVCO.globalOctaveShift = Mathf.RoundToInt(value);
                break;
        }
    }

    public void StartModulation()
    {
        if (!isActive) return;
        
        isRunning = true;
        currentTime = 0f;
        
        if (startStopButton != null)
        {
            // Możemy dodać zmianę tekstu/ikony przycisku
        }
    }

    public void StopModulation()
    {
        isRunning = false;
        SetParameterValue(originalValue);
        
        if (startStopButton != null)
        {
            // Możemy dodać zmianę tekstu/ikony przycisku
        }
    }

    public void ToggleModulation()
    {
        if (isRunning)
            StopModulation();
        else
            StartModulation();
    }

    public void SetLooping(bool value)
    {
        isLooping = value;
    }

    public void SetActive(bool value)
    {
        isActive = value;
        if (!value && isRunning)
        {
            StopModulation();
        }
    }

    private void OnDisable()
    {
        if (isRunning)
        {
            StopModulation();
        }
    }
} 