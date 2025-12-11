using UnityEngine;
using System.Collections;

public class TutorialDummy : MonoBehaviour
{
    public int hp = 3;
    public bool isShooter = false;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public WorldVariable worldVar;

    [Header("Shooting Settings")]
    public float shootInterval = 3f;

    [Header("Sound Settings")]
    public AudioClip shootSound;  // Add this for shooting sound
    protected AudioSource audioSource;  // Add this

    protected Renderer rend;
    protected Color originalColor;

    protected virtual void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend) originalColor = rend.material.color;

        // Initialize AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f; // 3D sound
            audioSource.minDistance = 1.0f;
            audioSource.maxDistance = 50.0f;
        }

        if (isShooter) StartCoroutine(ShootRoutine());
    }

    void OnCollisionEnter(Collision collision)
    {
        MoleBall ball = collision.gameObject.GetComponent<MoleBall>();

        if (ball != null && ball.currentState == MoleBall.BallState.Yeeted)
        {
            // Both dummies and monsters take damage here.
            // Monsters can still customize behavior by overriding TakeDamage.
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


    public virtual void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log($"Dummy Hit! HP Left: {hp}");
        StartCoroutine(FlashRed());

        if (hp <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (worldVar != null) worldVar.tutorialRoomClear = true;
        Debug.Log("Dummy Defeated!");
        gameObject.SetActive(false);
    }

    protected IEnumerator FlashRed()
    {
        if (rend) rend.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        if (rend) rend.material.color = originalColor;
    }

    protected virtual IEnumerator ShootRoutine()
    {
        while (hp > 0)
        {
            yield return new WaitForSeconds(shootInterval);
            Shoot();
        }
    }

    // New: Separate method for shooting that can be overridden
    protected virtual void Shoot()
    {
        if (projectilePrefab && firePoint)
        {
            // Play shooting sound if available
            if (shootSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(shootSound);
            }

            Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        }
    }
}