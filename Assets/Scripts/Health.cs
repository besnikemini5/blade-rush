using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    [Tooltip("Seconds of invincibility after a hit. The player always gets at least 0.8s.")]
    public float invincibilityTime = 0f;

    public bool IsDead { get; private set; }
    public bool IsInvincible { get { return Time.time < invincibleUntil; } }

    private float invincibleUntil;
    private SpriteRenderer sprite;
    private Color baseColor = Color.white;
    private Coroutine flashRoutine;

    private void Awake()
    {
        currentHealth = maxHealth;
        sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) baseColor = sprite.color;

        // Without a short grace period, an enemy touching the player drains all health at once
        if (CompareTag("Player") && invincibilityTime < 0.8f) invincibilityTime = 0.8f;
    }

    // Returns true if the damage was applied (false while dead or invincible)
    public bool TakeDamage(int amount)
    {
        if (IsDead || amount <= 0 || IsInvincible) return false;

        currentHealth = Mathf.Max(currentHealth - amount, 0);
        invincibleUntil = Time.time + invincibilityTime;

        if (currentHealth == 0)
        {
            Die();
        }
        else
        {
            Sfx.Play(CompareTag("Player") ? "hurt" : "hit", 0.8f);
            Flash();
        }
        return true;
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    // Used by EnemyManager to make enemies tougher in later waves
    public void SetMaxHealth(int value)
    {
        maxHealth = Mathf.Max(1, value);
        currentHealth = maxHealth;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    private void Flash()
    {
        if (sprite == null) return;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        sprite.color = new Color(1f, 0.35f, 0.35f, baseColor.a);
        yield return new WaitForSeconds(0.1f);
        sprite.color = baseColor;

        // Blink while invincible so the player can see it
        while (IsInvincible && !IsDead)
        {
            sprite.enabled = !sprite.enabled;
            yield return new WaitForSeconds(0.08f);
        }
        sprite.enabled = true;
        flashRoutine = null;
    }

    private void Die()
    {
        IsDead = true;
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }
        if (sprite != null)
        {
            sprite.enabled = true;
            sprite.color = baseColor;
        }

        if (CompareTag("Player"))
        {
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null) gameManager.GameOver();
        }
        else if (CompareTag("Enemy"))
        {
            EnemyManager manager = FindFirstObjectByType<EnemyManager>();
            if (manager != null) manager.OnEnemyKilled(gameObject);
            StartCoroutine(EnemyDeathRoutine());
        }
    }

    private IEnumerator EnemyDeathRoutine()
    {
        foreach (Collider2D col in GetComponents<Collider2D>()) col.enabled = false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        // Squash flat, then disappear
        Vector3 start = transform.localScale;
        const float duration = 0.25f;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float k = t / duration;
            transform.localScale = new Vector3(start.x * (1f + k * 0.6f), start.y * (1f - k), start.z);
            yield return null;
        }
        Destroy(gameObject);
    }
}
