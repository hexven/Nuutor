using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Hit Points")]
    [SerializeField] private int shotsToDestroy = 1;
    private int hitsRemaining;

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

    void Awake()
    {
        hitsRemaining = Mathf.Max(1, shotsToDestroy);
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
        if (deathClips == null || deathClips.Length == 0)
        {
            return;
        }

        AudioClip clip = deathClips[Random.Range(0, deathClips.Length)];
        if (clip == null)
        {
            return;
        }

        GameObject audioGO = new GameObject("TargetDeathAudio");
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

    public void ApplyHit(int damage = 1)
    {
        int d = Mathf.Max(1, damage);
        hitsRemaining -= d;
        if (hitsRemaining <= 0)
        {
            Destroy(gameObject);
        }
    }
}
