using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class ScenarioLoader : MonoBehaviour {
    private Scenario currentScenario;
    public Scenario CurrentScenario => currentScenario;
    public string[] GetScenarios() {
        if (!Directory.Exists(Application.streamingAssetsPath)) {
            return new string[0];
        }
        return Directory.GetFiles(Application.streamingAssetsPath, "*.json");
    }

    public bool LoadScenario(string path) {
        if (!File.Exists(path)) {
            Debug.LogError("File not found: " + path);
            return false;
        }
        try {
            string text = File.ReadAllText(path);
            var loaded = JsonConvert.DeserializeObject<Scenario>(text);

            if (loaded?.meta == null || loaded.nodes == null || loaded.nodes.Count == 0) {
                Debug.LogError($"Invalid scenario {path}: missing scenario_meta or nodes.");
                return false;
            }

            currentScenario = loaded;
            Debug.Log($"Loaded scenario: {currentScenario.meta.title}");
            Debug.Log($"Schema version: {currentScenario.schemaVersion}");
            return true;
        } catch (System.Exception ex) {
            Debug.LogError($"Failed to parse {path}: {ex.Message}");
            return false;
        }
    }
}
