using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class StaticExitPortal : MonoBehaviour
{
    [Header("Inventory Check")]
    [Tooltip("The script will automatically locate the 'HoldPoint'. No manual assignment required.")]
    public Transform richardHand;

    [Header("Destination Settings")] public string destinationSceneName = "MainRoom";
    public string destinationPointName;

    [Header("Transition Effects (Fade)")] public Image whiteScreen;
    public Transform player;
    public float fadeSpeed = 3f;

    [Header("Optimization (Scene Cleanup)")]
    public string sceneToUnload;

    [Header("Game Progression (Current Room)")]
    [Tooltip("Which room type will be marked as completed upon entering this portal?")]
    public LockType completedRoomType;

    [Header("Security & Warning System (Target Room)")]
    [Tooltip("Does this portal return to the Main Room? (If true, bypasses completion checks)")]
    public bool isGoingToMainRoom = false;

    [Tooltip("If not returning to the Main Room, specify the target room's lock type.")]
    public LockType destinationRoomType;

    [Tooltip("UI Text element to display warnings.")]
    public TextMeshProUGUI warningTextUI;

    private bool isTeleporting = false;
    private Collider triggerCollider;

    void Start()
    {
        triggerCollider = GetComponent<Collider>();

        if (player == null)
        {
            GameObject adam = GameObject.Find("richard");
            if (adam != null) player = adam.transform;
        }

        if (richardHand == null)
        {
            GameObject handObj = GameObject.Find("HoldPoint");
            if (handObj != null)
            {
                richardHand = handObj.transform;
                Debug.Log("[PortalSystem] HoldPoint successfully linked.");
            }
            else
            {
                if (player != null)
                {
                    richardHand = FindChildRecursive(player, "HoldPoint");
                }
            }
        }

        if (whiteScreen == null)
        {
            GameObject wsObj = GameObject.Find("WhiteScreen");
            if (wsObj != null) whiteScreen = wsObj.GetComponent<Image>();
        }

        if (warningTextUI != null) warningTextUI.gameObject.SetActive(false);
    }

    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform result = FindChildRecursive(child, name);
            if (result != null) return result;
        }

        return null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTeleporting)
        {
            if (richardHand != null && richardHand.childCount > 0)
            {
                Debug.LogWarning("[PortalSystem] Player is holding an item. Teleportation aborted.");
                ShowWarningText("You must drop the item before entering the portal!");
                return;
            }

            if (!isGoingToMainRoom && GameManager.Instance != null)
            {
                if (GameManager.Instance.IsRoomCompleted(destinationRoomType))
                {
                    Debug.LogWarning($"[PortalSystem] Room {destinationRoomType} is already completed. Access denied.");
                    ShowWarningText("You already have the lock for this room. You cannot return!");
                    return;
                }
            }

            EventManager.TriggerRoomCompleted(completedRoomType);
            StartCoroutine(TeleportRoutine());
        }
    }

    private void ShowWarningText(string message)
    {
        if (warningTextUI != null)
        {
            warningTextUI.text = message;
            warningTextUI.gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(HideWarningRoutine());
        }
    }

    private IEnumerator HideWarningRoutine()
    {
        yield return new WaitForSeconds(3f);
        if (warningTextUI != null) warningTextUI.gameObject.SetActive(false);
    }

    IEnumerator TeleportRoutine()
    {
        isTeleporting = true;
        if (triggerCollider != null) triggerCollider.enabled = false;

        if (whiteScreen == null)
        {
            GameObject wsObj = GameObject.Find("WhiteScreen");
            if (wsObj != null) whiteScreen = wsObj.GetComponent<Image>();
        }

        if (whiteScreen == null || player == null)
        {
            Debug.LogError("[PortalSystem] Teleportation failed! Player or WhiteScreen object is missing.");
            yield break;
        }

        Color c = whiteScreen.color;
        while (c.a < 1f)
        {
            c.a += Time.deltaTime * fadeSpeed;
            whiteScreen.color = c;
            yield return null;
        }

        Scene targetScene = SceneManager.GetSceneByName(destinationSceneName);
        if (!targetScene.isLoaded)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(destinationSceneName, LoadSceneMode.Additive);
            while (!asyncLoad.isDone) yield return null;
        }

        GameObject spawnObj = GameObject.Find(destinationPointName);
        if (spawnObj != null)
        {
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = Vector3.zero;

            player.position = spawnObj.transform.position;
            player.eulerAngles = new Vector3(0f, spawnObj.transform.eulerAngles.y, 0f);
        }
        else
        {
            Debug.LogWarning("[PortalSystem] Target spawn point not found.");
        }

        yield return new WaitForSeconds(0.2f);

        while (c.a > 0f)
        {
            c.a -= Time.deltaTime * fadeSpeed;
            whiteScreen.color = c;
            yield return null;
        }

        if (!string.IsNullOrEmpty(sceneToUnload))
        {
            Scene oldScene = SceneManager.GetSceneByName(sceneToUnload);
            if (oldScene.isLoaded)
            {
                SceneManager.UnloadSceneAsync(sceneToUnload);
                Resources.UnloadUnusedAssets();
            }
        }
    }
}