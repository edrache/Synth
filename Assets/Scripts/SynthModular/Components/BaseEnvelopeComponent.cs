using UnityEngine;

public abstract class BaseEnvelopeComponent : MonoBehaviour
{
    [Header("Envelope Settings")]
    [SerializeField] protected float attack = 0.1f;
    [SerializeField] protected float decay = 0.1f;
    [SerializeField] protected float sustain = 0.7f;
    [SerializeField] protected float release = 0.3f;
    
    protected float currentLevel = 0f;
    protected float currentTime = 0f;
    protected bool isNoteOn = false;
    protected bool isReleasing = false;

    protected virtual void Start()
    {
        OnValidate();
    }

    public abstract float GetAmplitude(float deltaTime);
    public abstract void NoteOn();
    public abstract void NoteOff();

    public void SetAttack(float newAttack)
    {
        attack = Mathf.Max(0.001f, newAttack);
    }

    public void SetDecay(float newDecay)
    {
        decay = Mathf.Max(0.001f, newDecay);
    }

    public void SetSustain(float newSustain)
    {
        sustain = Mathf.Clamp01(newSustain);
    }

    public void SetRelease(float newRelease)
    {
        release = Mathf.Max(0.001f, newRelease);
    }

    protected virtual void OnValidate()
    {
        attack = Mathf.Max(0.001f, attack);
        decay = Mathf.Max(0.001f, decay);
        release = Mathf.Max(0.001f, release);
        sustain = Mathf.Clamp01(sustain);
    }
} 