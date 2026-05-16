using UnityEngine;

public class PadLock : BaseInteractable
{
    [Header("Lock Identity")] [Tooltip("Specifies which room this padlock belongs to.")]
    public LockType targetRoomLockType;

    [Header("Exit Portals")] [Tooltip("Static portals to activate upon collecting this lock.")]
    public GameObject[] staticPanels;

    protected override void Interact()
    {
        Debug.Log($"[PadLock] {targetRoomLockType} lock added to inventory.");

        EventManager.TriggerLockCollected(targetRoomLockType);

        if (staticPanels != null && staticPanels.Length > 0)
        {
            foreach (GameObject panel in staticPanels)
            {
                if (panel != null)
                {
                    panel.SetActive(true);
                    Debug.Log($"[PadLock] Exit portal activated: {panel.name}");
                }
            }
        }

        gameObject.SetActive(false);
    }
}