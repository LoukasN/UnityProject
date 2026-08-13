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
        Debug.Log($"Test: {scenario.nodes[0].id}");
        foreach (var node in scenario.nodes) {
            if (node.id == "n7_gate_documentation_2") {
                Debug.Log($"From Id: {node.gateRequirements.requiredForms[0].formId}");
                foreach (var form in node.gateRequirements.requiredForms[0].fields) {
                    Debug.Log($"From fields: {form}");
                }
            }
        }

        if (scenario.rules.globalRules[0].effects[0].type == "ui_visual"){
            Debug.Log($"{scenario.rules.globalRules[0].effects[0].type}");
            Debug.Log($"{scenario.rules.globalRules[0].effects[0].target}");
            Debug.Log($"{scenario.rules.globalRules[0].effects[0].state}");
        }

        if (scenario.rules.globalRules[0].effects[1].type == "ui_toast"){
            Debug.Log($"{scenario.rules.globalRules[0].effects[1].type}");
            Debug.Log($"{scenario.rules.globalRules[0].effects[1].style}");
            Debug.Log($"{scenario.rules.globalRules[0].effects[1].message}");
        }
        
    }
}
