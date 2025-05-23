using UnityEngine;
using System.Collections.Generic;

public class NoteMovement : MonoBehaviour
{
    [Range(0.1f, 100f)]
    public float minDistance = 0.5f;
    [Range(0.1f, 100f)]
    public float maxDistance = 2f;
    [Range(0.1f, 5f)]
    public float moveDuration = 1f; // Czas w sekundach potrzebny na osiągnięcie celu

    [Range(0f, 1000f)]
    public float maxDesyncOffset = 0f; // Maksymalne przesunięcie w milisekundach (0 do 1000ms)

    public Transform targetObject; // Obiekt do poruszania
    public VCO vcoSource; // Źródło sygnału VCO
    public bool useAllSteps = false; // Czy używać wszystkich kroków
    public List<int> activeSteps = new List<int> { 0, 3, 7, 11 }; // Kroki, które będą poruszać obiektem
    public bool alwaysStartFromInitialPosition = false; // Czy zawsze zaczynać ruch od pozycji początkowej

    private Vector3 targetPosition;
    private Vector3 startPosition;
    private Vector3 currentStartPosition; // Aktualna pozycja startowa dla ruchu
    private float moveTimer = 0f;
    private bool isMoving = false;
    private float desyncTimer = 0f;
    private bool isWaitingForDesync = false;
    private float currentPitch;
    private int currentStepNumber;
    private float currentDesyncOffset;

    void Start()
    {
        // Jeśli nie wybrano obiektu, używamy własnego transform
        if (targetObject == null)
        {
            targetObject = transform;
            Debug.LogWarning("NoteMovement: targetObject nie został ustawiony, używam własnego transform");
        }

        // Jeśli nie wybrano źródła VCO, szukamy go w scenie
        if (vcoSource == null)
        {
            vcoSource = FindFirstObjectByType<VCO>();
            if (vcoSource == null)
            {
                Debug.LogError("NoteMovement: Nie znaleziono źródła VCO w scenie!");
            }
            else
            {
                Debug.LogWarning("NoteMovement: Automatycznie znaleziono źródło VCO w scenie");
            }
        }

        startPosition = targetObject.position;
        currentStartPosition = startPosition;
        targetPosition = startPosition;

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
        currentPitch = pitch;
        currentStepNumber = stepNumber;
        
        if (Mathf.Approximately(maxDesyncOffset, 0f))
        {
            // Bez rozsynchronizowania - ruch natychmiastowy
            MoveToNote(pitch, stepNumber);
        }
        else
        {
            // Wylosuj nowe przesunięcie dla tego kroku
            currentDesyncOffset = Random.Range(-maxDesyncOffset, maxDesyncOffset);
            
            // Rozpocznij odliczanie do ruchu
            isWaitingForDesync = true;
            desyncTimer = 0f;
        }
    }

    public void MoveToNote(float pitch, int stepNumber)
    {
        if (targetObject == null)
        {
            Debug.LogError("NoteMovement: targetObject jest null!");
            return;
        }

        // Sprawdź czy ten krok powinien poruszać obiektem
        if (!useAllSteps && !activeSteps.Contains(stepNumber))
        {
            return;
        }

        // Normalizuj pitch do zakresu 0-1 (zakładając, że pitch jest w Hz)
        float normalizedPitch = Mathf.InverseLerp(20f, 2000f, pitch);
        
        // Oblicz odległość na podstawie znormalizowanego pitcha
        float distance = Mathf.Lerp(minDistance, maxDistance, normalizedPitch);
        
        // Wybierz losowy kierunek
        float randomAngle = Random.Range(0f, 360f);
        Vector3 direction = new Vector3(
            Mathf.Cos(randomAngle * Mathf.Deg2Rad),
            0f,
            Mathf.Sin(randomAngle * Mathf.Deg2Rad)
        );
        
        // Ustaw aktualną pozycję startową
        currentStartPosition = alwaysStartFromInitialPosition ? startPosition : targetObject.position;
        
        // Oblicz nową pozycję docelową
        targetPosition = currentStartPosition + direction * distance;
        
        // Rozpocznij ruch
        moveTimer = 0f;
        isMoving = true;
    }

    void Update()
    {
        if (isWaitingForDesync)
        {
            desyncTimer += Time.deltaTime * 1000f; // Konwertuj na milisekundy
            
            if (desyncTimer >= Mathf.Abs(currentDesyncOffset))
            {
                isWaitingForDesync = false;
                MoveToNote(currentPitch, currentStepNumber);
            }
        }
        
        if (isMoving && targetObject != null)
        {
            moveTimer += Time.deltaTime;
            float t = moveTimer / moveDuration;
            
            if (t >= 1f)
            {
                targetObject.position = targetPosition;
                isMoving = false;
            }
            else
            {
                // Płynny ruch z użyciem SmoothStep
                t = Mathf.SmoothStep(0f, 1f, t);
                targetObject.position = Vector3.Lerp(currentStartPosition, targetPosition, t);
            }
        }
    }
} 