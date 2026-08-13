using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class ScenarioLoader : MonoBehaviour {
    private Scenario scenario;
    private string[] paths;

    void Start() {
        Debug.Log("Starting Scenario...");
        path = Directory.GetFiles(Application.streamingAssetsPath, "*.json");
        
        if (paths.Length == 0) {
            Debug.LogError("No JSON file found in: " + Application.streamingAssetsPath);
            return;
        }

        string text = File.ReadAllText(paths[0]);
        scenario = JsonConvert.DeserializeObject<Scenario>(text);

        Debug.Log($"Loaded scenario: {scenario.meta.title}");
        Debug.Log($"Schema version: {scenario.schemaVersion}");
    }
}
