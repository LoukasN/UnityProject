using UnityEngine;

namespace DefaultNamespace{
    public class GenericInteractable : MonoBehaviour, InterfaceInteractable{
        public string InteractMessage => objectInteractMessage;

        [SerializeField] GameObject spawnPrefab;
        [SerializeField] string objectInteractMessage;

        public void Interact(){
            Instantiate(spawnPrefab);
        }
    }
}
