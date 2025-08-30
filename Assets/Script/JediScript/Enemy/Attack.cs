using UnityEngine;

public class Attack : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target; // assign player transform; if null tries Camera.main

    [Header("Attack Settings")]
    [SerializeField] private float range = 2.0f;
    [SerializeField] private float cooldownSeconds = 1.0f;
    [SerializeField] private int damagePerHit = 10;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] attackClips;
    [SerializeField] private float attackVolume = 1f;
    [SerializeField] private bool randomizePitch = false;
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.05f;

    [Header("Animation")]
    [SerializeField] private Animator animator; // อ้างอิง Animator Component
    [SerializeField] private string attackTriggerName = "Attack"; // ชื่อ Trigger Parameter ใน Animator

    private float nextAttackTime;

    void Awake()
    {
        if (target == null && Camera.main != null)
        {
            target = Camera.main.transform;
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

        // Auto-assign Animator if not set
        if (animator == null)
        {
            animator = GetComponent<Animator>();
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
            health.ApplyDamage(damagePerHit, transform.position);
            PlayAttackSound();
            PlayAttackAnimation(); // เพิ่มการเรียกใช้ animation
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

    // เพิ่มฟังก์ชันใหม่สำหรับเล่น animation
    private void PlayAttackAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(attackTriggerName))
        {
            animator.SetTrigger(attackTriggerName);
        }
    }
}