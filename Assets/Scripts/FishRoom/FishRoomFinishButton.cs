using UnityEngine;

public class FishRoomFinishButton : BaseInteractable
{
    public FishRoomManager roomManager;

    protected override void Interact()
    {
        if (roomManager != null)
        {
            roomManager.TryFinishMinigame();
        }
    }
}