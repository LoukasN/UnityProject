using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class ScenarioLoader : MonoBehaviour {
    private Scenario scenario;

    void Start() {
        Debug.Log("Starting Scenario...");
        string path = Path.Combine(Application.streamingAssetsPath, "decisionTree.json");
        
        if (!File.Exists(path)) {
            Debug.LogError("file not found: " + path);
            return;
        }

        string text = File.ReadAllText(path);
        scenario = JsonConvert.DeserializeObject<Scenario>(text);

        Debug.Log($"Loaded scenario: {scenario.meta.title}");
        Debug.Log($"Schema version: {scenario.schemaVersion}");
        Debug.Log($"Initial Hr: {scenario.initialState.vitals.hr}");
        Debug.Log($"Initial SpO2: {scenario.initialState.vitals.spo2}");
        Debug.Log($"Initial RR: {scenario.initialState.vitals.rr}");
        Debug.Log($"Initial Bp: {scenario.initialState.vitals.bp}");
        Debug.Log($"Initial temp: {scenario.initialState.vitals.temp}");
        Debug.Log($"Nodes count: {scenario.nodes.Count}");
        // Debug.Log($"Test: {scenario.nodes[0].id}");
        // Debug.Log($"Test1: {scenario.rules.globalRules[0].condition["vitals.spo2"]}");
        // Debug.Log($"Test1: {scenario.rules.globalRules[0].condition.ContainsKey()}");
        // Debug.Log($"Test2: {scenario.rules.globalRules[0].condition["vitals.hr"]}");
        // Debug.Log($"Test3: {scenario.rules.globalRules[0].condition["vitals.bp"]}");
    }
}
