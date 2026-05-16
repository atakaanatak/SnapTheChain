using System;

public class EventManager
{
    
    public static event Action<LockType> OnLockCollected;
    
    public static void TriggerLockCollected(LockType lockType)
    {

        OnLockCollected?.Invoke(lockType);
    }
    
    public static event Action<LockType> OnRoomCompleted;
    
    public static void TriggerRoomCompleted(LockType completedRoom)
    {
        OnRoomCompleted?.Invoke(completedRoom);
    }
}