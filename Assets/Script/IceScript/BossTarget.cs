using UnityEngine;
using UnityEngine.UI; // Required for UI Image
using UnityEngine.SceneManagement; // Required for scene management (optional fallback)

public class BossTarget : MonoBehaviour
{
    [Header("Hit Points")]
    [SerializeField] private int shotsToDestroy = 20; // Total shots/hits needed to destroy the boss
    private int hitsRemaining;

    [Header("Health Bar")]
    [Tooltip("UI Image representing the boss's health bar (set to Filled type)")]
    [SerializeField] private Image healthBarImage; // Reference to the UI Image for the health bar

    [Header("Death Audio")]
    [SerializeField] private AudioClip[] deathClips;
    [SerializeField] private float volume = 1f;
    [SerializeField] private bool randomizePitch = true;
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.05f;
    [SerializeField] private float spatialBlend = 1f; // 1 = 3D, 0 = 2D
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 25f;

    private static bool isQuitting;
    private WaveSpawner waveSpawner; // Reference to the WaveSpawner for scene transition

    void Awake()
    {
        // Initialize hitsRemaining, ensuring it's at least 1
        hitsRemaining = Mathf.Max(1, shotsToDestroy);

        // Find the WaveSpawner in the scene
        waveSpawner = FindObjectOfType<WaveSpawner>();
        if (waveSpawner == null)
        {
            Debug.LogWarning("WaveSpawner not found in the scene! Boss destruction may not trigger scene change.");
        }

        // Initialize the health bar
        if (healthBarImage != null)
        {
            if (healthBarImage.type != Image.Type.Filled)
            {
                Debug.LogWarning("Health Bar Image is not set to Filled type! Please set it to Filled in the Inspector.");
            }
            healthBarImage.fillAmount = 1f; // Full health
        }
        else
        {
            Debug.LogError("Health Bar Image not assigned to BossTarget! Please assign it in the Inspector.");
        }
    }

    void OnApplicationQuit()
    {
        isQuitting = true;
    }

    void OnDestroy()
    {
        if (isQuitting)
        {
            return;
        }

        // Play death audio if available
        if (deathClips != null && deathClips.Length > 0)
        {
            AudioClip clip = deathClips[Random.Range(0, deathClips.Length)];
            if (clip != null)
            {
                GameObject audioGO = new GameObject("BossDeathAudio");
                audioGO.transform.position = transform.position;
                AudioSource src = audioGO.AddComponent<AudioSource>();
                src.clip = clip;
                src.volume = Mathf.Clamp01(volume);
                src.spatialBlend = Mathf.Clamp01(spatialBlend);
                src.minDistance = Mathf.Max(0.01f, minDistance);
                src.maxDistance = Mathf.Max(src.minDistance + 0.01f, maxDistance);
                if (randomizePitch)
                {
                    src.pitch = Mathf.Clamp(Random.Range(minPitch, maxPitch), 0.1f, 3f);
                }
                src.playOnAwake = false;
                src.loop = false;
                src.rolloffMode = AudioRolloffMode.Linear;
                src.Play();
                Destroy(audioGO, clip.length / Mathf.Max(0.1f, src.pitch) + 0.05f);
            }
        }

        // Trigger scene transition
        if (waveSpawner != null)
        {
            waveSpawner.ChangeToGoodEnd();
        }
        else
        {
            Debug.LogWarning("WaveSpawner not found, attempting direct scene load.");
            SceneManager.LoadScene("GoodEnd");
        }
    }

    // Handle collisions with projectiles
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Assuming projectiles have a "Projectile" tag
        if (collision.gameObject.CompareTag("Projectile"))
        {
            ApplyHit();
            // Optionally destroy the projectile
            Destroy(collision.gameObject);
        }
    }

    // Handle 3D collisions (if using 3D physics)
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            ApplyHit();
            Destroy(collision.gameObject);
        }
    }

    public void ApplyHit(int damage = 1)
    {
        int d = Mathf.Max(1, damage);
        hitsRemaining -= d;
        if (hitsRemaining < 0)
        {
            hitsRemaining = 0;
        }

        // Update the health bar
        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = (float)hitsRemaining / shotsToDestroy;
            Debug.Log($"Health bar updated: fillAmount = {healthBarImage.fillAmount}, Hits remaining: {hitsRemaining}/{shotsToDestroy}");
        }
        else
        {
            Debug.LogError("Cannot update health bar: Health Bar Image is not assigned!");
        }

        Debug.Log($"Boss took {d} hit(s). Hits remaining: {hitsRemaining}/{shotsToDestroy}");

        if (hitsRemaining <= 0)
        {
            Destroy(gameObject);
        }
    }

    // Optional: Keep Update for debug controls only
    void Update()
    {
        // Debug: Simulate a hit by pressing 'H' key for testing
        if (Input.GetKeyDown(KeyCode.H))
        {
            ApplyHit();
            Debug.Log($"Debug: Simulated hit. Hits remaining: {hitsRemaining}/{shotsToDestroy}");
        }

        // Debug: Trigger GoodEnd scene by pressing 'G' key for testing
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (waveSpawner != null)
            {
                waveSpawner.ChangeToGoodEnd();
            }
            else
            {
                Debug.LogWarning("WaveSpawner not found, attempting direct scene load.");
                SceneManager.LoadScene("GoodEnd");
            }
        }
    }
}