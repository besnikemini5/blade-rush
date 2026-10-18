using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public float moveSpeed = 2f;

    private Transform player;
    private Health playerHealth;
    private Health health;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private float stunnedUntil;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        sprite = GetComponent<SpriteRenderer>();
        if (rb != null) rb.freezeRotation = true;
    }

    // Briefly stop chasing, so a knockback from the player's sword isn't cancelled immediately
    public void Stun(float seconds)
    {
        stunnedUntil = Time.time + seconds;
    }

    private void FixedUpdate()
    {
        if (rb == null || (health != null && health.IsDead)) return;
        if (player == null)
        {
            FindPlayer();
            if (player == null) return;
        }
        if (Time.time < stunnedUntil) return;

        // Walk along the ground towards the player (gravity handles the vertical part)
        float dx = player.position.x - transform.position.x;
        bool playerAlive = playerHealth == null || !playerHealth.IsDead;
        float direction = playerAlive && Mathf.Abs(dx) > 0.1f ? Mathf.Sign(dx) : 0f;
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        if (sprite != null && direction != 0f) sprite.flipX = direction < 0f;
    }

    private void FindPlayer()
    {
        GameObject found = GameObject.FindGameObjectWithTag("Player");
        if (found == null) return;
        player = found.transform;
        playerHealth = found.GetComponent<Health>();
    }
}
