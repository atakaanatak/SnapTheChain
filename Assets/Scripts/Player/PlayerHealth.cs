using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Visual Feedback (Impact & Shake)")]
    public Image damageScreenOverlay;

    public RectTransform healthUIContainer;
    public float shakeMagnitude = 10f;

    [Header("Environmental Hazards")]
    [Tooltip("The scene name where the toxic visual effect is allowed to render (e.g., MazeRoom).")]
    public string acidSceneName = "MazeRoom";

    [Header("Health Core Settings")] public float maxHealth = 100f;
    public float CurrentHealth { get; private set; }

    [Header("UI System - Hybrid Display")] public TextMeshProUGUI healthTextUI;
    public Image healthBarImage;
    public Sprite[] healthSprites;

    [Header("Damage Flash Properties")] public Color damageFlashColor = Color.red;
    public Color normalColor = Color.white;
    public float flashDuration = 0.2f;

    [Header("Low Health Threshold (Panic State)")]
    public float lowHealthThreshold = 25f;

    public AudioSource playerAudioSource;
    public AudioClip heartbeatSound;

    [Header("Death and Respawn Settings")] public string mainRoomSpawnPointName = "MainRoomReturnPoint";
    public Image fadeScreen;
    public float fadeSpeed = 2f;

    private bool isDead = false;
    private Coroutine flashCoroutine;
    private Coroutine lowHealthPulseCoroutine;
    private Vector2 originalHealthUIPos;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopAllPanicEffects();
    }

    private void Start()
    {
        CurrentHealth = maxHealth;
        if (healthTextUI != null) healthTextUI.color = normalColor;
        UpdateHealthUI();


        if (fadeScreen == null)
        {
            GameObject fsObj = GameObject.Find("FadeScreen");
            if (fsObj != null) fadeScreen = fsObj.GetComponent<Image>();
        }

        if (healthUIContainer != null)
        {
            originalHealthUIPos = healthUIContainer.anchoredPosition;
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        CurrentHealth -= damageAmount;
        CurrentHealth = Mathf.Max(CurrentHealth, 0f);

        UpdateHealthUI();
        TriggerDamageFlash();

        if (CurrentHealth <= 0f)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthTextUI != null)
        {
            healthTextUI.text = Mathf.CeilToInt(CurrentHealth).ToString();
        }

        if (healthBarImage != null && healthSprites != null && healthSprites.Length > 0)
        {
            float healthPercent = CurrentHealth / maxHealth;
            int index = Mathf.CeilToInt(healthPercent * (healthSprites.Length - 1));
            index = Mathf.Clamp(index, 0, healthSprites.Length - 1);
            healthBarImage.sprite = healthSprites[index];
        }


        if (damageScreenOverlay != null)
        {
            if (CurrentHealth <= lowHealthThreshold && CurrentHealth > 0 && !isDead && IsInAcidScene())
            {
                if (lowHealthPulseCoroutine == null)
                {
                    lowHealthPulseCoroutine = StartCoroutine(LowHealthPulseRoutine());
                }
            }
            else
            {
                StopAllPanicEffects();
            }
        }
    }


    private bool IsInAcidScene()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == acidSceneName)
            {
                return true;
            }
        }

        return false;
    }

    private void StopAllPanicEffects()
    {
        if (lowHealthPulseCoroutine != null)
        {
            StopCoroutine(lowHealthPulseCoroutine);
            lowHealthPulseCoroutine = null;
        }

        if (damageScreenOverlay != null)
        {
            damageScreenOverlay.color = new Color(damageScreenOverlay.color.r, damageScreenOverlay.color.g,
                damageScreenOverlay.color.b, 0f);
        }

        if (playerAudioSource != null) playerAudioSource.Stop();
    }

    private void TriggerDamageFlash()
    {
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator DamageFlashRoutine()
    {
        if (healthTextUI != null) healthTextUI.color = damageFlashColor;
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float fadePercent = 1f - (elapsed / flashDuration);


            if (damageScreenOverlay != null && lowHealthPulseCoroutine == null && IsInAcidScene())
            {
                damageScreenOverlay.color = new Color(0f, 0.5f, 0f, 0.2f * fadePercent);
            }

            if (healthUIContainer != null)
            {
                Vector2 randomShake = Random.insideUnitCircle * (shakeMagnitude * fadePercent);
                healthUIContainer.anchoredPosition = originalHealthUIPos + randomShake;
            }

            yield return null;
        }

        if (healthTextUI != null) healthTextUI.color = normalColor;
        if (healthUIContainer != null) healthUIContainer.anchoredPosition = originalHealthUIPos;


        if (damageScreenOverlay != null && lowHealthPulseCoroutine == null)
            damageScreenOverlay.color = new Color(damageScreenOverlay.color.r, damageScreenOverlay.color.g,
                damageScreenOverlay.color.b, 0f);

        flashCoroutine = null;
    }

    private IEnumerator LowHealthPulseRoutine()
    {
        while (true)
        {
            if (playerAudioSource != null && heartbeatSound != null)
            {
                playerAudioSource.PlayOneShot(heartbeatSound);
            }

            float t = 0;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Lerp(0.1f, 0.4f, t / 0.3f);
                damageScreenOverlay.color = new Color(0f, 0.5f, 0f, alpha);
                yield return null;
            }

            t = 0;
            while (t < 0.7f)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Lerp(0.4f, 0.1f, t / 0.7f);
                damageScreenOverlay.color = new Color(0f, 0.5f, 0f, alpha);
                yield return null;
            }
        }
    }

    private void Die()
    {
        isDead = true;
        StopAllPanicEffects();
        StartCoroutine(DeathAndRespawnRoutine());
    }

    private IEnumerator DeathAndRespawnRoutine()
    {
        if (fadeScreen != null)
        {
            Color c = fadeScreen.color;
            while (c.a < 1f)
            {
                c.a += Time.deltaTime * fadeSpeed;
                fadeScreen.color = c;
                yield return null;
            }
        }


        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name != "CoreScene" && scene.name != "MainRoom")
            {
                SceneManager.UnloadSceneAsync(scene);
            }
        }

        Scene mainScene = SceneManager.GetSceneByName("MainRoom");
        if (!mainScene.isLoaded)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainRoom", LoadSceneMode.Additive);
            while (!asyncLoad.isDone) yield return null;
        }

        GameObject spawnObj = GameObject.Find(mainRoomSpawnPointName);
        if (spawnObj != null)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = Vector3.zero;
            transform.position = spawnObj.transform.position;
            transform.eulerAngles = new Vector3(0f, spawnObj.transform.eulerAngles.y, 0f);
        }

        if (GameManager.Instance != null) GameManager.Instance.ResetGameDataOnDeath();
        if (RoomStateManager.Instance != null) RoomStateManager.Instance.ResetAllRooms();

        CurrentHealth = maxHealth;
        isDead = false;
        UpdateHealthUI();

        yield return new WaitForSeconds(0.5f);
        if (fadeScreen != null)
        {
            Color c = fadeScreen.color;
            while (c.a > 0f)
            {
                c.a -= Time.deltaTime * fadeSpeed;
                fadeScreen.color = c;
                yield return null;
            }
        }
    }
}