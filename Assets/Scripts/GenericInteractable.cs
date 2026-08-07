using UnityEngine;

namespace DefaultNamespace
{
    public class GenericInteractable : MonoBehaviour, InterfaceInteractable
    {
        public string InteractMessage => objectInteractMessage;

        public enum InteractableType
        {
            CallDoctor,
            BreathTube
        }

        [SerializeField] string objectInteractMessage;

        [SerializeField] InteractableType interactableType;

        public void Interact()
        {
            if (interactableType == InteractableType.CallDoctor)
            {
                UIManager.Instance.OpenCallDoctor();
            }
            else if (interactableType == InteractableType.BreathTube)
            {
                UIManager.Instance.OpenBreathTube();
            }
        }
    }
}
