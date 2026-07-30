using UnityEngine;

namespace DefaultNamespace
{
    public class VitalsMonitor : MonoBehaviour, InterfaceInteractable
    {
        public string InteractMessage => objectInteractMessage;

        [SerializeField]
        GameObject spawnPrefab;

        [SerializeField]
        string objectInteractMessage;

        void spawn()
        {
            var spawnobject = Instantiate(spawnPrefab, transform.position + Vector3.up, Quaternion.identity);

            var randomSize = Random.Range(0.1f, 1f);
            spawnobject.transform.localScale = Vector3.one * randomSize;

            var randomColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            spawnobject.GetComponent<MeshRenderer>().material.color = randomColor;
        }

        public void Interact()
        {
            spawn();
        }

    }
}
