using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Shoot : MonoBehaviour
{
    [Header("Weapon Anchor")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Vector3 localPositionOffset = new Vector3(0.35f, -0.3f, 0.6f);
    [SerializeField] private Vector3 localEulerOffset = new Vector3(0f, 0f, 0f);
    [SerializeField] private float followLerp = 20f;
    [SerializeField] private bool matchCameraFOV = true;

    private Camera cameraComponent;

    [Header("Fire & Recoil")]
    [SerializeField] private float recoilKickBack = 0.12f;
    [SerializeField] private float recoilReturnSpeed = 18f;
    [SerializeField] private float recoilKickClamp = 0.2f;
    [SerializeField] private float shakeAmplitude = 0.006f;
    [SerializeField] private float shakeDuration = 0.08f;
    [SerializeField] private float shakeFrequency = 40f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireClip;
    [SerializeField] private float fireVolume = 1f;

    private float recoilZ;
    private float shakeTimeRemaining;

    [Header("Ammo")]
    [SerializeField] private int magazineSize = 6;
    [SerializeField] private int currentAmmo = 6;
    [SerializeField] private TextMeshProUGUI ammoText;

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

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 0f; // 2D sound by default
                audioSource.playOnAwake = false;
            }
        }

        // Auto-create a simple player UI with ammo text if none assigned
        if (ammoText == null)
        {
            CreatePlayerCanvasWithAmmo();
        }
    }

    void Start()
    {
        currentAmmo = Mathf.Clamp(currentAmmo, 0, magazineSize);
        UpdateAmmoUI();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Fire();
        }

        // Smoothly return recoil to neutral
        recoilZ = Mathf.Lerp(recoilZ, 0f, Time.deltaTime * recoilReturnSpeed);
        if (shakeTimeRemaining > 0f)
        {
            shakeTimeRemaining -= Time.deltaTime;
        }
    }

    private void CreatePlayerCanvasWithAmmo()
    {
        if (cameraComponent == null)
        {
            return;
        }

        GameObject canvasGO = new GameObject("PlayerUI_Canvas");
        canvasGO.transform.SetParent(cameraTransform != null ? cameraTransform : transform, false);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cameraComponent;
        canvas.planeDistance = 1f;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 1f;

        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject textGO = new GameObject("AmmoText", typeof(RectTransform));
        textGO.transform.SetParent(canvasGO.transform, false);
        RectTransform rt = textGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-40f, 30f);
        rt.sizeDelta = new Vector2(300f, 80f);

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = $"{currentAmmo}/{magazineSize}";
        tmp.fontSize = 36f;
        tmp.alignment = TextAlignmentOptions.BottomRight;
        tmp.color = Color.white;

        ammoText = tmp;
    }

    void LateUpdate()
    {
        if (cameraTransform == null)
        {
            return;
        }

        // Compute recoil and shake offsets in local camera space
        Vector3 recoilLocalOffset = new Vector3(0f, 0f, recoilZ);
        Vector3 shakeLocalOffset = Vector3.zero;
        if (shakeTimeRemaining > 0f)
        {
            float t = Time.time * shakeFrequency;
            float nx = (Mathf.PerlinNoise(t, 0.137f) - 0.5f) * 2f;
            float ny = (Mathf.PerlinNoise(0.137f, t) - 0.5f) * 2f;
            shakeLocalOffset = new Vector3(nx, ny, 0f) * shakeAmplitude;
        }

        // Target pose relative to camera (so the gun stays aligned with the crosshair)
        Vector3 targetPosition = cameraTransform.TransformPoint(localPositionOffset + recoilLocalOffset + shakeLocalOffset);
        Quaternion targetRotation = cameraTransform.rotation * Quaternion.Euler(localEulerOffset);

        // Smoothly move and rotate to reduce jitter
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followLerp);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * followLerp);
    }

    private void Fire()
    {
        if (currentAmmo <= 0)
        {
            return;
        }

        currentAmmo--;
        UpdateAmmoUI();

        // Apply instant kick back (clamped)
        recoilZ = Mathf.Max(recoilZ - recoilKickBack, -recoilKickClamp);
        shakeTimeRemaining = shakeDuration;

        if (audioSource != null && fireClip != null)
        {
            audioSource.PlayOneShot(fireClip, fireVolume);
        }
    }

    public bool TryReload()
    {
        if (currentAmmo == magazineSize)
        {
            return false;
        }
        currentAmmo = magazineSize;
        UpdateAmmoUI();
        return true;
    }

    private void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = $"{currentAmmo}/{magazineSize}";
        }
    }
}
