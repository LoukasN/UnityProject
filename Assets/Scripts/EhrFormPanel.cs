using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EhrFormPanel : MonoBehaviour
{
    [Serializable]
    public class FieldEntry
    {
        public string fieldKey;
        public TMP_InputField input;
    }

    public string formId;
    public List<FieldEntry> fields;
    public Color disabledLabelColor = new Color(1f, 1f, 1f, 0.35f);

    private readonly HashSet<string> requiredFields = new();

    public void SetRequiredFields(IEnumerable<string> keys)
    {
        requiredFields.Clear();
        if (keys != null)
        {
            foreach (var key in keys)
                requiredFields.Add(key);
        }
        RefreshInteractable();
    }

    public void SetAllFieldsRequired()
    {
        requiredFields.Clear();
        foreach (var entry in fields)
            requiredFields.Add(entry.fieldKey);
        RefreshInteractable();
    }

    private void RefreshInteractable()
    {
        foreach (var entry in fields)
        {
            bool required = requiredFields.Contains(entry.fieldKey);
            entry.input.interactable = required;
            if (!required)
                entry.input.text = "";

            var label = entry.input.transform.parent.Find("Label");
            if (label != null && label.TryGetComponent(out TMP_Text labelText))
                labelText.color = required ? Color.white : disabledLabelColor;
        }
    }

    public Dictionary<string, string> GetValues()
    {
        var values = new Dictionary<string, string>();
        foreach (var entry in fields)
        {
            if (requiredFields.Contains(entry.fieldKey))
                values[entry.fieldKey] = entry.input.text;
        }
        return values;
    }

    public bool RequiredFieldsFilled()
    {
        foreach (var entry in fields)
        {
            if (requiredFields.Contains(entry.fieldKey) && string.IsNullOrWhiteSpace(entry.input.text))
                return false;
        }
        return true;
    }
}
