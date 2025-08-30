using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;
    [SerializeField] private float knockbackForce = 20f;
    [SerializeField] private float knockbackUpward = 5f;
    [SerializeField] private float knockbackDuration = 0.45f;
    [SerializeField] private bool ignoreKnockbackWhileMoving = true;
    [SerializeField] private float movementInputThreshold = 0.1f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Slider healthSlider;

    private CharacterController characterController;
    private Vector3 knockbackVelocity;
    private float knockbackTimeRemaining;
    private bool isDead;

    [Header("Death/Scene Transition")]
    [SerializeField] private string deathSceneName = "BadEnd";
    [SerializeField] private AudioClip deathClip;
    [SerializeField] private float deathVolume = 1f;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();
    }

    void Update()
    {
        if (knockbackTimeRemaining > 0f)
        {
            knockbackTimeRemaining -= Time.deltaTime;
            if (characterController != null)
            {
                characterController.Move(knockbackVelocity * Time.deltaTime);
            }
            else
            {
                transform.position += knockbackVelocity * Time.deltaTime;
            }
        }
    }

    public void ApplyDamage(int amount, Vector3 attackerPosition)
    {
        ApplyDamageWithPush(amount, attackerPosition, knockbackForce, knockbackUpward, knockbackDuration, true);
    }

    public void ApplyDamageWithPush(int amount, Vector3 attackerPosition, float pushForce, float pushUpward, float pushDuration, bool respectIgnoreMoving)
    {
        if (currentHealth <= 0)
        {
            return;
        }
        currentHealth = Mathf.Max(0, currentHealth - amount);
        UpdateUI();

        if (!isDead && currentHealth <= 0)
        {
            isDead = true;
            PlayDeathAudioAndLoadScene();
            return;
        }

        if (respectIgnoreMoving && ignoreKnockbackWhileMoving)
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveZ = Input.GetAxisRaw("Vertical");
            if ((new Vector2(moveX, moveZ)).sqrMagnitude > movementInputThreshold * movementInputThreshold)
            {
                return;
            }
        }

        Vector3 away = (transform.position - attackerPosition);
        away.y = 0f;
        if (away.sqrMagnitude > 0.0001f)
        {
            away = away.normalized;
        }
        Vector3 up = Vector3.up * pushUpward;
        knockbackVelocity = (away * pushForce) + up;
        knockbackTimeRemaining = pushDuration;
    }

    private void UpdateUI()
    {
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public bool Heal(int amount)
    {
        if (amount <= 0)
        {
            return false;
        }
        int before = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        if (currentHealth != before)
        {
            UpdateUI();
            return true;
        }
        return false;
    }

    private void PlayDeathAudioAndLoadScene()
    {
        GameObject audioGO = null;
        if (deathClip != null)
        {
            audioGO = new GameObject("DeathAudio");
            DontDestroyOnLoad(audioGO);
            var src = audioGO.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            src.spatialBlend = 0f;
            src.clip = deathClip;
            src.volume = Mathf.Clamp01(deathVolume);
            src.Play();
            Object.Destroy(audioGO, deathClip.length + 0.1f);
        }
        SceneManager.LoadScene(deathSceneName);
    }
}
