using UnityEngine;
using Rewired;

// This script moves the player using Rigidbody and Rewired input.
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    // The Rewired player id of this character
    public int playerId = 0;
    private Player player; // The Rewired Player

    public float moveSpeed = 5f;
    private Rigidbody rb;

    private void Awake()
    {
        // Get the Rewired Player object for this player
        player = ReInput.players.GetPlayer(playerId);
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // Get input from Rewired
        float moveHorizontal = player.GetAxis("MoveHorizontal");
        float moveVertical = player.GetAxis("MoveVertical");

        // Create movement vector
        Vector3 movement = new Vector3(moveHorizontal, 0, moveVertical);

        // Move the player using Rigidbody
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
} 