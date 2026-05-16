using UnityEngine;
using System.Collections;

public class HazardZone : MonoBehaviour
{
    [Header("Hazard Settings")] [Tooltip("The tag of the target object (should be Player).")]
    public string targetTag = "Player";

    [Tooltip("If true, the player dies instantly on contact (e.g., for bottomless pits).")]
    public bool isInstantKill = false;

    [Header("Pulsed Damage Settings")] [Tooltip("How much health to remove per tick? (e.g., 5)")]
    public float damageAmount = 5f;

    [Tooltip("How many seconds between each damage tick? (e.g., 2)")]
    public float damageInterval = 2f;

    [Header("Audio Feedback")] [Tooltip("AudioSource component attached to the hazard zone.")]
    public AudioSource hazardAudioSource;

    [Tooltip("Audio clip played upon applying damage.")]
    public AudioClip acidBurnSound;

    [Header("Camera Shake Settings")] public bool enableCameraShake = true;
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.1f;

    private Coroutine damageCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            if (other.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth))
            {
                if (other.TryGetComponent<FPSController>(out FPSController fpsController))
                {
                    fpsController.walkSpeed *= 0.5f;
                    fpsController.sprintSpeed *= 0.5f;
                }

                if (isInstantKill)
                {
                    playerHealth.TakeDamage(playerHealth.maxHealth);
                }
                else
                {
                    if (damageCoroutine == null)
                    {
                        damageCoroutine = StartCoroutine(PulseDamage(playerHealth));
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            if (other.TryGetComponent<FPSController>(out FPSController fpsController))
            {
                fpsController.walkSpeed *= 2f;
                fpsController.sprintSpeed *= 2f;
            }

            StopDamageRoutine();
        }
    }

    private void OnDisable()
    {
        StopDamageRoutine();
    }

    private void StopDamageRoutine()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }

    private IEnumerator PulseDamage(PlayerHealth playerHealth)
    {
        while (true)
        {
            playerHealth.TakeDamage(damageAmount);

            if (hazardAudioSource != null && acidBurnSound != null)
            {
                hazardAudioSource.PlayOneShot(acidBurnSound);
            }

            if (enableCameraShake && Camera.main != null)
            {
                StartCoroutine(CameraShakeRoutine());
            }

            yield return new WaitForSeconds(damageInterval);
        }
    }

    private IEnumerator CameraShakeRoutine()
    {
        Transform camTransform = Camera.main.transform;
        Vector3 originalPos = camTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            camTransform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        camTransform.localPosition = originalPos;
    }
}