using UnityEngine;
using TMPro;
using System.Collections; 
using System.Collections.Generic;

public class FishRoomManager : MonoBehaviour
{
    [HideInInspector] public bool isRoomWon = false;

    [Header("UI & Scene References")] 
    private GameObject timerUIObject;
    private TextMeshProUGUI timerText;
    private PlayerHealth playerHealthScript;

    [Header("Room Settings")] 
    public float timeLimit = 60f;

    [Header("Visibility & Interaction Lists")]
    public GameObject[] fishModels;
    public List<Collider> roomInteractables = new List<Collider>();

    [Header("Explosion & Penalty Settings")] 
    public GameObject explosionEffectObj;
    public CanvasGroup bordeauxFadeGroup;

    [Tooltip("Time before the death sequence completes. Recommended: 2.5 or 3.0.")]
    public float explosionWaitTime = 2.5f; 

    [Tooltip("Intensity of the camera shake. Recommended range: 0.1 to 0.5.")]
    public float cameraShakeMagnitude = 0.2f;

    [Header("Audio Settings")] 
    public AudioSource bgAudioSource;
    public AudioSource sfxAudioSource;
    public AudioClip winSound;
    public AudioClip stupidSound;

    private float currentTime;
    private bool isMinigameActive = false;
    private int placedFishCount = 0;

    void Start()
    {
        timerUIObject = GameObject.Find("FishRoomTimer_UI");
        if (FishRoomTimerUI.Instance != null)
        {
            timerUIObject = FishRoomTimerUI.Instance.gameObject;
            timerText = FishRoomTimerUI.Instance.timerText;
        }

        GameObject playerObj = GameObject.Find("richard");
        if (playerObj != null)
        {
            playerHealthScript = playerObj.GetComponent<PlayerHealth>();
        }

        foreach (GameObject fish in fishModels) 
        {
            if (fish != null) fish.SetActive(false);
        }

        foreach (Collider col in roomInteractables) 
        {
            if (col != null) col.enabled = false;
        }

        if (explosionEffectObj != null) explosionEffectObj.SetActive(false);
        if (bordeauxFadeGroup != null) bordeauxFadeGroup.alpha = 0f;
    }

    void Update()
    {
        if (isMinigameActive)
        {
            currentTime -= Time.deltaTime;
            if (timerText != null) timerText.text = currentTime.ToString("F2");

            if (currentTime <= 0f) FailRoom();
        }
    }

    public void SetTimerVisibility(bool state)
    {
        if (timerUIObject != null) timerUIObject.SetActive(state);
    }

    public void StartMinigame()
    {
        if (isMinigameActive) return;

        isMinigameActive = true;
        currentTime = timeLimit;
        placedFishCount = 0;

        foreach (GameObject fish in fishModels) 
        {
            if (fish != null) fish.SetActive(true);
        }

        foreach (Collider col in roomInteractables) 
        {
            if (col != null) col.enabled = true;
        }

        if (bgAudioSource != null) bgAudioSource.Play();
    }

    public void AddFish()
    {
        placedFishCount++;
    }

    public void TryFinishMinigame()
    {
        if (!isMinigameActive) return;

        if (placedFishCount >= 4)
        {
            WinRoom();
        }
        else
        {
            if (sfxAudioSource != null && stupidSound != null) sfxAudioSource.PlayOneShot(stupidSound);
        }
    }

    private void WinRoom()
    {
        isMinigameActive = false;
        isRoomWon = true;

        if (bgAudioSource != null) bgAudioSource.Stop();
        if (sfxAudioSource != null && winSound != null) sfxAudioSource.PlayOneShot(winSound);

        if (timerText != null) timerText.text = "COMPLETED!";
        
        StartCoroutine(HideTimerAfterDelay(2f));
    }

    private void FailRoom()
    {
        if (!isMinigameActive) return;
        isMinigameActive = false;

        if (bgAudioSource != null) bgAudioSource.Stop();
        if (timerText != null) timerText.text = "00.00";

        SetTimerVisibility(false);

        StartCoroutine(ExplosionAndBordeauxFadeRoutine());
    }

    private IEnumerator HideTimerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetTimerVisibility(false);
    }

    private IEnumerator ExplosionAndBordeauxFadeRoutine()
    {
        if (explosionEffectObj != null) explosionEffectObj.SetActive(true);
        if (sfxAudioSource != null && stupidSound != null) sfxAudioSource.PlayOneShot(stupidSound);

        StartCoroutine(CameraShakeRoutine());

        float elapsed = 0f;
        while (elapsed < explosionWaitTime)
        {
            elapsed += Time.deltaTime;

            if (bordeauxFadeGroup != null)
            {
                bordeauxFadeGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / explosionWaitTime);
            }

            yield return null;
        }

        if (bordeauxFadeGroup != null) bordeauxFadeGroup.alpha = 1f;

        if (playerHealthScript != null)
        {
            playerHealthScript.TakeDamage(playerHealthScript.maxHealth);
        }
    }

    private IEnumerator CameraShakeRoutine()
    {
        Transform camTransform = null;
        Transform armCamTransform = null;

        GameObject mainCamObj = GameObject.Find("PlayerCamera");
        if (mainCamObj != null) camTransform = mainCamObj.transform;

        GameObject armCamObj = GameObject.Find("ArmCamera");
        if (armCamObj != null) armCamTransform = armCamObj.transform;

        if (camTransform == null && armCamTransform == null) yield break;

        Vector3 originalPos = camTransform != null ? camTransform.localPosition : Vector3.zero;
        Vector3 originalArmPos = armCamTransform != null ? armCamTransform.localPosition : Vector3.zero;

        float elapsed = 0f;

        while (elapsed < explosionWaitTime)
        {
            float x = Random.Range(-1f, 1f) * cameraShakeMagnitude;
            float y = Random.Range(-1f, 1f) * cameraShakeMagnitude;

            if (camTransform != null)
                camTransform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            if (armCamTransform != null)
                armCamTransform.localPosition = new Vector3(originalArmPos.x + x, originalArmPos.y + y, originalArmPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (camTransform != null) camTransform.localPosition = originalPos;
        if (armCamTransform != null) armCamTransform.localPosition = originalArmPos;
    }
}