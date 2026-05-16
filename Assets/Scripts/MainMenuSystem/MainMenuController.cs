using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Scene Settings")] public string coreSceneName = "CoreScene";

    [Header("Tutorial Board")] public GameObject tutorialBlueBoard;
    public GameObject tutorialBoardObjects;

    [Header("Credits Board")] public GameObject creditsBoard;

    [Header("Back Buttons")] public GameObject backTutorialButton;
    public GameObject backCreditsButton;

    [Header("Other Menu Camera Controller")]
    public MonoBehaviour mainMenuController;

    [Header("Camera")] public Camera mainCamera;

    public Transform tutorialLookPoint;
    public Transform tutorialZoomPoint;

    public Transform creditsLookPoint;
    public Transform creditsZoomPoint;

    [Header("Black Fade")] public GameObject blackFadePanel;
    public CanvasGroup blackFadeCanvasGroup;

    [Header("Start Timing")] public float startTurnDuration = 1.2f;
    public float startZoomDuration = 1f;
    public float startWaitAfterZoom = 0.3f;

    [Header("Quit Timing")] public float quitTurnDuration = 1.2f;
    public float quitZoomDuration = 1f;
    public float quitWaitAfterZoom = 0.5f;

    [Header("Fade Timing")] public float fadeDuration = 0.8f;
    public float waitAfterFade = 1f;

    private bool isTransitioning = false;

    public void StartGame()
    {
        if (isTransitioning) return;
        StartCoroutine(StartGameSequence());
    }

    private IEnumerator StartGameSequence()
    {
        isTransitioning = true;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        DisableOtherMenuController();
        DisableBackButtons();
        PrepareFade();

        if (tutorialBoardObjects != null) tutorialBoardObjects.SetActive(false);
        if (tutorialBlueBoard != null) tutorialBlueBoard.SetActive(true);

        if (mainCamera != null && tutorialLookPoint != null)
            yield return MoveCameraToPoint(tutorialLookPoint, startTurnDuration);

        if (mainCamera != null && tutorialZoomPoint != null)
            yield return MoveCameraToPoint(tutorialZoomPoint, startZoomDuration);

        yield return new WaitForSeconds(startWaitAfterZoom);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(coreSceneName, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;

        yield return FadeToBlack();
        yield return new WaitForSeconds(waitAfterFade);

        asyncLoad.allowSceneActivation = true;
    }

    public void QuitGame()
    {
        if (isTransitioning) return;
        StartCoroutine(QuitGameSequence());
    }

    private IEnumerator QuitGameSequence()
    {
        isTransitioning = true;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        DisableOtherMenuController();
        DisableBackButtons();
        PrepareFade();

        if (creditsBoard != null) creditsBoard.SetActive(true);

        if (mainCamera != null && creditsLookPoint != null)
            yield return MoveCameraToPoint(creditsLookPoint, quitTurnDuration);

        if (mainCamera != null && creditsZoomPoint != null)
            yield return MoveCameraToPoint(creditsZoomPoint, quitZoomDuration);

        yield return new WaitForSeconds(quitWaitAfterZoom);
        yield return FadeToBlack();
        yield return new WaitForSeconds(waitAfterFade);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void DisableOtherMenuController()
    {
        if (mainMenuController != null) mainMenuController.enabled = false;
    }

    private void DisableBackButtons()
    {
        if (backTutorialButton != null) backTutorialButton.SetActive(false);
        if (backCreditsButton != null) backCreditsButton.SetActive(false);
    }

    private void PrepareFade()
    {
        if (blackFadePanel != null)
        {
            blackFadePanel.SetActive(true);
            blackFadePanel.transform.SetAsLastSibling();
        }

        if (blackFadeCanvasGroup != null)
        {
            blackFadeCanvasGroup.alpha = 0f;
            blackFadeCanvasGroup.blocksRaycasts = true;
            blackFadeCanvasGroup.interactable = false;
        }
    }

    private IEnumerator MoveCameraToPoint(Transform targetPoint, float duration)
    {
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;
        Vector3 targetPosition = targetPoint.position;
        Quaternion targetRotation = targetPoint.rotation;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t);

            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = targetRotation;
    }

    private IEnumerator FadeToBlack()
    {
        if (blackFadeCanvasGroup == null) yield break;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            blackFadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        blackFadeCanvasGroup.alpha = 1f;
    }
}