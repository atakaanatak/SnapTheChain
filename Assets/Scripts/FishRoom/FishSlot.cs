using UnityEngine;

public class FishSlot : BaseInteractable
{
    [Header("Manager Reference")]
    public FishRoomManager roomManager;
    
    [Header("Slot Placement Settings")]
    [Tooltip("Position offset when the fish is placed in the slot.")]
    public Vector3 slotPositionOffset = Vector3.zero;
    
    [Tooltip("Rotation when the fish is placed in the slot.")]
    public Vector3 slotRotation = Vector3.zero;
    
    [Tooltip("Scale when the fish is placed in the slot. Default is (1,1,1).")]
    public Vector3 slotScale = Vector3.one; 

    private bool isFilled = false;

    protected override void Interact()
    {
        if (isFilled) return;

        Transform playerHand = GameObject.Find("HoldPoint").transform;
        
        if (playerHand.childCount > 0)
        {
            Transform heldObject = playerHand.GetChild(0);
            
            if (heldObject.CompareTag("Fish"))
            {
                heldObject.SetParent(transform); 
                heldObject.localPosition = slotPositionOffset; 
                heldObject.localEulerAngles = slotRotation; 
                heldObject.localScale = slotScale; 
                
                Collider col = heldObject.GetComponent<Collider>();
                if (col != null) col.enabled = false;
                
                Rigidbody rb = heldObject.GetComponent<Rigidbody>();
                if (rb != null) 
                { 
                    rb.isKinematic = true; 
                    rb.useGravity = false; 
                }

                BasicProp fishProp = heldObject.GetComponent<BasicProp>();
                if (fishProp != null) 
                {
                    fishProp.ChangeLayerObject(heldObject.gameObject, fishProp.worldLayerName);
                    Destroy(fishProp); 
                }

                isFilled = true;
                GetComponent<Collider>().enabled = false; 
                
                if (roomManager != null) roomManager.AddFish();
                
                Debug.Log("[FishSlot] Fish successfully placed into the slot.");
            }
            else
            {
                Debug.LogWarning("[FishSlot] Invalid object. Only items tagged 'Fish' can be placed here.");
            }
        }
    }
}