using UnityEngine;

public class FishRoomChest : ChestInteractable
{
    [Header("Core References")]
    public FishRoomManager roomManager;

    protected override void Interact()
    {
        if (roomManager != null && !roomManager.isRoomWon)
        {
            Debug.LogWarning("[FishRoomChest] Chest is locked. Minigame must be completed first.");
            return; 
        }

        base.Interact(); 
    }
}