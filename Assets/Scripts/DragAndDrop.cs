using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Height above the original position when object is picked up")]
    [SerializeField] private float pickupHeight = 1.0f;
    [Tooltip("How fast the object moves to cursor position")]
    [SerializeField] private float moveSpeed = 10f;
    [Tooltip("Layer mask for raycast")]
    [SerializeField] private LayerMask pickupLayer = -1;
    [Tooltip("Maximum distance for pickup")]
    [SerializeField] private float maxPickupDistance = 10f;

    private Camera mainCamera;
    private GameObject pickedObject;
    private Rigidbody pickedRigidbody;
    private Vector3 originalPosition;
    private float originalY;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found!");
            enabled = false;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryPickupObject();
        }
        else if (Input.GetMouseButtonUp(0) && pickedObject != null)
        {
            DropObject();
        }

        if (pickedObject != null)
        {
            MoveObject();
        }
    }

    private void TryPickupObject()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxPickupDistance, pickupLayer))
        {
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                pickedObject = hit.collider.gameObject;
                pickedRigidbody = rb;
                originalPosition = pickedObject.transform.position;
                originalY = originalPosition.y;

                // Enable kinematic during drag
                pickedRigidbody.isKinematic = true;
                
                Debug.Log($"Picked up: {pickedObject.name}");
            }
        }
    }

    private void MoveObject()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, originalY + pickupHeight, 0));
        float distance;

        if (groundPlane.Raycast(ray, out distance))
        {
            Vector3 targetPoint = ray.GetPoint(distance);
            
            // Smooth movement
            pickedObject.transform.position = Vector3.Lerp(
                pickedObject.transform.position,
                targetPoint,
                moveSpeed * Time.deltaTime
            );
        }
    }

    private void DropObject()
    {
        if (pickedRigidbody != null)
        {
            // Disable kinematic to allow physics
            pickedRigidbody.isKinematic = false;
            
            // Reset velocity to prevent unwanted movement
            pickedRigidbody.linearVelocity = Vector3.zero;
            pickedRigidbody.angularVelocity = Vector3.zero;
            
            Debug.Log($"Dropped: {pickedObject.name}");
        }

        pickedObject = null;
        pickedRigidbody = null;
    }
} 