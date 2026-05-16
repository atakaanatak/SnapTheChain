using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class RaycastInteractor : MonoBehaviour
{
    [Header("Raycast Settings")]
    [Tooltip("Maximum distance the player can interact with objects.")]
    public float interactDistance = 3f;

    [Header("UI Elements")]
    [Tooltip("TextMeshPro object to display the interaction prompt.")]
    public TextMeshProUGUI promptTextUI;

    private Camera playerCamera;
    private InputAction interactAction;

    private void Awake()
    {
        playerCamera = GetComponent<Camera>();


        interactAction = new InputAction(
            name: "Interact",
            type: InputActionType.Button,
            binding: "<Keyboard>/e"
        );
    }

    private void OnEnable()
    {
        interactAction.Enable();
    }

    private void OnDisable()
    {
        interactAction.Disable();
    }

    private void Update()
    {

        if (promptTextUI == null || playerCamera == null)
            return;


        promptTextUI.text = string.Empty;


        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);


        if (Physics.Raycast(ray, out RaycastHit hitInfo, interactDistance))
        {

            if (hitInfo.collider.TryGetComponent<BaseInteractable>(out BaseInteractable interactableObject))
            {

                promptTextUI.text = interactableObject.promptMessage;


                if (interactAction.WasPressedThisFrame())
                {
                    interactableObject.BaseInteract();
                }
            }
        }
    }
}