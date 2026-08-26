using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EhrPanelController : MonoBehaviour {
    public EhrFormPanel assessmentForm;
    public EhrFormPanel interventionForm;
    public EhrFormPanel communicationForm;
    public Button submitButton;

    private EhrFormPanel[] Forms => new[] {
        assessmentForm, interventionForm, communicationForm
    };

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

        RefreshSubmitButton();

        if (activeNode == null) {
            Debug.LogWarning("EhrPanelController: No active scenario node.");
            return;
        }

        if (activeNode.gateRequirements?.requiredForms == null) {
            foreach (var form in Forms) {
                form.SetRequiredFields(null);
            }
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

    private void RefreshSubmitButton() {
        if (submitButton != null) {
            submitButton.interactable = activeNode != null && activeNode.type == "gate";
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

        if (activeNode.type != "gate") {
            UIManager.Instance.CloseEHR();
            return;
        }

        foreach (var form in Forms) {
            if (!form.RequiredFieldsFilled()) {
                UIManager.Instance.ShowToast(activeNode.feedbackBlocked);
                return;
            }
        }

        var filledFieldKeys = new List<string>();

        foreach (var form in Forms) {
            var values = form.GetValues();

            foreach (var fieldKey in values.Keys) {
                filledFieldKeys.Add(form.formId + "." + fieldKey);
            }
        }

        ScenarioEngine.Instance.OnEhrSubmit(filledFieldKeys);
    }
}
