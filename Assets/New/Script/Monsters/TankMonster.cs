using UnityEngine;
using System.Collections;

public class TankMonster : Monster
{
    [Header("Tank Settings")]
    public bool onlyDamagedByExplosives = false;

    [Header("Tank Specific Sounds")]
    public AudioClip[] heavyFootstepSounds;

    [Header("Tank Animation")]
    private MonsterAnimationController animationController; // Uses spawn/walk/attack/die
    private bool isAttacking = false;

    protected override void Start()
    {
        monsterType = MonsterType.Tank;

        // Run base Monster startup (find player, worldVariable, audio, etc.)
        base.Start();

        // Initialize animation controller
        animationController = GetComponent<MonsterAnimationController>();
        if (animationController == null)
        {
            animationController = gameObject.AddComponent<MonsterAnimationController>();
        }
    }

    protected override void StartRepeatedSounds()
    {
        // Use tank-specific footstep sounds if provided
        if (heavyFootstepSounds != null && heavyFootstepSounds.Length > 0)
        {
            movementSounds = heavyFootstepSounds;
        }

        base.StartRepeatedSounds();
    }

    void Update()
    {
        if (hp <= 0 || isAttacking) return;

        HandleMovement();
        // Uses Monster.CheckForRingBreach(), which now calls our override of GetVisualCenter()
        CheckForRingBreach();
    }

    protected override void HandleMovement()
    {
        if (playerCenter == null) return;

        Vector3 direction = (playerCenter.position - transform.position).normalized;
        direction.y = 0;

        transform.position += direction * moveSpeed * Time.deltaTime;

        FacePlayer();
    }

    /// <summary>
    /// Override visual center to ignore animationOffset for Tank.
    /// Breach will use the Tank's root transform position.
    /// </summary>
    protected override Vector3 GetVisualCenter()
    {
        return transform.position;
    }

    protected override void BreachRing()
    {
        hasBreached = true;
        isAttacking = true;

        Debug.Log($"{monsterType} breached the ring! Playing attack animation...");

        // Stop movement
        moveSpeed = 0;

        // Play attack animation
        if (animationController != null)
        {
            animationController.PlayAttackAnimation();
        }
        else
        {
            // Fallback: immediate damage and destroy
            ApplyBreachDamage();
            Destroy(gameObject);
            return;
        }

        // Apply damage immediately (or time this with animation if you want)
        ApplyBreachDamage();

        // Destroy after attack animation
        Invoke(nameof(DestroyAfterAttack), 1.5f);
    }

    void DestroyAfterAttack()
    {
        Destroy(gameObject);
    }

    void ApplyBreachDamage()
    {
        if (worldVariable != null)
        {
            worldVariable.playerHealth -= damageOnBreach;
            Debug.Log($"Player health (via WorldVariable): {worldVariable.playerHealth}");
        }
    }

    // Override OnCollisionEnter to handle MoleBall collisions with tank-specific rules
    void OnCollisionEnter(Collision collision)
    {
        MoleBall ball = collision.gameObject.GetComponent<MoleBall>();

        if (ball != null && ball.currentState == MoleBall.BallState.Yeeted)
        {
            HandleBallHit(ball);
        }

        SimpleProjectile proj = collision.gameObject.GetComponent<SimpleProjectile>();

        if (proj != null && proj.isReflected())
        {
            TakeDamage(proj.damageToMonster);
            Destroy(proj.gameObject);
        }
    }

    // Handle ball hits (separate from TutorialDummy's OnCollisionEnter)
    public void HandleBallHit(MoleBall ball)
    {
        if (onlyDamagedByExplosives && !ball.isExplosive)
        {
            StartCoroutine(FlashResist());
            Debug.Log("Tank resisted non-explosive damage!");
        }
        else
        {
            // Apply damage using Monster.TakeDamage (hit sound + flash red)
            TakeDamage(ball.damage);
            Debug.Log($"Tank took {ball.damage} damage (Explosive: {ball.isExplosive})");
        }

        // Destroy the ball after hitting
        Destroy(ball.gameObject);
    }

    private IEnumerator FlashResist()
    {
        if (rend)
        {
            Color original = rend.material.color;
            rend.material.color = Color.gray;
            yield return new WaitForSeconds(0.3f);
            if (rend) rend.material.color = original;
        }
    }

    protected override void Die()
    {
        // Stop movement
        moveSpeed = 0;

        // Play death animation
        if (animationController != null)
        {
            animationController.PlayDeathAnimation();
        }
        else
        {
            // Fallback if no animation controller
            base.Die();
        }
    }
}
