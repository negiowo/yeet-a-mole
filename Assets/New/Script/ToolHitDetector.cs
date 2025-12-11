using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using static UnityEditor.PlayerSettings;

public class ToolHitDetector : MonoBehaviour
{
    public enum SwingType
    {
        Any,
        DownOnly,
        UpOnly,
        ForwardOnly
    }

    [Header("Targets")]
    //  Can hit multiple tags (e.g. "Ball" and "enemy_bullet")
    public string[] targetTags;

    [Header("Haptic target")]
    public XRInputHapticImpulseProvider rightControllerHaptic;

    [Header("Input")]
    // Right trigger action (pressed while swinging)
    public InputActionReference triggerAction;

    [Header("Swing")]
    public SwingType swingType = SwingType.Any;
    public float minSwingSpeed = 0.5f;

    [Header("Audio")]
    public AudioClip hitClip;

    private AudioSource audioSource;
    private Vector3 lastPosition;
    private Vector3 velocity; 
    private bool canHit = true;
    public float hitCooldown = 0.5f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        if (triggerAction != null)
        {
            triggerAction.action.Enable();
        }

        lastPosition = transform.position;
    }

    private void OnDisable()
    {
        if (triggerAction != null)
        {
            triggerAction.action.Disable();
        }
    }

    private void Update()
    {
        // Estimate swing velocity based on tool position
        Vector3 currentPos = transform.position;
        velocity = (currentPos - lastPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
        lastPosition = currentPos;
    }

    private bool IsTriggerHeld()
    {
        if (triggerAction == null)
            return false;

        return triggerAction.action.IsPressed();
    }

    private bool IsSwingDirectionValid()
    {
        switch (swingType)
        {
            case SwingType.DownOnly:
                // Negative Y = moving downward
                return velocity.y <= -minSwingSpeed;

            case SwingType.UpOnly:
                // Positive Y = moving upward
                return velocity.y >= minSwingSpeed;

            case SwingType.ForwardOnly:
                // Negative X = moving forward (your current convention)
                return velocity.x <= -minSwingSpeed;

            case SwingType.Any:
            default:
                return true;
        }
    }

    private bool MatchesTargetTags(Collider other)
    {
        if (targetTags == null || targetTags.Length == 0)
            return false;

        foreach (string t in targetTags)
        {
            if (!string.IsNullOrEmpty(t) && other.CompareTag(t))
            {
                return true;
            }
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Must hit a valid target tag (Ball, enemy_bullet, etc.)
        if (!MatchesTargetTags(other))
            return;

        //// Must be holding trigger
        //if (!IsTriggerHeld())
        //    return;

        // Must be swinging in the correct direction
        if (!IsSwingDirectionValid() && swingType != SwingType.ForwardOnly)
            return;

        // Haptics
        if (rightControllerHaptic != null)
        {
            rightControllerHaptic
                .GetChannelGroup()?
                .GetChannel()?
                .SendHapticImpulse(0.7f, 0.15f, 0.0f);
        }

        // Sound
        if (audioSource != null && hitClip != null)
        {
            audioSource.PlayOneShot(hitClip);
        }

        // ------------------------------------------------------------------------------------
        // NON-Forward swings: these are for hitting moles (explode / drop ball)
        // ------------------------------------------------------------------------------------
        if (swingType != SwingType.ForwardOnly)
        {
            Mole m = other.GetComponent<Mole>();
            if (m != null)
            {
                if (m.isExplosive && swingType == SwingType.DownOnly)  // hitting explosive with a bat
                {
                    m.Explode();
                }
                else if (m.isExplosive && swingType == SwingType.UpOnly)  // digging explosive with a shovel
                {
                    m.OnHit();
                }
                else if (!m.isExplosive && swingType == SwingType.DownOnly)  // hitting normal mole with a bat
                {
                    m.OnHit();
                }
            }
        }
        // ------------------------------------------------------------------------------------
        // ForwardOnly: racket physics for MoleBall AND spitter projectiles
        // ------------------------------------------------------------------------------------
        else
        {
            // 0.5s cooldown between each hit
            if (!canHit) return;

            // Get rigidbody on the hit object
            Rigidbody rb = other.attachedRigidbody;
            if (rb == null)
                return;

            // Launch in direction of racket swing
            Vector3 dir = velocity.normalized;
            float launchSpeed = velocity.magnitude * 2.0f;

            rb.linearVelocity = dir * launchSpeed;

            //canHit = false;
            //StartCoroutine(HitCooldown());

            // If it's a MoleBall, set its yeet state (same as before)
            MoleBall moleBall = other.GetComponent<MoleBall>();
            if (moleBall != null)
            {
                moleBall.yeeted();

                // prevents double collision
                //IgnoreFor(other, 2f);
            }

            // If it's a spitter projectile, mark it as reflected and reuse same velocity
            SimpleProjectile proj = other.GetComponent<SimpleProjectile>();
            if (proj != null)
            {
                proj.SetReflected(dir * launchSpeed);

                // prevents double collision
                //IgnoreFor(other, 2f);
            }
        }
    }

    public void IgnoreFor(Collider target, float seconds)
    {
        StartCoroutine(IgnoreRoutine(target, seconds));
    }

    private IEnumerator IgnoreRoutine(Collider ball, float seconds)
    {
        Physics.IgnoreCollision(GetComponent<Collider>(), ball, true);
        yield return new WaitForSeconds(seconds);
        Physics.IgnoreCollision(GetComponent<Collider>(), ball, false);
    }

    IEnumerator HitCooldown()
    {
        yield return new WaitForSeconds(hitCooldown);
        canHit = true;
    }
}
