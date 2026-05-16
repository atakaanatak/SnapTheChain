using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class DialogueQuestion
{
    [Tooltip("Question ID referenced from the Localization CSV (e.g., bobby_q1)")]
    public string questionID;

    [Tooltip("Answer ID for the first option (Pink/Blue button) from the CSV")]
    public string blueAnswerID;

    [Tooltip("Answer ID for the second option (Green button) from the CSV")]
    public string greenAnswerID;

    [Tooltip("Sticker visual to activate upon answering this question")]
    public GameObject openSticker;
}

public class BobbyDialogManager : BaseInteractable
{
    [Header("Core References (Auto-Assigned if empty)")]
    public GameObject playerHUDCanvas;

    [Tooltip("Reference to the main UI canvas containing health and timer data")]
    public GameObject healthAndTimerCanvas;

    public GameObject viewmodelCameraObj;
    public MonoBehaviour playerController;
    public Transform playerCamera;
    public PlayerHealth playerHealthScript;
    private Rigidbody playerRb;

    [Header("UI & World Elements")] public TextMeshProUGUI questiontext;
    public TextMeshProUGUI pinkbuttontext;
    public TextMeshProUGUI bluebuttontext;
    public GameObject canvasObject;
    public GameObject bloodEffect;
    public Transform canvasPoint;

    [Header("Dialogue Configuration")] public DialogueQuestion[] allQuestions;
    public float cameraTransitionSpeed = 2f;

    [Header("Event Outcomes")] public Animator bobbyAnimator;
    public GameObject hiddenPadlock;

    [Header("Audio Settings")] public AudioSource audioSource;
    public AudioClip generalDeathSound;
    public AudioClip gunSound;

    private Vector3 originalCameraLocalPos;
    private Quaternion originalCameraLocalRot;

    private int currentQuestionIndex = 0;
    private int blueScore = 0;
    private int greenScore = 0;
    private bool isDialogueActive = false;
    private bool isProcessingAnswer = false;

    private void Start()
    {
        LocateCoreDependencies();

        if (hiddenPadlock != null)
        {
            hiddenPadlock.SetActive(false);
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void LocateCoreDependencies()
    {
        if (playerHUDCanvas == null)
        {
            GameObject hud = GameObject.Find("CrossCanvas");
            if (hud != null) playerHUDCanvas = hud;
        }

        if (healthAndTimerCanvas == null)
        {
            healthAndTimerCanvas = GameObject.Find("HealthAndTimerCanvas");
        }

        if (playerController == null)
        {
            GameObject playerObj = GameObject.Find("richard");
            if (playerObj != null)
            {
                playerController = playerObj.GetComponent<FPSController>();
                playerHealthScript = playerObj.GetComponent<PlayerHealth>();
                playerRb = playerObj.GetComponent<Rigidbody>();
            }
        }

        if (playerCamera == null)
        {
            GameObject camObj = GameObject.Find("PlayerCamera");
            if (camObj != null) playerCamera = camObj.transform;
        }

        if (viewmodelCameraObj == null)
        {
            GameObject armCamObj = GameObject.Find("ArmCamera");
            if (armCamObj != null) viewmodelCameraObj = armCamObj;
        }
    }

    protected override void Interact()
    {
        if (!isDialogueActive)
        {
            StartDialogueSequence();
        }
    }

    public void StartDialogueSequence()
    {
        isDialogueActive = true;
        isProcessingAnswer = false;
        currentQuestionIndex = 0;
        blueScore = 0;
        greenScore = 0;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerController != null) playerController.enabled = false;

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.isKinematic = true;
        }


        if (playerHUDCanvas != null) playerHUDCanvas.SetActive(false);
        if (viewmodelCameraObj != null) viewmodelCameraObj.SetActive(false);
        if (healthAndTimerCanvas != null) healthAndTimerCanvas.SetActive(false);
        if (bloodEffect != null) bloodEffect.SetActive(false);

        StartCoroutine(TransitionCameraToCanvas());
    }

    private IEnumerator TransitionCameraToCanvas()
    {
        originalCameraLocalPos = playerCamera.localPosition;
        originalCameraLocalRot = playerCamera.localRotation;

        float transitionProgress = 0f;
        Vector3 startPos = playerCamera.position;
        Quaternion startRot = playerCamera.rotation;

        while (transitionProgress < 1f)
        {
            transitionProgress += Time.deltaTime * cameraTransitionSpeed;
            playerCamera.position = Vector3.Lerp(startPos, canvasPoint.position, transitionProgress);
            playerCamera.rotation = Quaternion.Lerp(startRot, canvasPoint.rotation, transitionProgress);
            yield return null;
        }

        playerCamera.position = canvasPoint.position;
        playerCamera.rotation = canvasPoint.rotation;

        canvasObject.SetActive(true);
        DisplayCurrentQuestion();
    }

    private void DisplayCurrentQuestion()
    {
        if (currentQuestionIndex < allQuestions.Length)
        {
            string qID = allQuestions[currentQuestionIndex].questionID;
            string bID = allQuestions[currentQuestionIndex].blueAnswerID;
            string gID = allQuestions[currentQuestionIndex].greenAnswerID;

            questiontext.text = LocalizationManager.Instance.GetText(qID);
            pinkbuttontext.text = LocalizationManager.Instance.GetText(bID);
            bluebuttontext.text = LocalizationManager.Instance.GetText(gID);
        }
        else
        {
            EvaluateDialogueOutcome();
        }
    }

    public void OnBlueSelection()
    {
        if (currentQuestionIndex >= allQuestions.Length || isProcessingAnswer) return;
        StartCoroutine(ProcessSelectionSequence(true));
    }

    public void OnGreenSelection()
    {
        if (currentQuestionIndex >= allQuestions.Length || isProcessingAnswer) return;
        StartCoroutine(ProcessSelectionSequence(false));
    }

    private IEnumerator ProcessSelectionSequence(bool isBlue)
    {
        isProcessingAnswer = true;

        if (isBlue) blueScore++;
        else greenScore++;

        bool isLastQuestion = (currentQuestionIndex == allQuestions.Length - 1);

        ActivateQuestionSticker();
        currentQuestionIndex++;

        if (isLastQuestion)
        {
            yield return new WaitForSeconds(1.5f);
        }

        DisplayCurrentQuestion();
        isProcessingAnswer = false;
    }

    private void ActivateQuestionSticker()
    {
        if (allQuestions[currentQuestionIndex].openSticker != null)
        {
            allQuestions[currentQuestionIndex].openSticker.SetActive(true);
        }
    }

    private void EvaluateDialogueOutcome()
    {
        StartCoroutine(FinalEvaluationSequence());
    }

    private IEnumerator FinalEvaluationSequence()
    {
        if (audioSource != null && generalDeathSound != null)
        {
            audioSource.PlayOneShot(generalDeathSound);
        }

        yield return new WaitForSeconds(0.5f);


        bloodEffect.SetActive(true);

        if (greenScore >= 2)
        {
            StartCoroutine(PlayerDeathSequence());
        }
        else if (blueScore >= 2)
        {
            StartCoroutine(SuccessCinematicSequence());
        }
    }

    private IEnumerator PlayerDeathSequence()
    {
        yield return new WaitForSeconds(1f);
        canvasObject.SetActive(false);
        bloodEffect.SetActive(false);

        yield return StartCoroutine(TransitionCameraToPlayer());

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerRb != null) playerRb.isKinematic = false;

        if (playerHealthScript != null)
        {
            playerHealthScript.TakeDamage(playerHealthScript.maxHealth);
        }
    }

    private IEnumerator SuccessCinematicSequence()
    {
        yield return new WaitForSeconds(1f);
        canvasObject.SetActive(false);
        bloodEffect.SetActive(false);

        yield return StartCoroutine(TransitionCameraToPlayer());
        if (viewmodelCameraObj != null) viewmodelCameraObj.SetActive(true);

        if (audioSource != null && gunSound != null)
        {
            audioSource.PlayOneShot(gunSound);
        }

        if (bobbyAnimator != null)
        {
            bobbyAnimator.SetTrigger("Death");
        }

        yield return new WaitForSeconds(3f);

        if (hiddenPadlock != null)
        {
            hiddenPadlock.SetActive(true);
        }

        yield return StartCoroutine(SmoothlyRestoreUIAndControls());

        Collider interactCollider = GetComponent<Collider>();
        if (interactCollider != null) interactCollider.enabled = false;

        isDialogueActive = false;
    }

    private IEnumerator TransitionCameraToPlayer()
    {
        float transitionProgress = 0f;
        Vector3 startPos = playerCamera.localPosition;
        Quaternion startRot = playerCamera.localRotation;

        while (transitionProgress < 1f)
        {
            transitionProgress += Time.deltaTime * cameraTransitionSpeed;
            playerCamera.localPosition = Vector3.Lerp(startPos, originalCameraLocalPos, transitionProgress);
            playerCamera.localRotation = Quaternion.Lerp(startRot, originalCameraLocalRot, transitionProgress);
            yield return null;
        }

        playerCamera.localPosition = originalCameraLocalPos;
        playerCamera.localRotation = originalCameraLocalRot;
    }

    private IEnumerator SmoothlyRestoreUIAndControls()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerRb != null) playerRb.isKinematic = false;
        if (playerController != null) playerController.enabled = true;

        CanvasGroup hudGroup = PrepareCanvasGroup(playerHUDCanvas);
        CanvasGroup healthAndTimerGroup = PrepareCanvasGroup(healthAndTimerCanvas);

        float fadeDuration = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float currentAlpha = Mathf.Clamp01(elapsedTime / fadeDuration);

            if (hudGroup != null) hudGroup.alpha = currentAlpha;
            if (healthAndTimerGroup != null) healthAndTimerGroup.alpha = currentAlpha;

            yield return null;
        }
    }

    private CanvasGroup PrepareCanvasGroup(GameObject obj)
    {
        if (obj == null) return null;

        obj.SetActive(true);
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = obj.AddComponent<CanvasGroup>();
        }

        cg.alpha = 0f;
        return cg;
    }
}