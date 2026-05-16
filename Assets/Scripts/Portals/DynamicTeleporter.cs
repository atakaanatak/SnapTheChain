using UnityEngine;
using UnityEngine.UI;

public class DynamicTeleporter : MonoBehaviour
{
    [Header("Inventory Settings")]
    [Tooltip("The script will automatically locate the 'HoldPoint'. No manual assignment required.")]
    public Transform richardHand;
    
    [Header("UI & Effects")] 
    public Image whiteScreen;
    public float fadeSpeed = 3f;
    public Transform player;

    private LockType myRoomLock;
    private Transform currentDestination;
    private Collider triggerCollider;
    private bool isTeleporting = false;

    private void Start()
    {
        triggerCollider = GetComponent<Collider>();
        triggerCollider.enabled = false;

        if (player == null)
        {
            GameObject playerObj = GameObject.Find("richard");
            if (playerObj != null) player = playerObj.transform;
        }

        if (richardHand == null)
        {
            GameObject handObj = GameObject.Find("HoldPoint");
            if (handObj != null) 
            {
                richardHand = handObj.transform;
                Debug.Log("[Teleporter] HoldPoint successfully linked.");
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

    public void ActivateTeleporter(Transform newDestination, LockType roomLock)
    {
        currentDestination = newDestination;
        myRoomLock = roomLock;
        triggerCollider.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTeleporting)
        {
            if (richardHand != null && richardHand.childCount > 0)
            {
                Debug.LogWarning("[Teleporter] Player's hand is full. Teleportation denied.");
                return; 
            }
            
            if (currentDestination != null)
            {
                if (GameManager.Instance != null && GameManager.Instance.IsRoomCompleted(myRoomLock))
                {
                    return; 
                }

                isTeleporting = true;
                triggerCollider.enabled = false;

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.StartTeleportSequence(player, currentDestination, whiteScreen, fadeSpeed);
                }
            }
        }
    }
}