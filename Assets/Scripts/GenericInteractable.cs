using UnityEngine;

namespace DefaultNamespace
{
    public class GenericInteractable : MonoBehaviour, InterfaceInteractable
    {
        public enum InteractableType
        {
            CallDoctor,
            BreathTube,
            VitalsMonitor
        }

        [SerializeField] string hotspotId;

        [SerializeField] string idleMessage = "Press (E) to interact";

        [SerializeField] InteractableType interactableType;

        ScenarioLoader scenarioLoader;

        public string InteractMessage
        {
            get
            {
                var option = GetActiveOption();
                return option != null ? $"{option.label} (E)" : idleMessage;
            }
        }

        public void Interact()
        {
            var option = GetActiveOption();

            if (option != null)
            {
                HandleOption(option);
            }
            else
            {
                UIManager.Instance.ShowToast($"{hotspotId}: not needed right now.");
            }

            OpenPanel();
        }

        Option GetActiveOption()
        {
            var loader = GetScenarioLoader();
            if (loader == null || loader.CurrentScenario == null)
                return null;

            Node currentNode = null;

            return ScenarioLookup.GetOptionForHotspot(currentNode, hotspotId);
        }

        void HandleOption(Option option)
        {
            UIManager.Instance.ShowToast(option.effects?.toast);

            if (option.effects?.vitalsUpdate != null)
            {
                var vitalsSource = FindFirstObjectByType<VitalsDataSource>();
                vitalsSource?.ApplyVitalsUpdate(option.effects.vitalsUpdate);
            }
        }

        void OpenPanel()
        {
            if (interactableType == InteractableType.CallDoctor)
            {
                UIManager.Instance.OpenCallDoctor();
            }
            else if (interactableType == InteractableType.BreathTube)
            {
                UIManager.Instance.OpenBreathTube();
            }
            else if (interactableType == InteractableType.VitalsMonitor)
            {
                UIManager.Instance.OpenVitalsMonitor();
            }
        }

        ScenarioLoader GetScenarioLoader()
        {
            if (scenarioLoader == null)
                scenarioLoader = FindFirstObjectByType<ScenarioLoader>();

            return scenarioLoader;
        }
    }
}
