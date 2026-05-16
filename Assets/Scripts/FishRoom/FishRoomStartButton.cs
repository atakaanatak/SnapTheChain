using UnityEngine;

public class FishRoomStartButton : BaseInteractable
{
    public FishRoomManager roomManager;

    protected override void Interact()
    {
        if (roomManager != null)
        {
            roomManager.StartMinigame();
        }
    }
}