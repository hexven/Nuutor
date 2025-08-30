using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
        if (currentHealth <= 0)
        {
            return;
        }
        currentHealth = Mathf.Max(0, currentHealth - amount);
        UpdateUI();

        // Optionally skip knockback if player is moving (WASD)
        if (ignoreKnockbackWhileMoving)
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveZ = Input.GetAxisRaw("Vertical");
            if ((new Vector2(moveX, moveZ)).sqrMagnitude > movementInputThreshold * movementInputThreshold)
            {
                return;
            }
        }

        // Knockback away from attacker
        Vector3 away = (transform.position - attackerPosition);
        away.y = 0f;
        if (away.sqrMagnitude > 0.0001f)
        {
            away = away.normalized;
        }
        Vector3 up = Vector3.up * knockbackUpward;
        knockbackVelocity = (away * knockbackForce) + up;
        knockbackTimeRemaining = knockbackDuration;
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
}
