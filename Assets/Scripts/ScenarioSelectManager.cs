using System.Collections;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioSelectManager : MonoBehaviour {
    public GameObject scenarioButtonPrefab;
    public Transform listContainer;

    void OnEnable() {
        StartCoroutine(PopulateWhenReady());
    }

    IEnumerator PopulateWhenReady() {
        while (FindFirstObjectByType<ScenarioLoader>() == null) {
            yield return null;
        }
        PopulateList();
    }

    void OnApplicationFocus(bool hasFocus) {
        if (hasFocus && isActiveAndEnabled) {
            PopulateList();
        }
    }

    public void OpenScenarioFolder() {
        string path = Application.streamingAssetsPath;

        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        System.Diagnostics.Process.Start("explorer.exe", "\"" + Path.GetFullPath(path).Replace('/', '\\') + "\"");
#else
        Application.OpenURL("file://" + Path.GetFullPath(path).Replace('\\', '/'));
#endif
    }

    public void PopulateList() {
        ClearList();

        var loader = FindFirstObjectByType<ScenarioLoader>();
        if (loader == null) {
            Debug.LogError("ScenarioSelectManager: no ScenarioLoader found.");
            return;
        }

        foreach (string path in loader.GetScenarios()) {
            string title = ReadTitle(path);
            if (title == null) {
                continue;
            }

            string capturedPath = path;
            GameObject buttonObj = Instantiate(scenarioButtonPrefab, listContainer);
            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = title;

            Button button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(() => LoadScenario(capturedPath));
        }
    }

    void ClearList() {
        for (int i = listContainer.childCount - 1; i >= 0; i--) {
            Transform child = listContainer.GetChild(i);
            child.SetParent(null);
            Destroy(child.gameObject);
        }
    }

    string ReadTitle(string path) {
        try {
            var scenario = JsonConvert.DeserializeObject<Scenario>(File.ReadAllText(path));
            if (scenario?.meta == null || string.IsNullOrWhiteSpace(scenario.meta.title)) {
                Debug.LogWarning($"Skipping '{Path.GetFileName(path)}': no scenario_meta.title.");
                return null;
            }
            return scenario.meta.title;
        } catch (System.Exception ex) {
            Debug.LogWarning($"Skipping '{Path.GetFileName(path)}': {ex.Message}");
            return null;
        }
    }

    void LoadScenario(string path) {
        var loader = FindFirstObjectByType<ScenarioLoader>();

        if (!loader.LoadScenario(path)) {
            PopulateList();
            return;
        }

        UIManager.Instance.RefreshStartScreenText();
        UIManager.Instance.CloseScenarioSelect();
    }
}
