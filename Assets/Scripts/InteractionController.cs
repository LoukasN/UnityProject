using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DefaultNamespace {
public class InteractionController : MonoBehaviour {
    [SerializeField]
    Camera playerCamera;
    [SerializeField]
    TextMeshProUGUI interactionText;
    [SerializeField]
    float interactionDistance = 5f;
    [SerializeField]
    LayerMask interactableLayers = ~0;

    InterfaceInteractable currentTargetInteractable;
    PlayerMovement playerMovement;

    void Start() {
        playerMovement = FindFirstObjectByType<PlayerMovement>();
    }

    void Update() {
        if (playerMovement != null && !playerMovement.canMove) {
            interactionText.text = string.Empty;
            currentTargetInteractable?.SetHovered(false);
            currentTargetInteractable = null;
            return;
        }

        UpdateCurrentInteractable();
        UpdateInteractionText();
        CheckForInteractionInput();
    }

    void UpdateCurrentInteractable() {
        var ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        InterfaceInteractable newTarget = null;
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayers)) {
            newTarget = hit.collider.GetComponentInParent<InterfaceInteractable>();
        }

        if (newTarget != currentTargetInteractable) {
            currentTargetInteractable?.SetHovered(false);
            newTarget?.SetHovered(true);
            currentTargetInteractable = newTarget;
        }
    }

    void UpdateInteractionText() {
        interactionText.text = currentTargetInteractable == null ? string.Empty : currentTargetInteractable.InteractMessage;
    }

    void CheckForInteractionInput() {
        if (currentTargetInteractable != null &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
            currentTargetInteractable.Interact();
    }
}
}
