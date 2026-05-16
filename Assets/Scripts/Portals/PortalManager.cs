using UnityEngine;

public class PortalManager : MonoBehaviour
{
    [Header("Dependencies")] public Transform playerCamera;

    [HideInInspector] public Transform activePortalCamera;

    [Header("Portal State Flags")] public bool isPhotoPlaced = false;
    public bool isPlayerInZone = false;


    private Quaternion originalCameraRot;

    private Vector3 startPlayerPos;
    private Quaternion startPlayerRot;
    private Vector3 startPortalCamPos;
    private Quaternion startPortalCamRot;

    private void Start()
    {
        if (playerCamera == null)
        {
            GameObject camObj = GameObject.Find("PlayerCamera");
            if (camObj != null)
            {
                playerCamera = camObj.transform;
            }
            else
            {
                Debug.LogError("[PortalManager] PlayerCamera reference not found in the scene.");
            }
        }
    }

    private void LateUpdate()
    {
        if (!isPhotoPlaced || !isPlayerInZone || activePortalCamera == null || playerCamera == null) return;


        Quaternion localRotationDelta = Quaternion.Inverse(startPlayerRot) * playerCamera.rotation;
        activePortalCamera.rotation = startPortalCamRot * localRotationDelta;
    }

    public void SetDynamicCamera(Transform newCamera)
    {
        activePortalCamera = newCamera;
        isPhotoPlaced = true;

        if (newCamera != null)
        {
            originalCameraRot = newCamera.rotation;
        }

        if (isPlayerInZone)
        {
            LockZeroPoint();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;

            if (isPhotoPlaced && activePortalCamera != null)
            {
                LockZeroPoint();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;


            if (activePortalCamera != null)
            {
                activePortalCamera.rotation = originalCameraRot;
                Debug.Log("[PortalManager] Player exited the portal zone. Camera orientation reset.");
            }
        }
    }

    private void LockZeroPoint()
    {
        if (playerCamera == null || activePortalCamera == null) return;


        startPlayerPos = playerCamera.position;
        startPlayerRot = playerCamera.rotation;

        startPortalCamPos = activePortalCamera.position;
        startPortalCamRot = activePortalCamera.rotation;
    }
}