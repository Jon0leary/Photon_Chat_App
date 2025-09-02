using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    private Collider col;

    void Awake()
    {
        col = GetComponent<Collider>();
        if (col != null) col.enabled = false; // safety: start disabled
    }

    // Called by animation events (OnHitboxEnable / OnHitboxDisable)
    public void EnableHitbox()
    {
        if (col != null) col.enabled = true;
    }

    public void DisableHitbox()
    {
        if (col != null) col.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Example: damage anything that implements IDamageable
        if (other.TryGetComponent<IDamageable>(out var dmg))
        {
            dmg.TakeDamage(damage);
        }
    }
}

// Simple damage interface other scripts can implement
public interface IDamageable
{
    void TakeDamage(int amount);
}
