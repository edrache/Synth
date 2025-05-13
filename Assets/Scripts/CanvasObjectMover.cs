using UnityEngine;
using Rewired;

// This script moves a UI object on canvas using Rewired input
[RequireComponent(typeof(RectTransform))]
public class CanvasObjectMover : MonoBehaviour
{
    public int playerId = 0;
    public float moveSpeed = 100f;
    public bool clampToScreen = true;
    
    [Header("Button Movement")]
    public bool useButtonMovement = false;
    public float buttonMoveStep = 10f;

    private Player player;
    private RectTransform rectTransform;
    private Canvas canvas;

    private void Awake()
    {
        // Get the Rewired Player object for this player
        player = ReInput.players.GetPlayer(playerId);
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        if (!ReInput.isReady) return;

        Vector2 movement = Vector2.zero;

        if (useButtonMovement)
        {
            // Button-based movement
            if (player.GetButtonDown("MoveRight"))
                movement.x += buttonMoveStep;
            if (player.GetButtonDown("MoveLeft"))
                movement.x -= buttonMoveStep;
            if (player.GetButtonDown("MoveUp"))
                movement.y += buttonMoveStep;
            if (player.GetButtonDown("MoveDown"))
                movement.y -= buttonMoveStep;
        }
        else
        {
            // Analog movement
            float moveHorizontal = player.GetAxis("MoveHorizontal");
            float moveVertical = player.GetAxis("MoveVertical");
            movement = new Vector2(moveHorizontal, moveVertical) * moveSpeed * Time.deltaTime;
        }
        
        // Apply movement to RectTransform
        rectTransform.anchoredPosition += movement;

        // Clamp position to screen if enabled
        if (clampToScreen)
        {
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);
            Vector2 canvasSize = canvas.GetComponent<RectTransform>().sizeDelta;
            Vector2 objectSize = rectTransform.sizeDelta;
            
            // Calculate bounds
            float minX = objectSize.x / 2;
            float maxX = canvasSize.x - objectSize.x / 2;
            float minY = objectSize.y / 2;
            float maxY = canvasSize.y - objectSize.y / 2;

            // Clamp position
            Vector2 clampedPosition = rectTransform.anchoredPosition;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
            rectTransform.anchoredPosition = clampedPosition;
        }
    }
} 