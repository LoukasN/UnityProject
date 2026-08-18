using UnityEngine;

namespace DefaultNamespace
{
    public class GenericInteractable : MonoBehaviour, InterfaceInteractable
    {
        public enum InteractableType
        {
            None,
            VitalsMonitor,
            EHR
        }

        [SerializeField] string hotspotId;
        public string HotspotId => hotspotId;

        [SerializeField] string idleMessage = "Press (E) to interact";

        [SerializeField] InteractableType interactableType;

        [SerializeField] bool opensPanel = true;

        [SerializeField] Outline outline;

        [SerializeField] bool interactionEnabled = true;

        ScenarioLoader scenarioLoader;
        Color scenarioGlowColor;
        bool isHovered;

        void Start()
        {
            if (outline != null)
                scenarioGlowColor = outline.OutlineColor;
        }

        void Update()
        {
            if (outline == null)
                return;

            bool scenarioActive = GetActiveOption() != null;
            bool shouldGlow = InteractionGlowSettings.Enabled && (isHovered || scenarioActive);

            if (outline.enabled != shouldGlow)
                outline.enabled = shouldGlow;

            if (shouldGlow)
                outline.OutlineColor = isHovered ? Color.white : scenarioGlowColor;
        }

        public void SetHovered(bool hovered)
        {
            isHovered = hovered;
        }

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

            if (!interactionEnabled){
                return;
            }

            if (option != null)
            {
                HandleOption(option);
            }
            else
            {
                UIManager.Instance.ShowToast($"{hotspotId}: not needed right now.");
            }

            if (opensPanel)
                OpenPanel();
        }

        Option GetActiveOption()
        {
            var loader = GetScenarioLoader();
            if (loader == null || loader.CurrentScenario == null)
                return null;

            Node currentNode = null; // TODO: once ScenarioLoader exposes the rule engine's current node (e.g. loader.CurrentNode), swap this line for it.

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
            if (interactableType == InteractableType.VitalsMonitor)
            {
                UIManager.Instance.OpenVitalsMonitor();
            }
            else if (interactableType == InteractableType.EHR)
            {
                UIManager.Instance.OpenEHR();
            }
        }

        public void SetInteractionEnabled(bool enabled){
            interactionEnabled = enabled;
        }

        ScenarioLoader GetScenarioLoader()
        {
            if (scenarioLoader == null)
                scenarioLoader = FindFirstObjectByType<ScenarioLoader>();

            return scenarioLoader;
        }
    }
}
