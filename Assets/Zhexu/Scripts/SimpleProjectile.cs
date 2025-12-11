using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class SimpleProjectile : MonoBehaviour
{
    public float speed = 5f;
    public float lifetime = 5f;

    [Header("Damage")]
    public int damageToPlayer = 1;
    public int damageToMonster = 1;

    private WorldVariable worldVariable;
    private Rigidbody rb;

    private enum ProjectileState
    {
        FromEnemy,
        Reflected
    }

    private ProjectileState state = ProjectileState.FromEnemy;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;

        // Make sure collider is trigger so ToolHitDetector's OnTriggerEnter works
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void Start()
    {
        // Auto destroy after lifetime
        Destroy(gameObject, lifetime);

        // Global vars
        worldVariable = FindAnyObjectByType<WorldVariable>();

        // Aim at player at spawn
        if (Camera.main != null)
        {
            Vector3 dir = (Camera.main.transform.position - transform.position).normalized;
            rb.linearVelocity = dir * speed;
        }
        else
        {
            // Fallback: just go forward
            rb.linearVelocity = transform.forward * speed;
        }
    }

    // Called by ToolHitDetector when racket hits this projectile
    public void SetReflected(Vector3 newVelocity)
    {
        state = ProjectileState.Reflected;
        rb.linearVelocity = newVelocity;

        // Optional: change color when reflected so it's visually obvious
        //var rend = GetComponentInChildren<Renderer>();
        //if (rend != null) rend.material.color = Color.cyan;
    }

    void OnTriggerEnter(Collider other)
    {
        // REFLECTED PROJECTILE: can hit monsters, not player
        if (state == ProjectileState.Reflected)
        {
            Monster monster = other.GetComponentInParent<Monster>();
            if (monster != null)
            {
                Debug.Log("Reflected projectile hit monster!");
                monster.TakeDamage(damageToMonster);
                Destroy(gameObject);
                return;
            }

            // Hit ground/wall after reflection → just destroy
            if (other.CompareTag("Ground") || other.CompareTag("Untagged"))
            {
                Destroy(gameObject);
                return;
            }
        }
        // ORIGINAL ENEMY PROJECTILE: can hit player
        else // state == FromEnemy
        {
            if (other.CompareTag("Player") || other.CompareTag("MainCamera"))
            {
                Debug.Log("Player hit!");

                if (worldVariable != null)
                {
                    worldVariable.playerHealth -= damageToPlayer;
                    Debug.Log($"Current health: {worldVariable.playerHealth}");
                }

                Destroy(gameObject);
                return;
            }

            // If it hits ground/wall before reflection, just destroy
            if (other.CompareTag("Ground") || other.CompareTag("Untagged"))
            {
                Destroy(gameObject);
                return;
            }
        }
    }

    public bool isReflected()
    {
        return state == ProjectileState.Reflected;
    }
}
