using UnityEngine;
using System.Collections;

public class ChestInteractable : BaseInteractable
{
    [Header("Chest Animation Settings")] [Tooltip("The transform of the chest lid that will be rotated.")]
    public Transform chestLid;

    [Tooltip("The target X-axis angle when the chest lid is fully open.")]
    public float openAngleX = 0f;

    [Tooltip("The speed at which the chest lid opens.")]
    public float openSpeed = 3f;

    [Header("Hidden Item")] [Tooltip("The PadLock (or any other item) that will be revealed once the chest is opened.")]
    public GameObject hiddenLock;

    private bool isOpened = false;

    private void Start()
    {
        if (hiddenLock != null)
        {
            hiddenLock.SetActive(false);
        }
    }

    protected override void Interact()
    {
        if (!isOpened)
        {
            isOpened = true;
            Debug.Log("[ChestInteractable] Chest interaction triggered. Opening lid.");

            StartCoroutine(OpenLidRoutine());

            promptMessage = string.Empty;
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }
    }

    private IEnumerator OpenLidRoutine()
    {
        float progress = 0f;
        Quaternion initialRotation = chestLid.localRotation;

        Quaternion targetRotation =
            Quaternion.Euler(openAngleX, chestLid.localEulerAngles.y, chestLid.localEulerAngles.z);

        while (progress < 1f)
        {
            progress += Time.deltaTime * openSpeed;

            chestLid.localRotation = Quaternion.Lerp(initialRotation, targetRotation, progress);
            yield return null;
        }


        chestLid.localRotation = targetRotation;

        if (hiddenLock != null)
        {
            hiddenLock.SetActive(true);
            Debug.Log("[ChestInteractable] Hidden item is now active and accessible.");
        }
    }
}