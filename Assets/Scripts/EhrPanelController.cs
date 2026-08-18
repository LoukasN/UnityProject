using System.Collections.Generic;
using UnityEngine;

public class EhrPanelController : MonoBehaviour
{
    public EhrFormPanel assessmentForm;
    public EhrFormPanel interventionForm;
    public EhrFormPanel communicationForm;

    private EhrFormPanel[] Forms => new[] { assessmentForm, interventionForm, communicationForm };

    private Node activeNode;

    private void OnEnable()
    {
        ApplyGateRequirements();
    }

    private void ApplyGateRequirements()
    {
        Node currentNode = null; // TODO: once ScenarioLoader exposes the rule engine's current node (e.g. loader.CurrentNode), swap this line for it.
        activeNode = currentNode;

        if (currentNode?.gateRequirements?.requiredForms == null)
        {
            foreach (var form in Forms)
                form.SetAllFieldsRequired();
            return;
        }

        var byFormId = new Dictionary<string, List<string>>();
        foreach (var required in currentNode.gateRequirements.requiredForms)
            byFormId[required.formId] = required.fields;

        foreach (var form in Forms)
        {
            byFormId.TryGetValue(form.formId, out var required);
            form.SetRequiredFields(required);
        }
    }

    public void OnSubmit()
    {
        foreach (var form in Forms)
        {
            if (!form.RequiredFieldsFilled())
            {
                UIManager.Instance.ShowToast(activeNode?.feedbackBlocked);
                return;
            }
        }

        foreach (var form in Forms)
            EhrDataStore.SubmitForm(form.formId, form.GetValues());

        UIManager.Instance.ShowToast(activeNode?.feedbackSuccess);
        UIManager.Instance.CloseEHR();
    }
}
