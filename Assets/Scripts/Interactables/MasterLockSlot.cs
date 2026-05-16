using UnityEngine;

public class MasterLockSlot : BaseInteractable
{
    [Header("Visual Settings (Sprites)")]
    [Tooltip("The SpriteRenderer responsible for displaying the lock stages on the scene.")]
    public SpriteRenderer slotSpriteRenderer;

    [Tooltip("Array of sprites representing lock states: 0=Empty, 1=1/4, 2=2/4, 3=3/4, 4=Full")]
    public Sprite[] lockStages = new Sprite[5];

    [Header("Audio & Voiceover Settings")] public AudioSource audioSource;

    [Tooltip("Specific voiceover clips triggered based on the number of placed locks.")]
    public AudioClip[] stageDialogues = new AudioClip[5];

    public AudioClip lockSnapSound;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            int placedLocks = GameManager.Instance.GetTotalSubmittedLocks();

            UpdateVisuals(placedLocks, false);
        }
    }


    protected override void Interact()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[MasterLockSlot] GameManager instance is missing from the scene!");
            return;
        }

        int heldLocks = GameManager.Instance.collectedLocks.Count;

        if (heldLocks > 0)
        {
            Debug.Log($"[MasterLockSlot] Player is submitting {heldLocks} lock(s).");

            GameManager.Instance.SubmitLocks();

            int newTotalLocks = GameManager.Instance.GetTotalSubmittedLocks();

            UpdateVisuals(newTotalLocks, true);
        }
        else
        {
            Debug.Log("[MasterLockSlot] Player has no locks to submit. Interaction ignored.");
        }
    }


    private void UpdateVisuals(int currentPlacedLocks, bool playSounds)
    {
        if (currentPlacedLocks > 4) currentPlacedLocks = 4;

        if (slotSpriteRenderer != null && lockStages.Length > currentPlacedLocks)
        {
            slotSpriteRenderer.sprite = lockStages[currentPlacedLocks];
        }


        if (playSounds && audioSource != null)
        {
            if (lockSnapSound != null) audioSource.PlayOneShot(lockSnapSound);

            if (stageDialogues.Length > currentPlacedLocks && stageDialogues[currentPlacedLocks] != null)
            {
                audioSource.PlayOneShot(stageDialogues[currentPlacedLocks]);
            }
        }
    }
}