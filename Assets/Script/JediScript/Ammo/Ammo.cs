using UnityEngine;

public class Ammo : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private bool destroyOnPickup = true;

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
            bool reloaded = shooter.TryReload();
            if (reloaded && destroyOnPickup)
            {
                Destroy(gameObject);
            }
        }
    }
}
