using UnityEngine;

// This script draws a laser beam from the player in the direction of the aim target, with an option for fixed length or to reach the aim target, and fading with distance.
[RequireComponent(typeof(LineRenderer))]
public class LaserBeamController : MonoBehaviour
{
    public Transform aimTarget; // Assign the Aim object in the inspector
    public bool useFixedLength = true; // If true, laser has fixed length; if false, ends at aimTarget
    public float laserLength = 10f; // Fixed length of the laser
    public Color laserColor = Color.red; // Base color of the laser

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
    }

    private void Update()
    {
        if (aimTarget == null) return;

        Vector3 start = transform.position;
        Vector3 end;
        float distance;

        if (useFixedLength)
        {
            // Calculate direction from player to aim and set fixed length
            Vector3 direction = (aimTarget.position - start).normalized;
            end = start + direction * laserLength;
            distance = laserLength;
        }
        else
        {
            // End at aim target
            end = aimTarget.position;
            distance = Vector3.Distance(start, end);
        }

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        // Fade based on distance (full alpha at start, 0 at end)
        Color startColor = new Color(laserColor.r, laserColor.g, laserColor.b, 1f);
        Color endColor = new Color(laserColor.r, laserColor.g, laserColor.b, 0f);
        lineRenderer.startColor = startColor;
        lineRenderer.endColor = endColor;
    }
} 