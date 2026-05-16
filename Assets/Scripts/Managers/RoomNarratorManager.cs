using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;


[System.Serializable]
public struct RoomNarration
{
    public string roomSceneName;
    public AudioClip narrationClip;
}

public class RoomNarratorManager : MonoBehaviour
{
    public static RoomNarratorManager Instance;

    [Header("Core System Settings")] public AudioSource narratorSource;

    [Tooltip("The UI button used to skip the current narration.")]
    public GameObject skipButtonObject;

    [Header("Room Audio Configurations")] public List<RoomNarration> roomNarrations;

    private string currentRoom = "";
    private bool wasPlayingBeforePause = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void Update()
    {
        if (skipButtonObject != null && skipButtonObject.activeSelf)
        {
            if (!narratorSource.isPlaying && !wasPlayingBeforePause)
            {
                skipButtonObject.SetActive(false);
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (var narration in roomNarrations)
        {
            if (narration.roomSceneName == scene.name)
            {
                currentRoom = scene.name;
                PlayNarration(narration.narrationClip);
                break;
            }
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.name == currentRoom)
        {
            StopNarration();
            currentRoom = "";
        }
    }


    private void PlayNarration(AudioClip clip)
    {
        narratorSource.Stop();
        narratorSource.clip = clip;
        narratorSource.time = 0f; // Reset timeline
        narratorSource.Play();

        if (skipButtonObject != null) skipButtonObject.SetActive(true);
    }

    public void SkipNarration()
    {
        StopNarration();
    }

    private void StopNarration()
    {
        narratorSource.Stop();
        if (skipButtonObject != null) skipButtonObject.SetActive(false);
    }


    public void HandleGamePause(bool isPaused)
    {
        if (isPaused)
        {
            if (narratorSource.isPlaying)
            {
                wasPlayingBeforePause = true;
                narratorSource.Pause();
            }
        }
        else
        {
            if (wasPlayingBeforePause)
            {
                narratorSource.time = Mathf.Max(0f, narratorSource.time - 5f);
                narratorSource.Play();
                wasPlayingBeforePause = false;
            }
        }
    }
}