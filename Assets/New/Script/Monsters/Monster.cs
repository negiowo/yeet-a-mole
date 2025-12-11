using UnityEngine;
using System.Collections;

public class Monster : TutorialDummy
{
    [Header("Monster Movement Settings")]
    public float moveSpeed = 2.0f;
    public int damageOnBreach = 1;
    public float defenseRingRadius = 5f;

    [Header("Monster Type")]
    public MonsterType monsterType;

    [Header("Spawn & Ambient Sounds")]
    public AudioClip spawnSound;

    [Header("Repeated Sounds")]
    public AudioClip[] idleSounds;           // Random idle sounds (growls, etc.)
    public AudioClip[] movementSounds;       // Footstep or movement sounds
    public float minIdleSoundInterval = 3f;
    public float maxIdleSoundInterval = 8f;
    public float movementSoundInterval = 0.5f; // For footsteps
    public bool playMovementSounds = false;

    [Header("Sound Settings")]
    public float soundVolume = 0.7f;
    public float spatialBlend = 1.0f;        // 0 = 2D, 1 = 3D

    [Header("Hit & Death Sounds")]
    public AudioClip hitSound;
    public float hitVolume = 0.9f;
    public AudioClip deathSound;
    public float deathVolume = 1.0f;

    protected bool hasBreached = false;
    protected Transform playerCenter;
    protected WorldVariable worldVariable;
    protected Coroutine idleSoundRoutine;
    protected Coroutine movementSoundRoutine;
    protected bool isMoving = true; // Monsters are always moving toward player

    public enum MonsterType
    {
        Walker,
        Spitter,
        Tank,
        Boss
    }

    protected override void Start()
    {
        // Find WorldVariable for player health
        worldVariable = FindAnyObjectByType<WorldVariable>();

        // Find player center (XR Origin)
        GameObject xrOrigin = GameObject.Find("XR Origin Hands (XR Rig)");
        if (xrOrigin != null)
        {
            playerCenter = xrOrigin.transform;
        }

        base.Start(); // sets up audioSource, rend, etc from TutorialDummy

        // Configure AudioSource for 3D monster sounds
        if (audioSource != null)
        {
            audioSource.spatialBlend = spatialBlend;
            audioSource.volume = soundVolume;
        }

        // Play spawn sound
        PlaySpawnSound();

        // Start repeated sound coroutines
        StartRepeatedSounds();

        Debug.Log($"{monsterType} monster spawned at {transform.position}");
    }

    protected virtual void StartRepeatedSounds()
    {
        // Start idle sounds (random intervals)
        if (idleSounds != null && idleSounds.Length > 0)
        {
            idleSoundRoutine = StartCoroutine(IdleSoundRoutine());
        }

        // Start movement sounds (regular intervals)
        if (playMovementSounds && movementSounds != null && movementSounds.Length > 0)
        {
            movementSoundRoutine = StartCoroutine(MovementSoundRoutine());
        }
    }

    protected virtual IEnumerator IdleSoundRoutine()
    {
        while (hp > 0)
        {
            // Wait random interval
            float waitTime = Random.Range(minIdleSoundInterval, maxIdleSoundInterval);
            yield return new WaitForSeconds(waitTime);

            // Play random idle sound
            PlayRandomSound(idleSounds);
        }
    }

    protected virtual IEnumerator MovementSoundRoutine()
    {
        while (hp > 0)
        {
            // Only play movement sound if actually moving
            if (isMoving && hp > 0)
            {
                PlayRandomSound(movementSounds);
            }

            // Wait for next movement sound
            yield return new WaitForSeconds(movementSoundInterval);
        }
    }

    protected virtual void PlayRandomSound(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0 || audioSource == null || hp <= 0)
            return;

        // Pick random sound
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip != null)
        {
            // Randomize pitch slightly for variety
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clip);
            // Reset pitch
            audioSource.pitch = 1.0f;
        }
    }

    protected virtual void PlaySpawnSound()
    {
        if (spawnSound != null && audioSource != null)
        {
            audioSource.pitch = 1.0f;
            audioSource.PlayOneShot(spawnSound);
        }
    }

    void Update()
    {
        if (hp <= 0) return;

        HandleMovement();
        CheckForRingBreach();
    }

    protected virtual void HandleMovement()
    {
        if (playerCenter == null) return;

        Vector3 direction = (playerCenter.position - transform.position).normalized;
        direction.y = 0;

        transform.position += direction * moveSpeed * Time.deltaTime;

        FacePlayer();

        // Update movement state (can be used to control sound)
        isMoving = direction.magnitude > 0.1f;
    }

    protected void FacePlayer()
    {
        if (playerCenter == null) return;

        Vector3 lookDirection = (playerCenter.position - transform.position).normalized;
        lookDirection.y = 0;

        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    protected virtual void CheckForRingBreach()
    {
        if (hasBreached || playerCenter == null) return;

        // Use visual center (includes animationOffset)
        Vector3 visualCenter = GetVisualCenter();

        Vector2 monsterPos = new Vector2(visualCenter.x, visualCenter.z);
        Vector2 centerPos = new Vector2(playerCenter.position.x, playerCenter.position.z);
        float distanceFromCenter = Vector2.Distance(monsterPos, centerPos);

        if (distanceFromCenter <= defenseRingRadius)
        {
            BreachRing();
        }
    }



    protected virtual void BreachRing()
    {
        hasBreached = true;
        Debug.Log($"{monsterType} breached the ring!");

        if (worldVariable != null)
        {
            worldVariable.playerHealth -= damageOnBreach;
            Debug.Log($"Player health (via WorldVariable): {worldVariable.playerHealth}");
        }
        else
        {
            Debug.LogWarning("WorldVariable not found to damage player!");
        }

        // Stop all sounds before destroying
        StopAllCoroutines();
        Destroy(gameObject);
    }

    // Monsters have their own collision damage from MoleBall
    void OnCollisionEnter(Collision collision)
    {
        MoleBall ball = collision.gameObject.GetComponent<MoleBall>();

        if (ball != null && ball.currentState == MoleBall.BallState.Yeeted)
        {
            TakeDamage(ball.damage);
            Destroy(ball.gameObject);
        }

        SimpleProjectile proj = collision.gameObject.GetComponent<SimpleProjectile>();

        if (proj != null && proj.isReflected())
        {
            TakeDamage(proj.damageToMonster);
            Destroy(proj.gameObject);
        }
    }

    // NEW: common damage behavior for all monsters (hit flash + sound)
    public override void TakeDamage(int damage)
    {
        if (hp <= 0) return;

        hp -= damage;
        Debug.Log($"{monsterType} Hit! HP Left: {hp}");

        // Play hit sound
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound, hitVolume);
        }

        // Flash all renderers red briefly
        StartCoroutine(FlashAllRenderersRed());

        if (hp <= 0)
        {
            hp = 0;
            Die();
        }
    }

    private IEnumerator FlashAllRenderersRed()
    {
        // Grab all renderers that exist *right now*
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers == null || renderers.Length == 0) yield break;

        // Store original colors per renderer/material
        var originalColors = new Color[renderers.Length][];

        // First pass: set everything to red
        for (int r = 0; r < renderers.Length; r++)
        {
            var rend = renderers[r];

            // Renderer may already have been destroyed
            if (rend == null) continue;

            Material[] mats;
            try
            {
                mats = rend.materials;
            }
            catch (MissingReferenceException)
            {
                // Renderer got destroyed while we were iterating; skip it
                continue;
            }

            if (mats == null || mats.Length == 0) continue;

            originalColors[r] = new Color[mats.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;
                originalColors[r][i] = mats[i].color;
                mats[i].color = Color.red;
            }
        }

        yield return new WaitForSeconds(0.1f);

        // If the whole monster got destroyed in the meantime, just bail out
        if (this == null) yield break;

        // Second pass: restore original colors where possible
        for (int r = 0; r < renderers.Length; r++)
        {
            var rend = renderers[r];
            var colorsForRenderer = originalColors[r];

            if (rend == null || colorsForRenderer == null) continue;

            Material[] mats;
            try
            {
                mats = rend.materials;
            }
            catch (MissingReferenceException)
            {
                // Renderer was destroyed between flash and restore; ignore
                continue;
            }

            if (mats == null || mats.Length == 0) continue;

            int count = Mathf.Min(mats.Length, colorsForRenderer.Length);
            for (int i = 0; i < count; i++)
            {
                if (mats[i] == null) continue;
                mats[i].color = colorsForRenderer[i];
            }
        }
    }


    protected override void Die()
    {
        // Play death sound once
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound, deathVolume);
        }

        // Stop sound coroutines
        if (idleSoundRoutine != null)
            StopCoroutine(idleSoundRoutine);
        if (movementSoundRoutine != null)
            StopCoroutine(movementSoundRoutine);

        base.Die(); // by default, this disables the GameObject
    }
    // Returns the "visual" center of this monster, including animation offset
    protected virtual Vector3 GetVisualCenter()
    {
        Vector3 basePos = transform.position;
        Vector3 offset = Vector3.zero;

        // Walker
        var walkerCtrl = GetComponent<WalkerAnimationController>();
        if (walkerCtrl != null)
        {
            offset = walkerCtrl.animationOffset;
        }
        else
        {
            // Generic Tank / Monster animation controller
            var tankCtrl = GetComponent<MonsterAnimationController>();
            if (tankCtrl != null)
            {
                offset = tankCtrl.animationOffset;
            }
            else
            {
                // Spitter
                var spitterCtrl = GetComponent<SpitterAnimationController>();
                if (spitterCtrl != null)
                {
                    offset = spitterCtrl.animationOffset;
                }
            }
        }

        // Only use X/Z for ring distance (Y height doesn't matter)
        return basePos + new Vector3(offset.x, 0f, offset.z);
    }


}
