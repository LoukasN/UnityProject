using System.Collections.Generic;
using UnityEngine;

public class EhrPanelController : MonoBehaviour {
    public EhrFormPanel assessmentForm;
    public EhrFormPanel interventionForm;
    public EhrFormPanel communicationForm;

    private EhrFormPanel[] Forms =>
        new[] { assessmentForm, interventionForm, communicationForm };

    private Node activeNode;

    private void OnEnable() {
        ApplyGateRequirements();
    }

    private void ApplyGateRequirements() {
        if (ScenarioEngine.Instance == null) {
            Debug.LogError("EhrPanelController: ScenarioEngine.Instance is null.");
            return;
        }

        activeNode = ScenarioEngine.Instance.CurrentNode;

        if (activeNode == null) {
            Debug.LogWarning("EhrPanelController: No active scenario node.");
            return;
        }

        // No gate on this node: nothing is required, so nothing is editable. Unlocking
        // every field here would let the player pre-fill all forms early and auto-pass
        // every later gate, since the engine's documentedFields set is never cleared.
        if (activeNode.gateRequirements?.requiredForms == null) {
            foreach (var form in Forms)
                form.SetRequiredFields(null);

            return;
        }

        var byFormId = new Dictionary<string, List<string>>();

        foreach (var required in activeNode.gateRequirements.requiredForms) {
            byFormId[required.formId] = required.fields;
        }

        foreach (var form in Forms) {
            byFormId.TryGetValue(form.formId, out var required);
            form.SetRequiredFields(required);
        }
    }

    public void OnSubmit() {
        if (ScenarioEngine.Instance == null) {
            Debug.LogError("EhrPanelController: ScenarioEngine.Instance is null.");
            return;
        }

        activeNode = ScenarioEngine.Instance.CurrentNode;

        if (activeNode == null) {
            Debug.LogError("EhrPanelController: No active scenario node.");
            return;
        }

        // Nothing to submit outside a gate - close instead of leaving a dead button.
        if (activeNode.type != "gate") {
            UIManager.Instance.CloseEHR();
            return;
        }

        // Check that all required fields have been filled.
        // Default-colour toast: only global_rules with "style": "danger" go red.
        foreach (var form in Forms) {
            if (!form.RequiredFieldsFilled()) {
                UIManager.Instance.ShowToast(activeNode.feedbackBlocked);
                return;
            }
        }

        // Collect the fields that were actually documented.
        var filledFieldKeys = new List<string>();

        foreach (var form in Forms) {
            var values = form.GetValues();

            foreach (var fieldKey in values.Keys) {
                filledFieldKeys.Add(
                    form.formId + "." + fieldKey);
            }
        }

        Debug.Log(
            "EHR SUBMIT: " +
            string.Join(", ", filledFieldKeys));

        // Let ScenarioEngine determine whether the gate is passed.
        ScenarioEngine.Instance.OnEhrSubmit(filledFieldKeys);
    }
}
