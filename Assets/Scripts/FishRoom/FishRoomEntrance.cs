using UnityEngine;

public class FishRoomEntrance : MonoBehaviour
{
    [Header("Core Dependencies")]
    public FishRoomManager roomManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (roomManager != null)
            {
                roomManager.SetTimerVisibility(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (roomManager != null)
            {
                roomManager.SetTimerVisibility(false);
            }
        }
    }
}