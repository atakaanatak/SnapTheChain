using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PanoSlot : BaseInteractable
{
    [Header("Slot References")] public Transform photoSocket;
    public DynamicTeleporter myTeleporter;
    public PortalManager myPortalManager;

    [Header("Visual Settings")] public Vector3 placedPhotoScale = new Vector3(0.1f, 1f, 1f);

    private PhotoItem currentPhotoData;
    private Transform currentPlacedPhoto;
    private Material originalThumbnailMaterial;

    private bool isRoomCurrentlyLoaded = false;

    protected override void Interact()
    {
        Transform playerHand = GameObject.Find("HoldPoint").transform;

        if (playerHand.childCount > 0)
        {
            Transform heldObject = playerHand.GetChild(0);

            if (heldObject.CompareTag("Photo"))
            {
                heldObject.SetParent(photoSocket);
                heldObject.localPosition = Vector3.zero;
                heldObject.localEulerAngles = Vector3.zero;
                heldObject.localScale = placedPhotoScale;

                currentPlacedPhoto = heldObject;
                currentPhotoData = heldObject.GetComponent<PhotoItem>();

                MeshRenderer mr = currentPlacedPhoto.GetComponentInChildren<MeshRenderer>();
                if (mr != null) originalThumbnailMaterial = mr.material;

                Collider slotCollider = GetComponent<Collider>();
                if (slotCollider != null) slotCollider.enabled = false;

                if (myPortalManager != null && myPortalManager.isPlayerInZone)
                {
                    isRoomCurrentlyLoaded = true;
                    StartCoroutine(LoadRoomFlow());
                }
            }
        }
    }

    void Update()
    {
        if (currentPhotoData == null || myPortalManager == null) return;

        if (myPortalManager.isPlayerInZone && !isRoomCurrentlyLoaded)
        {
            isRoomCurrentlyLoaded = true;
            StartCoroutine(LoadRoomFlow());
        }
        else if (!myPortalManager.isPlayerInZone && isRoomCurrentlyLoaded)
        {
            isRoomCurrentlyLoaded = false;
            UnloadRoomFlow();
        }
    }

    private IEnumerator LoadRoomFlow()
    {
        Scene targetScene = SceneManager.GetSceneByName(currentPhotoData.targetSceneName);
        if (!targetScene.isLoaded)
        {
            AsyncOperation asyncLoad =
                SceneManager.LoadSceneAsync(currentPhotoData.targetSceneName, LoadSceneMode.Additive);
            while (!asyncLoad.isDone) yield return null;
        }

        MeshRenderer mr = currentPlacedPhoto.GetComponentInChildren<MeshRenderer>();
        if (mr != null && currentPhotoData.livePortalMaterial != null)
        {
            mr.material = currentPhotoData.livePortalMaterial;
        }

        GameObject spawnPointObj = GameObject.Find(currentPhotoData.spawnPointName);
        GameObject cameraObj = GameObject.Find(currentPhotoData.targetCameraName);

        if (myTeleporter != null && spawnPointObj != null)
            myTeleporter.ActivateTeleporter(spawnPointObj.transform, currentPhotoData.targetRoomLockType);

        if (myPortalManager != null && cameraObj != null)
            myPortalManager.SetDynamicCamera(cameraObj.transform);
    }

    private void UnloadRoomFlow()
    {
        StopAllCoroutines();

        MeshRenderer mr = currentPlacedPhoto.GetComponentInChildren<MeshRenderer>();
        if (mr != null && originalThumbnailMaterial != null)
        {
            mr.material = originalThumbnailMaterial;
        }

        Scene targetScene = SceneManager.GetSceneByName(currentPhotoData.targetSceneName);
        if (targetScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(currentPhotoData.targetSceneName);
            Resources.UnloadUnusedAssets();
            Debug.Log($"[Memory Management] {currentPhotoData.targetSceneName} unloaded to optimize RAM usage.");
        }
    }
}