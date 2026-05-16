using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Inventory Data")] public List<LockType> collectedLocks = new List<LockType>();

    [Header("Permanent Progress")] public List<LockType> completedRooms = new List<LockType>();

    [Header("Game State")] public int rebellionCounter = 0;

    [Header("Master Lock State")]
    [Tooltip("Tracks the global pool of locks successfully submitted to the master door.")]
    public int totalSubmittedLocks = 0;

    [Header("Voiceover Configuration")] public AudioSource voiceoverSource;
    public AudioClip clip_NoLocks;
    public AudioClip clip_Single_1, clip_Single_2, clip_Single_3, clip_Single_4;
    public AudioClip clip_Double_MF, clip_Double_MP;
    public AudioClip clip_Triple, clip_Quad;

    [Header("Core References (Resurrection Setup)")] [Tooltip("Assign the main HUD Canvas (e.g., Healthbar) here.")]
    public GameObject coreHUDCanvas;

    [Tooltip("Assign the Crosshair Canvas here.")]
    public GameObject crosshairCanvas;

    [Tooltip("Assign the ArmCamera (Viewmodel) here.")]
    public GameObject viewmodelCamera;

    [Tooltip("Assign the player's movement controller script here.")]
    public MonoBehaviour playerController;

    [Header("Post-Processing & Audio Resets")]
    [Tooltip("Assign the ShakeEffect (Volume) from CoreScene here to reset screen darkening.")]
    public Volume doomsdayVolume;

    [Tooltip("Assign the PlayerCamera here to reset the AudioLowPassFilter upon death.")]
    public AudioLowPassFilter playerAudioFilter;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        EventManager.OnLockCollected += HandleLockCollected;
    }

    private void OnDisable()
    {
        EventManager.OnLockCollected -= HandleLockCollected;
    }

    private void HandleLockCollected(LockType newLock)
    {
        collectedLocks.Add(newLock);

        if (!completedRooms.Contains(newLock))
        {
            completedRooms.Add(newLock);
        }

        Debug.Log($"[GameManager] Lock added to inventory: {newLock}");
    }

    public bool IsRoomCompleted(LockType roomType)
    {
        return completedRooms.Contains(roomType);
    }

    public void SubmitLocks()
    {
        int lockCount = collectedLocks.Count;

        if (lockCount == 0)
        {
            PlayVoiceover(clip_NoLocks);
            return;
        }

        totalSubmittedLocks += lockCount;

        if (lockCount == 1)
        {
            rebellionCounter++;
            if (rebellionCounter == 1) PlayVoiceover(clip_Single_1);
            else if (rebellionCounter == 2) PlayVoiceover(clip_Single_2);
            else if (rebellionCounter == 3)
            {
                PlayVoiceover(clip_Single_3);
                Debug.Log("[GameManager] MAIN GATE UNLOCKED - GAME WON STATE REACHED!");
            }
        }
        else if (lockCount == 2)
        {
            if (collectedLocks.Contains(LockType.Maze) && collectedLocks.Contains(LockType.Fish))
                PlayVoiceover(clip_Double_MF);
            else if (collectedLocks.Contains(LockType.Maze) && collectedLocks.Contains(LockType.Platform))
                PlayVoiceover(clip_Double_MP);
        }
        else if (lockCount == 3)
        {
            PlayVoiceover(clip_Triple);
        }

        collectedLocks.Clear();
    }

    private void PlayVoiceover(AudioClip clip)
    {
        if (clip != null && voiceoverSource != null)
        {
            voiceoverSource.clip = clip;
            voiceoverSource.Play();
        }
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(0);
    }

    public void ResetGameDataOnDeath()
    {
        collectedLocks.Clear();
        totalSubmittedLocks = 0;
        completedRooms.Clear();
        rebellionCounter = 0;

        if (coreHUDCanvas != null) coreHUDCanvas.SetActive(true);
        if (crosshairCanvas != null) crosshairCanvas.SetActive(true);
        if (viewmodelCamera != null) viewmodelCamera.SetActive(true);

        if (playerController != null)
        {
            playerController.enabled = true;
            PlayerHealth healthScript = playerController.GetComponent<PlayerHealth>();
            if (healthScript != null && healthScript.healthTextUI != null)
            {
                healthScript.healthTextUI.gameObject.SetActive(true);
            }
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        if (doomsdayVolume != null) doomsdayVolume.weight = 0f;
        if (playerAudioFilter != null) playerAudioFilter.cutoffFrequency = 22000f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log(
            "[GameManager] Player death sequence handled. Inventory, UI, time scale, and A/V effects successfully reset.");
    }

    public void StartTeleportSequence(Transform player, Transform destination, Image whiteScreen, float fadeSpeed)
    {
        StartCoroutine(TeleportRoutine(player, destination, whiteScreen, fadeSpeed));
    }

    private IEnumerator TeleportRoutine(Transform player, Transform destination, Image whiteScreen, float fadeSpeed)
    {
        if (whiteScreen != null)
        {
            Color screenColor = whiteScreen.color;
            while (screenColor.a < 1f)
            {
                screenColor.a += Time.deltaTime * fadeSpeed;
                whiteScreen.color = screenColor;
                yield return null;
            }
        }

        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null) playerRb.linearVelocity = Vector3.zero;

        player.position = destination.position;
        player.rotation = destination.rotation;
        player.eulerAngles = new Vector3(0f, player.eulerAngles.y, 0f);

        string targetSceneName = destination.gameObject.scene.name;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene activeScene = SceneManager.GetSceneAt(i);
            if (activeScene.name != "CoreScene" && activeScene.name != targetSceneName)
            {
                SceneManager.UnloadSceneAsync(activeScene);
                Debug.Log($"[Memory Management] Scene '{activeScene.name}' unloaded to optimize memory usage.");
            }
        }

        Resources.UnloadUnusedAssets();

        yield return new WaitForSeconds(0.2f);

        if (whiteScreen != null)
        {
            Color screenColor = whiteScreen.color;
            while (screenColor.a > 0f)
            {
                screenColor.a -= Time.deltaTime * fadeSpeed;
                whiteScreen.color = screenColor;
                yield return null;
            }
        }
    }

    public int GetTotalSubmittedLocks()
    {
        return totalSubmittedLocks;
    }
}