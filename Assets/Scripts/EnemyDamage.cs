using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public int damage = 1;
    public float knockbackForce = 6f;

    private Health ownHealth;

    private void Awake()
    {
        ownHealth = GetComponent<Health>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryHit(collision);
    }

    // Keeps hurting while touching; the player's invincibility time spaces out the hits
    private void OnCollisionStay2D(Collision2D collision)
    {
        TryHit(collision);
    }

    private void TryHit(Collision2D collision)
    {
        if (ownHealth != null && ownHealth.IsDead) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        Health playerHealth = collision.gameObject.GetComponent<Health>();
        if (playerHealth == null || !playerHealth.TakeDamage(damage)) return;

        PlayerMovement movement = collision.gameObject.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            float side = Mathf.Sign(collision.transform.position.x - transform.position.x);
            movement.Knockback(new Vector2(side * knockbackForce, knockbackForce * 0.6f));
        }
    }
}
