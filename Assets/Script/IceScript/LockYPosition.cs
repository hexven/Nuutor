using UnityEngine;

public class LockYPosition : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The fixed Y position for the enemy sprite")]
    private float lockedYPosition = 0f;

    private Vector3 initialPosition;

    private void Start()
    {
        // Store the initial position
        initialPosition = transform.position;
        // Set initial Y position to the locked value
        transform.position = new Vector3(initialPosition.x, lockedYPosition, initialPosition.z);
    }

    private void Update()
    {
        // Continuously lock the Y position while preserving X and Z
        transform.position = new Vector3(transform.position.x, lockedYPosition, transform.position.z);
    }

    // Optional: Method to change locked Y position at runtime
    public void SetLockedYPosition(float newY)
    {
        lockedYPosition = newY;
        // Update position immediately
        transform.position = new Vector3(transform.position.x, lockedYPosition, transform.position.z);
    }
}