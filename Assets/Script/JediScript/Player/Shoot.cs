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
    [Header("Fire Rate")]
    [SerializeField] private float fireCooldown = 1f;
    private float nextFireTime;

    [Header("Reload FX")]
    [SerializeField] private AudioClip reloadClip;
    [SerializeField] private float reloadVolume = 1f;
    [SerializeField] private float reloadSpinDuration = 0.25f; // seconds
    [SerializeField] private int reloadSpinRevolutions = 1;    // 1 = 360°
    private float reloadSpinTimeRemaining;
    private float reloadSpinAngle;
    private int reloadSpinDirection = 1; // +1 or -1 from mouse Y

    [Header("Custom Aim Rotation")]
    [SerializeField] private bool enableAimRotation = false;
    [SerializeField] private Vector3 aimRotationOffsetEuler = Vector3.zero; // applied after base + reload spin

    private float recoilZ;
    private float shakeTimeRemaining;

    [Header("Ammo")]
    [SerializeField] private int magazineSize = 12;
    [SerializeField] private int currentAmmo = 6;
    [SerializeField] private int reserveAmmo = 1000;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private int pickupAmount = 6;

    [Header("Targeting")]
    [SerializeField] private float shootRange = 100f;
    [SerializeField] private LayerMask shootLayers3D = ~0;
    [SerializeField] private LayerMask shootLayers2D = ~0;
    [SerializeField] private bool includeTriggerColliders3D = true;
    [SerializeField] private float hitSphereRadius = 0.1f;

    private Transform ownerRoot;

    void Awake()
    {
        ownerRoot = transform.root;
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
        reserveAmmo = Mathf.Max(0, reserveAmmo);
        UpdateAmmoUI();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Fire();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            TryReload();
        }

        // Smoothly return recoil to neutral
        recoilZ = Mathf.Lerp(recoilZ, 0f, Time.deltaTime * recoilReturnSpeed);
        if (shakeTimeRemaining > 0f)
        {
            shakeTimeRemaining -= Time.deltaTime;
        }

        // Update reload spin progress
        if (reloadSpinTimeRemaining > 0f)
        {
            reloadSpinTimeRemaining -= Time.deltaTime;
            float progress = 1f - Mathf.Clamp01(reloadSpinTimeRemaining / Mathf.Max(0.0001f, reloadSpinDuration));
            reloadSpinAngle = progress * (reloadSpinRevolutions * 360f);
            if (reloadSpinTimeRemaining <= 0f)
            {
                reloadSpinAngle = 0f;
            }
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
        tmp.text = $"{currentAmmo}/{reserveAmmo}";
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
        if (reloadSpinAngle != 0f)
        {
            // Apply a consistent local-axis tilt (weapon's local right axis)
            float signedAngle = reloadSpinAngle * reloadSpinDirection;
            targetRotation *= Quaternion.AngleAxis(signedAngle, Vector3.right);
        }

        // Optional custom rotation offset for aiming
        if (enableAimRotation)
        {
            targetRotation *= Quaternion.Euler(aimRotationOffsetEuler);
        }

        // Smoothly move and rotate to reduce jitter
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followLerp);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * followLerp);
    }

    private void Fire()
    {
        if (Time.time < nextFireTime)
        {
            return;
        }
        if (currentAmmo <= 0)
        {
            return;
        }

        currentAmmo--;
        currentAmmo = Mathf.Max(0, currentAmmo);
        UpdateAmmoUI();

        // Apply instant kick back (clamped)
        recoilZ = Mathf.Max(recoilZ - recoilKickBack, -recoilKickClamp);
        shakeTimeRemaining = shakeDuration;

        if (audioSource != null && fireClip != null)
        {
            audioSource.PlayOneShot(fireClip, fireVolume);
        }

        // Raycast from camera forward to hit targets under the crosshair (3D and 2D)
        if (cameraTransform != null)
        {
            // 3D hits (nearest-first), skip self, allow hitting through non-target colliders
            Ray ray3D = new Ray(cameraTransform.position, cameraTransform.forward);
            var qti = includeTriggerColliders3D ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;
            RaycastHit[] hits3D = Physics.RaycastAll(ray3D, shootRange, shootLayers3D, qti);
            if (hits3D != null && hits3D.Length > 0)
            {
                System.Array.Sort(hits3D, (a, b) => a.distance.CompareTo(b.distance));
                for (int i = 0; i < hits3D.Length; i++)
                {
                    Collider col3D = hits3D[i].collider;
                    if (col3D == null) continue;
                    if (col3D.transform.root == ownerRoot) continue; // skip self/gun/player

                    Target target3D = col3D.GetComponentInParent<Target>();
                    if (target3D != null)
                    {
                        Destroy(target3D.gameObject);
                        return;
                    }

                    SpriteRenderer sr3D = col3D.GetComponentInParent<SpriteRenderer>();
                    if (sr3D != null)
                    {
                        Destroy(sr3D.gameObject);
                        return;
                    }
                }
            }
            else if (hitSphereRadius > 0f)
            {
                // Fallback: small sphere cast to be forgiving during fast turns
                RaycastHit[] sphereHits = Physics.SphereCastAll(ray3D, hitSphereRadius, shootRange, shootLayers3D, qti);
                if (sphereHits != null && sphereHits.Length > 0)
                {
                    System.Array.Sort(sphereHits, (a, b) => a.distance.CompareTo(b.distance));
                    for (int i = 0; i < sphereHits.Length; i++)
                    {
                        Collider col3D = sphereHits[i].collider;
                        if (col3D == null) continue;
                        if (col3D.transform.root == ownerRoot) continue;

                        Target target3D = col3D.GetComponentInParent<Target>();
                        if (target3D != null)
                        {
                            Destroy(target3D.gameObject);
                            return;
                        }

                        SpriteRenderer sr3D = col3D.GetComponentInParent<SpriteRenderer>();
                        if (sr3D != null)
                        {
                            Destroy(sr3D.gameObject);
                            return;
                        }
                    }
                }
            }

            // 2D hit (supports stacked SpriteRenderers with 2D colliders)
            Camera cam = cameraComponent != null ? cameraComponent : Camera.main;
            if (cam != null)
            {
                Ray ray2D = cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
                RaycastHit2D[] hits2D = Physics2D.GetRayIntersectionAll(ray2D, shootRange, shootLayers2D);
                if (hits2D != null && hits2D.Length > 0)
                {
                    // Iterate nearest first
                    System.Array.Sort(hits2D, (a, b) => a.distance.CompareTo(b.distance));
                    for (int i = 0; i < hits2D.Length; i++)
                    {
                        Collider2D col = hits2D[i].collider;
                        if (col == null) continue;
                        if (col.transform.root == ownerRoot) continue; // skip self

                        Target target2D = col.GetComponentInParent<Target>();
                        if (target2D != null)
                        {
                            Destroy(target2D.gameObject);
                            return;
                        }

                        SpriteRenderer sr2D = col.GetComponentInParent<SpriteRenderer>();
                        if (sr2D != null)
                        {
                            Destroy(sr2D.gameObject);
                            return;
                        }

                        // Fallback: destroy the collider's GameObject if no SpriteRenderer is found
                        Destroy(col.gameObject);
                        return;
                    }
                }
                else if (hitSphereRadius > 0f)
                {
                    // 2D fallback: small circle cast along the ray
                    Vector3 origin = ray2D.origin;
                    Vector3 dir = ray2D.direction.normalized;
                    RaycastHit2D[] circleHits = Physics2D.CircleCastAll(origin, hitSphereRadius, dir, shootRange, shootLayers2D);
                    if (circleHits != null && circleHits.Length > 0)
                    {
                        System.Array.Sort(circleHits, (a, b) => a.distance.CompareTo(b.distance));
                        for (int i = 0; i < circleHits.Length; i++)
                        {
                            Collider2D col = circleHits[i].collider;
                            if (col == null) continue;
                            if (col.transform.root == ownerRoot) continue;

                            Target target2D = col.GetComponentInParent<Target>();
                            if (target2D != null)
                            {
                                Destroy(target2D.gameObject);
                                return;
                            }

                            SpriteRenderer sr2D = col.GetComponentInParent<SpriteRenderer>();
                            if (sr2D != null)
                            {
                                Destroy(sr2D.gameObject);
                                return;
                            }

                            Destroy(col.gameObject);
                            return;
                        }
                    }
                }
            }
        }

        nextFireTime = Time.time + fireCooldown;
    }

    public bool TryReload()
    {
        if (currentAmmo == magazineSize || reserveAmmo <= 0)
        {
            return false;
        }
        int needed = magazineSize - currentAmmo;
        int toLoad = Mathf.Min(needed, reserveAmmo);
        currentAmmo += toLoad;
        reserveAmmo -= toLoad;
        UpdateAmmoUI();
        if (audioSource != null && reloadClip != null)
        {
            audioSource.PlayOneShot(reloadClip, reloadVolume);
        }
        reloadSpinTimeRemaining = reloadSpinDuration;
        reloadSpinAngle = 0f;
        // Use a constant spin direction for consistent rotation
        reloadSpinDirection = 1;
        return true;
    }

    private void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = $"{currentAmmo}/{reserveAmmo}";
        }
    }

    public int DefaultPickupAmount => pickupAmount;
    public void AddReserveAmmo(int amount)
    {
        if (amount <= 0) return;
        reserveAmmo += amount;
        UpdateAmmoUI();
    }
}
