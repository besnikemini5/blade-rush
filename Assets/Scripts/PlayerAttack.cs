using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;

    public int damage = 1;
    public float attackCooldown = 0.35f;
    public float knockbackForce = 5f;

    private PlayerMovement movement;
    private Health health;
    private SpriteRenderer slash;
    private Coroutine slashRoutine;
    private float nextAttackTime;
    private float attackOffsetX = 0.5f;

    private static Sprite slashSprite;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        health = GetComponent<Health>();
        if (attackPoint != null && Mathf.Abs(attackPoint.localPosition.x) > 0.01f)
        {
            attackOffsetX = Mathf.Abs(attackPoint.localPosition.x);
        }
        CreateSlashEffect();
    }

    private void Update()
    {
        if ((health != null && health.IsDead) || Time.timeScale == 0f) return;

        // Keep the attack point in front of the player when he turns around
        if (attackPoint != null && movement != null)
        {
            Vector3 p = attackPoint.localPosition;
            attackPoint.localPosition = new Vector3(movement.FacingRight ? attackOffsetX : -attackOffsetX, p.y, p.z);
        }

        if ((Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0)) && Time.time >= nextAttackTime)
        {
            Attack();
        }
    }

    private void Attack()
    {
        nextAttackTime = Time.time + attackCooldown;
        Sfx.Play("swing", 0.6f);
        ShowSlash();

        Vector2 center = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, attackRange, enemyLayers);
        HashSet<Health> alreadyHit = new HashSet<Health>(); // one hit per enemy, even with several colliders

        foreach (Collider2D hit in hits)
        {
            Health enemyHealth = hit.GetComponentInParent<Health>();
            if (enemyHealth == null || !alreadyHit.Add(enemyHealth)) continue;
            if (!enemyHealth.TakeDamage(damage) || enemyHealth.IsDead) continue;

            Rigidbody2D body = hit.attachedRigidbody;
            if (body != null)
            {
                float side = Mathf.Sign(hit.transform.position.x - transform.position.x);
                body.linearVelocity = new Vector2(side * knockbackForce, knockbackForce * 0.4f);
            }
            EnemyFollow follow = hit.GetComponentInParent<EnemyFollow>();
            if (follow != null) follow.Stun(0.25f);
        }
    }

    private void CreateSlashEffect()
    {
        if (slashSprite == null) slashSprite = BuildSlashSprite();

        GameObject go = new GameObject("Slash");
        go.transform.SetParent(transform, false);
        slash = go.AddComponent<SpriteRenderer>();
        slash.sprite = slashSprite;
        SpriteRenderer own = GetComponent<SpriteRenderer>();
        if (own != null)
        {
            slash.sortingLayerID = own.sortingLayerID;
            slash.sortingOrder = own.sortingOrder + 1;
        }
        slash.enabled = false;
    }

    private void ShowSlash()
    {
        if (slash == null) return;
        bool right = movement == null || movement.FacingRight;
        slash.flipX = !right;
        slash.transform.localPosition = new Vector3(right ? attackOffsetX : -attackOffsetX, 0f, 0f);
        if (slashRoutine != null) StopCoroutine(slashRoutine);
        slashRoutine = StartCoroutine(SlashRoutine());
    }

    private IEnumerator SlashRoutine()
    {
        slash.enabled = true;
        const float duration = 0.12f;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            slash.color = new Color(1f, 1f, 1f, 1f - t / duration);
            yield return null;
        }
        slash.enabled = false;
        slashRoutine = null;
    }

    // Draws a small white crescent in code, so the effect needs no image file
    private static Sprite BuildSlashSprite()
    {
        const int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color clear = new Color(0f, 0f, 0f, 0f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dy = y - 15.5f;
                float outer = Mathf.Sqrt((x - 4f) * (x - 4f) + dy * dy);
                float inner = Mathf.Sqrt((x + 1f) * (x + 1f) + dy * dy);
                bool inside = outer < 16f && inner > 15f && x > 6;
                tex.SetPixel(x, y, inside ? new Color(1f, 1f, 1f, outer > 14.5f ? 1f : 0.55f) : clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 24f);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
