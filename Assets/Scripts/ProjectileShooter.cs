using UnityEngine;
using Rewired;
using UnityEngine.Events;

// This script spawns and shoots a projectile towards the aim target when Fire is pressed or when FireProjectile() is called.
public class ProjectileShooter : MonoBehaviour
{
    public int playerId = 0;
    private Player player;
    public GameObject projectilePrefab; // Assign your projectile prefab in the inspector
    public Transform aimTarget;         // Assign the Aim object in the inspector
    public Transform firePoint;         // Point from which the projectile is spawned (e.g. a child of Player)
    public float shootForce = 20f;      // Force applied to the projectile
    public float shootAngle = 0f;       // Angle in degrees (0 = straight, positive = up, negative = down)

    public UnityEvent OnProjectileFired; // Event invoked when a projectile is fired

    private void Awake()
    {
        player = ReInput.players.GetPlayer(playerId);
    }

    private void Update()
    {
        if (player.GetButtonDown("Fire"))
        {
            FireProjectile();
        }
    }

    // Public method to fire a projectile, can be called from UnityEvents or other scripts
    public void FireProjectile()
    {
        if (projectilePrefab == null || aimTarget == null || firePoint == null) return;

        // Calculate direction from firePoint to aimTarget
        Vector3 direction = (aimTarget.position - firePoint.position).normalized;

        // Apply angle offset (in local space)
        direction = Quaternion.AngleAxis(shootAngle, Vector3.Cross(Vector3.up, direction)) * direction;

        // Instantiate projectile
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));

        // Add force to Rigidbody
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(direction * shootForce, ForceMode.Impulse);
        }

        // Invoke the event
        OnProjectileFired?.Invoke();
    }
} 