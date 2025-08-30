using UnityEngine;

public class Ammo : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private bool destroyOnPickup = true;
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private float pickupVolume = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
        {
            return;
        }

        Shoot shooter = other.GetComponentInChildren<Shoot>();
        if (shooter == null)
        {
            shooter = other.GetComponent<Shoot>();
        }

        if (shooter != null)
        {
            shooter.AddReserveAmmo(shooter.DefaultPickupAmount);
            if (pickupClip != null)
            {
                AudioSource.PlayClipAtPoint(pickupClip, transform.position, pickupVolume);
            }
            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
        }
    }
}
