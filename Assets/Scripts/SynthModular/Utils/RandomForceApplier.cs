using UnityEngine;

/// <summary>
/// Applies a random force to the attached Rigidbody in a random direction within a specified angle and force range.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class RandomForceApplier : MonoBehaviour
{
    [Header("Force Settings")]
    [Tooltip("Minimalna siła")]
    public float minForce = 5f;
    [Tooltip("Maksymalna siła")]
    public float maxForce = 10f;

    [Header("Kąt losowania kierunku (w stopniach, wokół osi Y)")]
    [Tooltip("Minimalny kąt (w stopniach)")]
    public float minAngle = 0f;
    [Tooltip("Maksymalny kąt (w stopniach)")]
    public float maxAngle = 360f;

    [Header("Czy siła ma być przyłożona natychmiast (Impulse)?")]
    public ForceMode forceMode = ForceMode.Impulse;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Applies a random force in a random direction within the specified angle and force range.
    /// </summary>
    public void ApplyRandomForce()
    {
        // Losuj siłę
        float force = Random.Range(minForce, maxForce);

        // Losuj kąt w płaszczyźnie poziomej (Y)
        float angle = Random.Range(minAngle, maxAngle);
        Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

        // Przyłóż siłę
        rb.AddForce(direction * force, forceMode);
    }
} 