using UnityEngine;
using Rewired;

// This script moves the player using Rigidbody and Rewired input, including jumping.
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    // The Rewired player id of this character
    public int playerId = 0;
    private Player player; // The Rewired Player

    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    private Rigidbody rb;

    // Ground check variables
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;

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

        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void Update()
    {
        // Handle jump input in Update for better responsiveness
        if (player.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
} 