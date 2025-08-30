using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private Transform target; // The player's Transform
    public float speed = 5f; // Movement speed, adjustable in Inspector

    public void SetTarget(Transform player)
    {
        target = player;
    }

    void Update()
    {
        if (target == null)
        {
            Debug.LogWarning("No target set for enemy movement!");
            return;
        }

        // Move toward the player
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Rotate to face the player (2D rotation, assuming a top-down or side view)
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, 0); // Adjust angle if needed (e.g., sprite orientation)
        
    }
 }
}