using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class NoteForceMovement : MonoBehaviour
{
    [Range(0.1f, 100f)]
    public float minForce = 0.5f;
    [Range(0.1f, 100f)]
    public float maxForce = 2f;
    [Range(0.1f, 100f)]
    public float forceDuration = 1f;

    public VCO vcoSource; // Źródło sygnału VCO
    public bool useAllSteps = false; // Czy używać wszystkich kroków
    public List<int> activeSteps = new List<int> { 0, 3, 7, 11 }; // Kroki, które będą poruszać obiektem
    public bool applyForceAtCenter = true; // Czy aplikować siłę w środku obiektu
    public bool useRandomDirection = true; // Czy używać losowego kierunku siły
    public Vector3 forceDirection = Vector3.forward; // Kierunek siły (używany gdy useRandomDirection = false)

    private Rigidbody rb;
    private float forceTimer = 0f;
    private bool isApplyingForce = false;
    private Vector3 currentForce;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("NoteForceMovement: Brak komponentu Rigidbody!");
            return;
        }

        // Jeśli nie wybrano źródła VCO, szukamy go w scenie
        if (vcoSource == null)
        {
            vcoSource = FindFirstObjectByType<VCO>();
            if (vcoSource == null)
            {
                Debug.LogError("NoteForceMovement: Nie znaleziono źródła VCO w scenie!");
            }
            else
            {
                Debug.LogWarning("NoteForceMovement: Automatycznie znaleziono źródło VCO w scenie");
            }
        }

        // Subskrybuj zdarzenia z VCO
        if (vcoSource != null)
        {
            vcoSource.OnStepChanged += HandleStepChanged;
        }
    }

    void OnDestroy()
    {
        // Odsubskrybuj zdarzenia z VCO
        if (vcoSource != null)
        {
            vcoSource.OnStepChanged -= HandleStepChanged;
        }
    }

    private void HandleStepChanged(float pitch, int stepNumber)
    {
        ApplyForce(pitch, stepNumber);
    }

    public void ApplyForce(float pitch, int stepNumber)
    {
        if (rb == null)
        {
            Debug.LogError("NoteForceMovement: Rigidbody jest null!");
            return;
        }

        // Sprawdź czy ten krok powinien poruszać obiektem
        if (!useAllSteps && !activeSteps.Contains(stepNumber))
        {
            return;
        }

        // Normalizuj pitch do zakresu 0-1 (zakładając, że pitch jest w Hz)
        float normalizedPitch = Mathf.InverseLerp(20f, 2000f, pitch);
        
        // Oblicz siłę na podstawie znormalizowanego pitcha
        float forceMagnitude = Mathf.Lerp(minForce, maxForce, normalizedPitch);
        
        // Wybierz kierunek siły
        Vector3 direction;
        if (useRandomDirection)
        {
            float randomAngle = Random.Range(0f, 360f);
            direction = new Vector3(
                Mathf.Cos(randomAngle * Mathf.Deg2Rad),
                0f,
                Mathf.Sin(randomAngle * Mathf.Deg2Rad)
            );
        }
        else
        {
            direction = forceDirection.normalized;
        }
        
        currentForce = direction * forceMagnitude;
        
        forceTimer = 0f;
        isApplyingForce = true;
    }

    void FixedUpdate()
    {
        if (isApplyingForce && rb != null)
        {
            forceTimer += Time.fixedDeltaTime;
            
            if (forceTimer >= forceDuration)
            {
                isApplyingForce = false;
            }
            else
            {
                // Płynne zmniejszanie siły w czasie
                float forceMultiplier = Mathf.Lerp(1f, 0f, forceTimer / forceDuration);
                
                if (applyForceAtCenter)
                {
                    rb.AddForce(currentForce * forceMultiplier);
                }
                else
                {
                    rb.AddForceAtPosition(currentForce * forceMultiplier, transform.position + Random.onUnitSphere * 0.1f);
                }
            }
        }
    }
} 