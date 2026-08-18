using System.Collections.Generic;

public static class EhrDataStore {
    private static readonly Dictionary<string, Dictionary<string, string>> submittedForms = new();

    public static void SubmitForm(string formId, Dictionary<string, string> values) {
        submittedForms[formId] = values;
    }

    public static bool IsFieldFilled(string formId, string field) {
        return submittedForms.TryGetValue(formId, out var fields) && fields.TryGetValue(field, out var value) && !string.IsNullOrWhiteSpace(value);
    }
}
