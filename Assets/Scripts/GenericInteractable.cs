using UnityEngine;

namespace DefaultNamespace {
public class GenericInteractable : MonoBehaviour, InterfaceInteractable {
    public enum InteractableType {
        None,
        VitalsMonitor,
        EHR
    }

    [SerializeField]
    string hotspotId;
    public string HotspotId => hotspotId;

    [SerializeField]
    InteractableType interactableType;

    [SerializeField]
    bool opensPanel = true;

    [SerializeField]
    Outline outline;

    [SerializeField]
    bool interactionEnabled = true;

    [Header("Glow colours")]
    [Tooltip("The current node targets this hotspot, and you are not looking at it.")]
    [SerializeField]
    Color scenarioColor = Color.red;

    [Tooltip("You are looking at the hotspot the current node targets — the 'do this now' state.")]
    [SerializeField]
    Color scenarioHoverColor = new Color(0.35f, 0.65f, 1f);

    [Tooltip("You are looking at an interactable the current node does not target.")]
    [SerializeField]
    Color idleHoverColor = Color.white;

    bool isHovered;

    void Update() {
        if (outline == null)
            return;

        bool scenarioActive = ScenarioEngine.Instance != null && ScenarioLookup.IsHotspotActive(ScenarioEngine.Instance.CurrentNode, hotspotId);
        bool shouldGlow = InteractionGlowSettings.Enabled && (isHovered || scenarioActive);

        if (outline.enabled != shouldGlow)
            outline.enabled = shouldGlow;

        if (shouldGlow) {
            if (!isHovered)
                outline.OutlineColor = scenarioColor;
            else
                outline.OutlineColor = scenarioActive ? scenarioHoverColor : idleHoverColor;
        }
    }

    public void SetHovered(bool hovered) {
        isHovered = hovered;
    }

    public string InteractMessage {
        get {
            var option = GetActiveOption();
            return option != null ? $"{option.label} (E)" : "";
        }
    }

    public void Interact() {
        if (!interactionEnabled) {
            return;
        }

        var option = GetActiveOption();

        if (ScenarioEngine.Instance != null)
            ScenarioEngine.Instance.LogHotspot(hotspotId);

        if (option != null) {
            HandleOption(option);
        } else if (!opensPanel) {
            UIManager.Instance.ShowToast("Δεν χρειάζεται αυτή την στιγμή.");
        }

        if (opensPanel)
            OpenPanel();
    }

    Option GetActiveOption() {
        if (ScenarioEngine.Instance == null) {
            return null;
        }
        Node currentNode = ScenarioEngine.Instance.CurrentNode;
        if (currentNode == null) {
            return null;
        }
        return ScenarioLookup.GetOptionForHotspot(currentNode, hotspotId);
    }

    void HandleOption(Option option) {
        if (option == null) {
            return;
        }
        ScenarioEngine.Instance.ChooseOption(option);
    }

    void OpenPanel() {
        if (interactableType == InteractableType.VitalsMonitor) {
            UIManager.Instance.OpenVitalsMonitor();
        } else if (interactableType == InteractableType.EHR) {
            UIManager.Instance.OpenEHR();
        }
    }

    public void SetInteractionEnabled(bool enabled) {
        interactionEnabled = enabled;
    }
}
}
