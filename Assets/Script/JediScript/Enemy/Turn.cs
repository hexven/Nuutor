using UnityEngine;

// Makes this object face the Player or the main Camera (billboard).
public class Turn : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target; // Set to Player or Camera. If null, uses Camera.main
    [SerializeField] private bool useMainCameraIfTargetNull = true;

    [Header("Rotation Options")]
    [SerializeField] private bool rotateOnlyAroundY = true; // Keep upright, rotate yaw only
    [SerializeField] private bool invertFacing = false; // Face away instead of towards
    [SerializeField] private float rotateLerpSpeed = 0f; // 0 = instant; >0 = smooth slerp

    void Awake()
    {
        EnsureTarget();
    }

    void LateUpdate()
    {
        if (target == null)
        {
            EnsureTarget();
            if (target == null)
            {
                return;
            }
        }

        Vector3 toTarget = target.position - transform.position;
        if (invertFacing)
        {
            toTarget = -toTarget;
        }

        if (rotateOnlyAroundY)
        {
            toTarget.y = 0f;
        }

        if (toTarget.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion desired = Quaternion.LookRotation(toTarget.normalized, Vector3.up);

        if (rotateLerpSpeed > 0f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, desired, Time.deltaTime * rotateLerpSpeed);
        }
        else
        {
            transform.rotation = desired;
        }
    }

    private void EnsureTarget()
    {
        if (target == null && useMainCameraIfTargetNull && Camera.main != null)
        {
            target = Camera.main.transform;
        }
    }
}
