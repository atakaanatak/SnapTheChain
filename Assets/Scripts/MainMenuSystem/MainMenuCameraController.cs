using UnityEngine;
using System.Collections;

public class MainMenuCameraController : MonoBehaviour
{
    [Header("Camera")]
    public Transform menuCamera;

    [Header("Menu Camera Points")]
    public Transform mainMenuPoint;
    public Transform creditsPoint;
    public Transform settingsPoint;
    public Transform howToPlayPoint;

    [Header("Transition Settings")]
    public float moveSpeed = 3f;
    public float rotateSpeed = 3f;

    private Coroutine transitionRoutine;
    private bool isLocked = false;

    private void Start()
    {
        MoveToPointInstant(mainMenuPoint);
    }

    public void LockCameraController()
    {
        isLocked = true;
        StopCurrentTransition();
    }

    public void UnlockCameraController()
    {
        isLocked = false;
    }

    public void GoToMainMenu()
    {
        if (isLocked)
            return;

        MoveToPoint(mainMenuPoint);
    }

    public void GoToCredits()
    {
        if (isLocked)
            return;

        MoveToPoint(creditsPoint);
    }

    public void GoToSettings()
    {
        if (isLocked)
            return;

        MoveToPoint(settingsPoint);
    }

    public void GoToHowToPlay()
    {
        if (isLocked)
            return;

        MoveToPoint(howToPlayPoint);
    }

    private void MoveToPoint(Transform targetPoint)
    {
        if (isLocked)
            return;

        if (menuCamera == null || targetPoint == null)
        {
            Debug.LogWarning("[MainMenuCameraController] Camera or target point is missing.");
            return;
        }

        StopCurrentTransition();

        transitionRoutine = StartCoroutine(MoveCameraRoutine(targetPoint));
    }

    private void MoveToPointInstant(Transform targetPoint)
    {
        if (menuCamera == null || targetPoint == null)
            return;

        menuCamera.position = targetPoint.position;
        menuCamera.rotation = targetPoint.rotation;
    }

    private void StopCurrentTransition()
    {
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }
    }

    private IEnumerator MoveCameraRoutine(Transform targetPoint)
    {
        while (
            Vector3.Distance(menuCamera.position, targetPoint.position) > 0.01f ||
            Quaternion.Angle(menuCamera.rotation, targetPoint.rotation) > 0.1f
        )
        {
            if (isLocked)
            {
                transitionRoutine = null;
                yield break;
            }

            menuCamera.position = Vector3.Lerp(
                menuCamera.position,
                targetPoint.position,
                Time.deltaTime * moveSpeed
            );

            menuCamera.rotation = Quaternion.Lerp(
                menuCamera.rotation,
                targetPoint.rotation,
                Time.deltaTime * rotateSpeed
            );

            yield return null;
        }

        menuCamera.position = targetPoint.position;
        menuCamera.rotation = targetPoint.rotation;
        transitionRoutine = null;
    }
}