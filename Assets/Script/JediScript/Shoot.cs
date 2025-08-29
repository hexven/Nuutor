using UnityEngine;

public class Shoot : MonoBehaviour
{
    [Header("Weapon Anchor")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Vector3 localPositionOffset = new Vector3(0.35f, -0.3f, 0.6f);
    [SerializeField] private Vector3 localEulerOffset = new Vector3(0f, 0f, 0f);
    [SerializeField] private float followLerp = 20f;
    [SerializeField] private bool matchCameraFOV = true;

    private Camera cameraComponent;

    void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        if (cameraTransform != null)
        {
            cameraComponent = cameraTransform.GetComponent<Camera>();
        }
        if (cameraComponent == null && Camera.main != null)
        {
            cameraComponent = Camera.main;
        }
    }

    void LateUpdate()
    {
        if (cameraTransform == null)
        {
            return;
        }

        // Target pose relative to camera (so the gun stays aligned with the crosshair)
        Vector3 targetPosition = cameraTransform.TransformPoint(localPositionOffset);
        Quaternion targetRotation = cameraTransform.rotation * Quaternion.Euler(localEulerOffset);

        // Smoothly move and rotate to reduce jitter
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followLerp);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * followLerp);
    }
}
