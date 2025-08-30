using UnityEngine;

// Attach to a medkit pickup with a Trigger Collider
public class Medkit : MonoBehaviour
{
    [SerializeField] private int healAmount = 50;
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private float volume = 1f;
    [SerializeField] private float destroyDelay = 0.02f;

    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
        {
            return;
        }
        Health health = other.GetComponentInParent<Health>();
        if (health == null)
        {
            health = other.GetComponent<Health>();
        }
        if (health == null)
        {
            return;
        }
        bool healed = health.Heal(healAmount);
        if (healed)
        {
            if (pickupClip != null)
            {
                AudioSource.PlayClipAtPoint(pickupClip, transform.position, volume);
            }
            Destroy(gameObject, destroyDelay);
        }
    }
}
