using UnityEngine;

public abstract class BaseInteractable : MonoBehaviour
{
    [Header("UI & Sound Settings")]
    [Tooltip("The message that appears on the screen when looking at the object.")]
    public string promptMessage = "Interact";
    
    [Tooltip("Optional sound to play when interacting.")]
    public AudioClip interactSound;
    public void BaseInteract()
    {
        if (interactSound != null)
        {
            AudioSource.PlayClipAtPoint(interactSound, transform.position);
        }
        Interact(); 
    }
    protected abstract void Interact();
}