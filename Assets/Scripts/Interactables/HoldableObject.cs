using UnityEngine;
using UnityEngine.InputSystem;

public abstract class HoldableObject : BaseInteractable
{
    [Header("Retention Settings")] public Vector3 holdScale = new Vector3(0.5f, 0.5f, 0.5f);
    public Vector3 holdRotation = Vector3.zero;
    public Vector3 holdPositionOffset = Vector3.zero;

    [Header("Drop Settings")] public bool canBeDropped = false;

    protected bool isHeld = false;

    protected override void Interact()
    {
        if (isHeld) return;
        Transform playerHand = GameObject.Find("HoldPoint").transform;
        if (playerHand.childCount > 0)
        {
            Debug.LogWarning("[HoldableObject] Player's hand is already occupied. Interaction denied.");
            return;
        }

        GrabObject();
    }

    protected virtual void Update()
    {
        if (isHeld && canBeDropped && Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
        {
            DropObject();
        }
    }

    protected virtual void GrabObject()
    {
        Transform playerHand = GameObject.Find("HoldPoint").transform;

        transform.SetParent(playerHand);
        transform.localPosition = holdPositionOffset;
        transform.localEulerAngles = holdRotation;
        transform.localScale = holdScale;

        isHeld = true;
        OnGrabbed();
    }

    protected virtual void DropObject()
    {
        transform.SetParent(null);
        isHeld = false;
        ChangeLayerObject(gameObject, "Default");
    }

    protected virtual void OnGrabbed()
    {
    }

    public void ChangeLayerObject(GameObject targetObject, string layerName)
    {
        int newLayer = LayerMask.NameToLayer(layerName);
        if (newLayer == -1) return;

        Transform[] allChildren = targetObject.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            child.gameObject.layer = newLayer;
        }
    }
}