using System.Collections.Generic;
using UnityEngine;

public class RoomStateManager : MonoBehaviour
{
    public static RoomStateManager Instance;

    [Header("Room States Memory")]
    [Tooltip("Tracks the completion status of each room lock type.")]
    public Dictionary<LockType, bool> roomStates = new Dictionary<LockType, bool>();

    private void Awake()
    {
        if (Instance == null) 
        { 
            Instance = this; 
            DontDestroyOnLoad(gameObject); 
        }
        else 
        { 
            Destroy(gameObject); 
            return; 
        }
        
        roomStates.Add(LockType.Maze, false);
        roomStates.Add(LockType.Platform, false);
        roomStates.Add(LockType.Slave, false);
    }

    private void OnEnable() => EventManager.OnRoomCompleted += MarkRoomAsComplete;
    private void OnDisable() => EventManager.OnRoomCompleted -= MarkRoomAsComplete;

    private void MarkRoomAsComplete(LockType completedRoom)
    {
        if (roomStates.ContainsKey(completedRoom))
        {
            roomStates[completedRoom] = true;
            Debug.Log($"[RoomStateManager] Room progression updated. {completedRoom} marked as complete.");
        }
    }

    public void ResetAllRooms()
    {
        var keys = new List<LockType>(roomStates.Keys);
        foreach (var key in keys)
        {
            roomStates[key] = false;
        }
        
        Debug.Log("[RoomStateManager] Player death state processed. All room progression has been reset.");
    }
}