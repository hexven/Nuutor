using UnityEngine;

public class Attack : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target; // assign player transform; if null tries to find by tag

    [Header("Attack Settings")]
    [SerializeField] private float range = 2.0f;
    [SerializeField] private float cooldownSeconds = 1.0f;
    [SerializeField] private int damagePerHit = 10;
    [SerializeField] private float pushForce = 20f;
    [SerializeField] private float pushUpward = 5f;
    [SerializeField] private float pushDuration = 0.45f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] attackClips;
    [SerializeField] private float attackVolume = 1f;
    [SerializeField] private bool randomizePitch = false;
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.05f;

    private float nextAttackTime;

    void Awake()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogWarning("No GameObject with tag 'Player' found in the scene.");
            }
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f;
            }
        }
    }

    void Update()
    {
        if (target == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance <= range && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + cooldownSeconds;
            TryDealDamage();
        }
    }

    private void TryDealDamage()
    {
        if (target == null)
        {
            return;
        }

        Health health = target.GetComponentInParent<Health>();
        if (health != null)
        {
            // Always apply push, even if the player is moving
            health.ApplyDamageWithPush(damagePerHit, transform.position, pushForce, pushUpward, pushDuration, false);
            PlayAttackSound();
        }
    }

    private void PlayAttackSound()
    {
        if (attackClips == null || attackClips.Length == 0 || audioSource == null)
        {
            return;
        }
        AudioClip clip = attackClips[Random.Range(0, attackClips.Length)];
        if (clip == null)
        {
            return;
        }
        float originalPitch = audioSource.pitch;
        if (randomizePitch)
        {
            audioSource.pitch = Mathf.Clamp(Random.Range(minPitch, maxPitch), 0.1f, 3f);
        }
        audioSource.PlayOneShot(clip, attackVolume);
        audioSource.pitch = originalPitch;
    }
}