using UnityEngine;
using System.Collections;

public class CannonInteractable : BaseInteractable
{
    [Header("Firing Settings")]
    [Tooltip("The point from which the cannonball is instantiated.")]
    public Transform firePoint; 
    
    [Tooltip("The cannonball prefab to spawn.")]
    public GameObject cannonballPrefab; 
    
    [Tooltip("The velocity applied to the cannonball upon firing.")]
    public float fireForce = 150f; 
    
    [Tooltip("Cooldown duration between shots to prevent spamming.")]
    public float cooldown = 3f;

    [Header("VFX & SFX")]
    [Tooltip("Muzzle flash particle effect triggered on fire.")]
    public ParticleSystem muzzleFlashVFX; 
    
    [Tooltip("Audio source and clip for the firing sound.")]
    public AudioSource audioSource;
    public AudioClip cannonBoomSound;

    private bool isReadyToFire = true;

    protected override void Interact()
    {
        if (!isReadyToFire)
        {
            Debug.LogWarning("[Cannon] Cannon is on cooldown. Firing denied.");
            return;
        }
        
        StartCoroutine(FireRoutine());
    }

    private IEnumerator FireRoutine()
    {
        isReadyToFire = false;
        Debug.Log("[Cannon] Firing sequence initiated.");
        
        if (muzzleFlashVFX != null) muzzleFlashVFX.Play();
        if (audioSource != null && cannonBoomSound != null) audioSource.PlayOneShot(cannonBoomSound);
        
        if (cannonballPrefab != null && firePoint != null)
        {
            GameObject ball = Instantiate(cannonballPrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = ball.GetComponentInChildren<Rigidbody>();
            
            if (rb != null)
            {
                rb.linearVelocity = firePoint.forward * fireForce; 
                Debug.Log($"[Cannon] Projectile fired with velocity: {fireForce}");
            }
            else
            {
                Debug.LogError("[Cannon] Rigidbody not found on the spawned cannonball.");
            }
            
            // Cleanup projectile to optimize memory usage
            Destroy(ball, 4f);
        }
        
        yield return new WaitForSeconds(cooldown);
        isReadyToFire = true;
    }
}