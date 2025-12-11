using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

[RequireComponent(typeof(MoleAnimationController))]
public class Mole : MonoBehaviour
{
    public bool isExplosive;

    [Header("Mole Ball (Drop Item)")]
    public GameObject moleBall;

    [Header("References")]
    public WorldVariable worldVariable;

    [Header("Blink / Emission Settings")]
    public Color blinkColor = Color.red;
    public float baseEmissionIntensity = 0.5f;   // dim state
    public float warningEmissionIntensity = 3f;  // bright state
    public float explosionTime = 10.0f;

    [Header("Audio")]
    public AudioClip Explosion;
    public AudioClip HitSound;

    [Header("VFX")]
    public GameObject explodeEffect;

    // Private references
    private MoleAnimationController animationController;
    private float timer = 0f;
    private bool isBlinking = false;
    private bool alreadyHit = false;

    void Start()
    {
        animationController = GetComponent<MoleAnimationController>();

        // Wait for the animation controller to spawn the FBX model,
        // then start blinking if needed
        StartCoroutine(InitAndMaybeStartBlinking());

        if (worldVariable == null)
        {
            worldVariable = FindAnyObjectByType<WorldVariable>();
        }
    }

    IEnumerator InitAndMaybeStartBlinking()
    {
        // Give MoleAnimationController.Start() a frame to run and spawn the first FBX
        yield return null;

        // Only explosive moles blink
        if (isExplosive && !isBlinking)
        {
            StartBlinking();
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (!isExplosive)
        {
            // Non-explosive moles just despawn after a while
            if (timer > 30.0f)
            {
                Destroy(gameObject);
            }
        }
        // Explosive moles: BlinkRoutine controls when they explode
    }

    void StartBlinking()
    {
        isBlinking = true;
        StartCoroutine(BlinkRoutine());
    }

    IEnumerator BlinkRoutine()
    {
        float baseSpeed = 2f;
        float maxSpeed = 8f;
        bool bright = false;

        // Runs until Explode() destroys this GameObject
        while (true)
        {
            // Check explosion timing first so we blink right up until death
            if (isExplosive && timer >= explosionTime)
            {
                Explode();
                yield break; // stop coroutine; object is being destroyed
            }

            // Always ask the animation controller for the CURRENT model,
            // because it may swap from "appear" FBX to "idle" FBX.
            Renderer moleRenderer = animationController.GetModelRenderer();
            if (moleRenderer != null)
            {
                // Get the current material instance used for rendering
                Material moleMaterial = moleRenderer.material;

                if (moleMaterial.HasProperty("_EmissionColor"))
                {
                    moleMaterial.EnableKeyword("_EMISSION");

                    // Compute blink speed based on how close we are to explosion
                    float blinkSpeed;
                    if (explosionTime <= 0.0001f)
                    {
                        blinkSpeed = maxSpeed;
                    }
                    else
                    {
                        float t = Mathf.Clamp01(timer / explosionTime);
                        blinkSpeed = Mathf.Lerp(baseSpeed, maxSpeed, t);
                    }

                    // Toggle emission between dim and bright
                    if (bright)
                    {
                        moleMaterial.SetColor("_EmissionColor", blinkColor * baseEmissionIntensity);
                    }
                    else
                    {
                        moleMaterial.SetColor("_EmissionColor", blinkColor * warningEmissionIntensity);
                    }

                    bright = !bright;

                    // Blink timing
                    yield return new WaitForSeconds(0.5f / blinkSpeed);
                }
                else
                {
                    // No emission on this material; just wait a frame and try again
                    yield return null;
                }
            }
            else
            {
                // No current model yet (or between swaps) – wait a frame and try again
                yield return null;
            }
        }
    }

    public void OnHit()
    {
        // prevent from being called multiple times in one frame
        if (alreadyHit)
            return;
        alreadyHit = true;

        if (HitSound != null)
        {
            AudioSource.PlayClipAtPoint(HitSound, transform.position, 1f);
        }

        if (moleBall != null)
        {
            Instantiate(moleBall, transform.position + Vector3.up, transform.rotation);
        }

        if (worldVariable.tutorialStage == 1 || worldVariable.tutorialStage == 2)
        {
            worldVariable.tutorialRoomClear = true;
        }

        Destroy(gameObject);
    }

    public void Explode()
    {
        // prevent from being called multiple times in one frame
        if (alreadyHit)
            return;
        alreadyHit = true;

        // Damage player
        if (worldVariable != null)
        {
            worldVariable.playerHealth -= 1;
        }

        // Play explosion sound
        if (Explosion != null)
        {
            AudioSource.PlayClipAtPoint(Explosion, transform.position, 1f);
        }

        // Spawn explosion VFX
        if (explodeEffect != null)
        {
            Instantiate(explodeEffect, transform.position, Quaternion.identity);
        }

        if (!worldVariable.tutorialRoom2Exploded)
        {
            worldVariable.tutorialRoom2Exploded = true;
        }
        
        Destroy(gameObject);
    }
}
