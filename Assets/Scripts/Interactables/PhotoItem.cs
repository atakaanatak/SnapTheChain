using UnityEngine;

public class PhotoItem : HoldableObject 
{
    [Header("Target Identity Settings")]
    [Tooltip("Exact name of the target scene (e.g., SlaveRoom).")]
    public string targetSceneName; 

    [Tooltip("Name of the target teleport spawn point (e.g., SlaveRoomSpawnPoint).")]
    public string spawnPointName; 

    [Tooltip("Name of the target camera reference point (e.g., SlaveRoomCamera).")]
    public string targetCameraName; 

    [Header("Portal Illusion Settings")]
    [Tooltip("The RenderTexture material used to display the live feed when this photo is placed on the slot.")]
    public Material livePortalMaterial;
    
    [Header("Progression System")]
    [Tooltip("The lock type associated with this photograph's destination.")]
    public LockType targetRoomLockType; 
}