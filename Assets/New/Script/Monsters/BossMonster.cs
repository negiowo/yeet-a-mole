using UnityEngine;
using System.Collections;

public class BossMonster : Monster
{
    public AudioClip specialPatternWarning;

    [Header("Boss Animation")]
    private MonsterAnimationController animationController; // Uses spawn/walk/attack/die
    private LevelWaveManager waveManager;
    private MoleSpawner[] moleHoles;

    private bool isAttacking = false;
    private int bossPhase = 0;

    protected override void Start()
    {
        monsterType = MonsterType.Boss;

        base.Start();

        // Initialize animation controller
        animationController = GetComponent<MonsterAnimationController>();
        if (animationController == null)
        {
            animationController = gameObject.AddComponent<MonsterAnimationController>();
        }

        // Fetch the wave manager for mob spawning
        waveManager = FindAnyObjectByType<LevelWaveManager>();

        // Fetch the mole spawners for mole spawning
        moleHoles = FindObjectsByType<MoleSpawner>(FindObjectsSortMode.None);
    }

    void Update()
    {
        if (hp <= 0 || isAttacking) return;

        // Switch on Boss phases for special patterns
        switch (bossPhase)
        {
            case 0:
                if (hp <= 6)
                {
                    isAttacking = true;

                    // play special pattern warning sound
                    AudioSource.PlayClipAtPoint(specialPatternWarning, Camera.main.transform.position, 1.5f);

                    // play attack animation and set isAttacking back to false
                    StartCoroutine(AttackThenFinish());

                    BossPatternMachinegunSpit();
                    Debug.Log($"{monsterType} attacked with machine gun, entering phase 1.");
                    bossPhase = 1;
                }
                break;
            case 1:
                if (hp <= 4)
                {
                    isAttacking = true;

                    // play special pattern warning sound
                    AudioSource.PlayClipAtPoint(specialPatternWarning, Camera.main.transform.position, 1.5f);

                    // play attack animation and set isAttacking back to false
                    StartCoroutine(AttackThenFinish());

                    BossPatternSummonMonster();
                    Debug.Log($"{monsterType} summoned more monsters, entering phase 2.");
                    bossPhase = 2;
                }
                break;
            case 2:
                if (hp <= 2)
                {
                    isAttacking = true;

                    // play special pattern warning sound
                    AudioSource.PlayClipAtPoint(specialPatternWarning, Camera.main.transform.position, 1.5f);

                    // play attack animation and set isAttacking back to false
                    StartCoroutine(AttackThenFinish());

                    BossPatternSummonMole();
                    Debug.Log($"{monsterType} summoned many explosive moles, entering phase 3.");
                    bossPhase = 3;
                }
                break;
        }

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
            return;
        }

        // Apply damage immediately (or time this with animation if you want)
        ApplyBreachDamage();
    }

    void ApplyBreachDamage()
    {
        if (worldVariable != null)
        {
            worldVariable.playerHealth -= damageOnBreach; // Breach from boss will be an instant kill
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
        TakeDamage(ball.damage);
        Destroy(ball.gameObject);
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
    private void BossPatternMachinegunSpit()
    {
        // shoots towards player 5 times, with an interval of 0.5s
        StartCoroutine(ShootRepeatedly(5, 0.5f));
    }

    IEnumerator ShootRepeatedly(int times, float interval)
    {
        for (int i = 0; i < times; i++)
        {
            Shoot();
            yield return new WaitForSeconds(interval);
        }
    }

    private void BossPatternSummonMonster()
    {
        // Summon 1 walker, 1 tank, and 1 spitter to random location on the outer ring
        waveManager.bossSummon(1, 1, 1);
    }

    private void BossPatternSummonMole()
    {
        foreach (var hole in moleHoles)
        {
            hole.bossSummonExplosive();
        }
    }

    IEnumerator AttackThenFinish()
    {
        if (animationController != null)
        {
            animationController.PlayAttackAnimation();
            yield return new WaitForSeconds(animationController.attackAnimationLength);
            animationController.PlayWalkAnimation();
        }

        isAttacking = false;
    }
}
