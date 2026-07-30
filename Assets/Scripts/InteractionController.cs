using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DefaultNamespace
{
    public class InteractionController : MonoBehaviour
    {
        [SerializeField] Camera playerCamera;
        [SerializeField] TextMeshProUGUI interactionText;
        [SerializeField] float interactionDistance = 5f;
        [SerializeField] LayerMask interactableLayers = ~0;

        InterfaceInteractable currentTargetInteractable;   // <-- was missing

        void Update()
        {
            UpdateCurrentInteractable();
            UpdateInteractionText();
            CheckForInteractionInput();
        }

        void UpdateCurrentInteractable()
        {
            var ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f,
0f));

            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance,
interactableLayers))
                currentTargetInteractable =
hit.collider.GetComponentInParent<InterfaceInteractable>();
            else
                currentTargetInteractable = null;
        }

        void UpdateInteractionText()
        {
            interactionText.text = currentTargetInteractable == null
                ? string.Empty
                : currentTargetInteractable.InteractMessage;
        }

        void CheckForInteractionInput()
        {
            if (currentTargetInteractable != null &&
Keyboard.current.eKey.wasPressedThisFrame)
                currentTargetInteractable.Interact();
        }
    }
}