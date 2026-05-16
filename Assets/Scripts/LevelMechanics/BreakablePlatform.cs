using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class BreakablePlatform : MonoBehaviour
{
    [Header("Platform Timing Settings")]
    [Tooltip("Total time (in seconds) the player must stand on the platform before it breaks.")]
    public float breakDelay = 7f;

    [Tooltip("Time (in seconds) before the platform is disabled after falling (for memory/performance optimization).")]
    public float disableDelayAfterFall = 3f;

    [Header("Camera Shake Settings")]
    [Tooltip("Reference to the viewmodel (arms) camera. Auto-assigned if left empty.")]
    public Transform viewmodelCamera;

    [Tooltip("Shake magnitude for the viewmodel camera (recommended: 0.1f).")]
    public float armCameraShakeMagnitude = 0.1f;

    [Space(10)] [Tooltip("Reference to the main player camera. Auto-assigned if left empty.")]
    public Transform playerCamera;

    [Tooltip("Shake magnitude for the main player camera (recommended: 0.05f to 0.06f to prevent motion sickness).")]
    public float playerCameraShakeMagnitude = 0.06f;

    [Header("Slow-Motion Settings")]
    [Tooltip("Duration (in seconds) before breaking when the slow-motion effect begins.")]
    public float slowMoDuration = 1.5f;

    [Tooltip("Time scale during slow-motion (e.g., 1.0 = Normal Speed, 0.3 = 30% Speed).")]
    public float slowMoScale = 0.3f;

    [Header("Audio & Post-Processing Effects")]
    [Tooltip("AudioSource for the metal creaking sound (should be attached to the platform).")]
    public AudioSource metalCreakAudio;

    [Tooltip("AudioLowPassFilter component on the player's camera. Auto-assigned if left empty.")]
    public AudioLowPassFilter lowPassFilter;

    [Tooltip("Post-processing Volume for environmental effects. Auto-assigned if left empty.")]
    public Volume doomsdayVolume;

    private bool isPlayerOnPlatform = false;
    public bool hasSnapped = false;
    private Rigidbody rb;
    public static BreakablePlatform currentActivePlatform;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 originalViewmodelPos;
    private Vector3 originalPlayerCamPos;
    private Coroutine breakCoroutine;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = true;

        startPosition = transform.position;
        startRotation = transform.rotation;


        if (playerCamera == null)
        {
            GameObject pCamObj = GameObject.Find("PlayerCamera");
            if (pCamObj != null)
            {
                playerCamera = pCamObj.transform;
                if (lowPassFilter == null) lowPassFilter = pCamObj.GetComponent<AudioLowPassFilter>();
            }
            else
            {
                Debug.LogError(
                    "[BreakablePlatform] PlayerCamera reference is missing. Ensure the object exists in the scene.");
            }
        }

        if (viewmodelCamera == null)
        {
            GameObject vCamObj = GameObject.Find("ArmCamera");
            if (vCamObj != null) viewmodelCamera = vCamObj.transform;
        }

        if (doomsdayVolume == null)
        {
            GameObject volObj = GameObject.Find("ShakeEffect");
            if (volObj != null)
            {
                doomsdayVolume = volObj.GetComponent<Volume>();
            }
            else
            {
                Debug.LogError("[BreakablePlatform] 'ShakeEffect' Volume component not found in the scene.");
            }
        }


        if (viewmodelCamera != null) originalViewmodelPos = viewmodelCamera.localPosition;
        if (playerCamera != null) originalPlayerCamPos = playerCamera.localPosition;

        ResetCinematicEffects();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isPlayerOnPlatform)
        {
            if (currentActivePlatform != null && currentActivePlatform.hasSnapped) return;

            isPlayerOnPlatform = true;
            currentActivePlatform = this;
            breakCoroutine = StartCoroutine(BreakRoutine());
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && isPlayerOnPlatform)
        {
            isPlayerOnPlatform = false;


            if (!hasSnapped)
            {
                if (breakCoroutine != null) StopCoroutine(breakCoroutine);
                ResetCinematicEffects();
            }
        }
    }

    private IEnumerator BreakRoutine()
    {
        float timer = 0f;

        if (metalCreakAudio != null)
        {
            metalCreakAudio.pitch = 1f;
            metalCreakAudio.Play();
        }


        while (timer < breakDelay)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / breakDelay;


            if (progress > 0.5f)
            {
                float intensityMultiplier = (progress - 0.5f) * 2f;
                Vector3 shakeDirection = Random.insideUnitSphere;

                if (viewmodelCamera != null)
                    viewmodelCamera.localPosition = originalViewmodelPos +
                                                    (shakeDirection * armCameraShakeMagnitude * intensityMultiplier);

                if (playerCamera != null)
                    playerCamera.localPosition = originalPlayerCamPos +
                                                 (shakeDirection * playerCameraShakeMagnitude * intensityMultiplier);
            }

            if (doomsdayVolume != null) doomsdayVolume.weight = progress;


            if (breakDelay - timer <= slowMoDuration)
            {
                Time.timeScale = slowMoScale;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;

                if (metalCreakAudio != null)
                    metalCreakAudio.pitch = Mathf.Lerp(metalCreakAudio.pitch, slowMoScale, Time.unscaledDeltaTime * 5f);

                if (lowPassFilter != null)
                    lowPassFilter.cutoffFrequency =
                        Mathf.Lerp(lowPassFilter.cutoffFrequency, 1000f, Time.unscaledDeltaTime * 10f);
            }

            yield return null;
        }


        hasSnapped = true;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        if (metalCreakAudio != null) metalCreakAudio.Stop();
        if (lowPassFilter != null) lowPassFilter.cutoffFrequency = 22000f;
        if (viewmodelCamera != null) viewmodelCamera.localPosition = originalViewmodelPos;
        if (playerCamera != null) playerCamera.localPosition = originalPlayerCamPos;

        rb.isKinematic = false;


        Invoke(nameof(DisablePlatform), disableDelayAfterFall);
    }

    private void ResetCinematicEffects()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        if (viewmodelCamera != null) viewmodelCamera.localPosition = originalViewmodelPos;
        if (playerCamera != null) playerCamera.localPosition = originalPlayerCamPos;
        if (doomsdayVolume != null) doomsdayVolume.weight = 0f;
        if (lowPassFilter != null) lowPassFilter.cutoffFrequency = 22000f;

        if (metalCreakAudio != null)
        {
            metalCreakAudio.Stop();
            metalCreakAudio.pitch = 1f;
        }

        if (!isPlayerOnPlatform) currentActivePlatform = null;
    }

    private void DisablePlatform()
    {
        gameObject.SetActive(false);
    }

    public void ResetPlatformForRogueLike()
    {
        isPlayerOnPlatform = false;
        hasSnapped = false;
        StopAllCoroutines();
        CancelInvoke(nameof(DisablePlatform));

        ResetCinematicEffects();
        transform.position = startPosition;
        transform.rotation = startRotation;
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        gameObject.SetActive(true);
    }
}