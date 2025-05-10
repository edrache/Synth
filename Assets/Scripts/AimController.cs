using UnityEngine;
using Rewired;

// This script moves the aiming reticle using Rewired input, optionally clamped to a min and max distance from the player.
public class AimController : MonoBehaviour
{
    public int playerId = 0;
    private Player player;
    public Transform playerTransform; // Reference to the player
    public float minDistance = 1f; // Minimal distance from the player
    public float maxDistance = 3f; // Maximum distance from the player
    public float aimSpeed = 5f;
    public bool isIndependent = false; // If true, reticle is not clamped to player

    private Vector3 aimOffset = Vector3.forward; // Start with some offset

    private void Awake()
    {
        player = ReInput.players.GetPlayer(playerId);
        if (!isIndependent && playerTransform == null)
        {
            Debug.LogError("[AimController] Player Transform not assigned!");
        }
        // Initialize aimOffset to minDistance in front of the player
        aimOffset = Vector3.forward * minDistance;
    }

    private void Update()
    {
        // Get input from Rewired
        float aimHorizontal = player.GetAxis("AimHorizontal");
        float aimVertical = player.GetAxis("AimVertical");

        // Update aim offset based on input
        Vector3 input = new Vector3(aimHorizontal, 0, aimVertical);
        aimOffset += input * aimSpeed * Time.deltaTime;

        if (!isIndependent)
        {
            // Clamp the offset to the min and max distance
            float distance = aimOffset.magnitude;
            if (distance > maxDistance)
            {
                aimOffset = aimOffset.normalized * maxDistance;
            }
            else if (distance < minDistance)
            {
                // Prevent the reticle from getting too close to the player
                if (aimOffset != Vector3.zero)
                    aimOffset = aimOffset.normalized * minDistance;
                else
                    aimOffset = Vector3.forward * minDistance; // Default direction if zero
            }
        }

        // Set the position of the reticle
        if (!isIndependent && playerTransform != null)
        {
            transform.position = playerTransform.position + aimOffset;
        }
        else
        {
            transform.position += input * aimSpeed * Time.deltaTime;
        }
    }
} 