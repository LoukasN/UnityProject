using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class ScenarioLoader : MonoBehaviour {
    private Scenario currentScenario;

    void Start() {
        Debug.Log("Starting Scenario...");
        string[] filePath = GetScenarios();
        if (filePath.Length > 0){
            LoadScenario(filePath[0]);
        }
    }

    public string[] GetScenarios() {
        return Directory.GetFiles(Application.streamingAssetsPath, "*.json");
    }

    public bool LoadScenario(string path) {
        if (!File.Exists(path)) {
            Debug.LogError("File not found: " + path);
            return false;
        }
    
        try {
            string text = File.ReadAllText(path);
            currentScenario = JsonConvert.DeserializeObject<Scenario>(text);
    
            Debug.Log($"Loaded scenario: {currentScenario.meta.title}");
            Debug.Log($"Schema version: {currentScenario.schemaVersion}");
            return true;
        }
        catch (JsonException ex) {
            Debug.LogError($"Failed to parse {path}: {ex.Message}");
            return false;
        }
    }
}
