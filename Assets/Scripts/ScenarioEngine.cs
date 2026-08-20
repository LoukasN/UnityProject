using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class ScenarioEngine : MonoBehaviour {
    public static ScenarioEngine Instance { get; private set; }

    private Scenario scenario;
    private Node currentNode;
    private int score;
    private Dictionary<string, bool> flags = new Dictionary<string, bool>();

    private HashSet<string> documentedFields = new HashSet<string>();

    private HashSet<string> activeRules = new HashSet<string>();

    private List<string> decisionPath = new List<string>();
    private List<string> logEntries = new List<string>();

    private float timeoutRemaining;
    private bool timeoutArmed;

    public bool TimeoutArmed => timeoutArmed;
    public float TimeoutRemaining => timeoutRemaining;

    [SerializeField]
    private float nodeTransitionDelay = 1.5f;
    [SerializeField]
    private float messageDuration = 3f;
    private Coroutine nodeTransitionCoroutine;
    private Coroutine messageAdvanceCoroutine;

    public Node CurrentNode => currentNode;
    public int Score => score;

    void Awake() {
        Instance = this;
    }

    public void StartScenario(Scenario loadedScenario) {
        scenario = loadedScenario;
        ApplyInitialState();
        GoToNode(scenario.nodes[0].id);
    }

    void ApplyInitialState() {
        score = scenario.initialState.currentScore;

        flags.Clear();
        if (scenario.initialState.flags != null) {
            foreach (var (flagName, value) in scenario.initialState.flags) {
                flags[flagName] = value;
            }
        }

        documentedFields.Clear();
        activeRules.Clear();
        decisionPath.Clear();
        logEntries.Clear();
        timeoutArmed = false;

        var vitalsSource = FindFirstObjectByType<DefaultNamespace.VitalsDataSource>();
        vitalsSource.UpdateVitals(scenario.initialState.vitals);

        ApplyActiveHotspots();

        CheckGlobalRules();
    }

    Node FindNode(string nodeId) {
        foreach (var node in scenario.nodes) {
            if (node.id == nodeId) {
                return node;
            }
        }
        Debug.LogError("Node not found: " + nodeId);
        return null;
    }

    public void GoToNode(string nodeId) {
        StopPendingNodeWork();

        currentNode = FindNode(nodeId);

        if (currentNode == null) {
            return;
        }

        nodeTransitionCoroutine = StartCoroutine(EnterNodeAfterDelay(currentNode));
    }

    public void AbortScenario() {
        StopPendingNodeWork();
        currentNode = null;
        activeRules.Clear();
        HotspotVisual.ClearAll();
    }

    void StopPendingNodeWork() {
        timeoutArmed = false;
        if (nodeTransitionCoroutine != null) {
            StopCoroutine(nodeTransitionCoroutine);
            nodeTransitionCoroutine = null;
        }
        if (messageAdvanceCoroutine != null) {
            StopCoroutine(messageAdvanceCoroutine);
            messageAdvanceCoroutine = null;
        }
    }

    IEnumerator EnterNodeAfterDelay(Node node) {
        yield return new WaitForSeconds(nodeTransitionDelay);

        nodeTransitionCoroutine = null;

        Debug.Log("NODE ENTER: " + node.id);
        Log("NODE_ENTER", node.id);
        EnterNode(node);
    }

    void EnterNode(Node node) {
        timeoutArmed = false;

        if (node.type == "message") {
            messageAdvanceCoroutine = StartCoroutine(AutoAdvanceMessage(node));
        } else if (node.type == "decision") {
            if (node.timeout != null) {
                timeoutRemaining = node.timeout.seconds;
                timeoutArmed = true;
            }
        } else if (node.type == "gate") {
            // Probably nothing here
        } else if (node.type == "end") {
            FinishScenario(node);
        } else {
            Debug.LogWarning("Unknown node type: " + node.type);
        }
    }

    IEnumerator AutoAdvanceMessage(Node node) {
        yield return new WaitForSeconds(messageDuration);

        messageAdvanceCoroutine = null;
        GoToNode(node.nextNodeId);
    }

    [ContextMenu("Continue (message nodes)")]
    public void ContinueFromMessage() {
        if (currentNode != null && currentNode.type == "message") {
            GoToNode(currentNode.nextNodeId);
        }
    }

    public void ChooseOption(Option option) {
        Debug.Log("PATIENT CHOICE CLICKED: " + option.label);
        timeoutArmed = false;
        decisionPath.Add($"{currentNode.text} \n -{option.label}");
        Log("OPTION_SELECTED", option.id);
        ApplyEffects(option.effects);
        GoToNode(option.nextNodeId);
    }

    void Update() {
        if (!timeoutArmed || !GameTimer.Instance.IsRunning) {
            return;
        }

        timeoutRemaining -= Time.deltaTime;
        if (timeoutRemaining <= 0f) {
            timeoutArmed = false;
            decisionPath.Add($"{currentNode.text}\n -Καμία επιλογή (Έληξε το χρονικό όριο).");
            Log("OPTION_SELECTED", "timeout");
            ApplyEffects(currentNode.timeout.onTimeoutEffects);
            GoToNode(currentNode.timeout.nextNodeId);
        }
    }

    void ApplyEffects(Effects effects) {
        if (effects == null) {
            return;
        }

        score += effects.scoreDelta;

        if (!string.IsNullOrEmpty(effects.toast)) {
            UIManager.Instance.ShowToast(effects.toast);
        }

        if (effects.stateUpdate != null) {
            foreach (var (key, value) in effects.stateUpdate) {
                if (key.StartsWith("flags.")) {
                    flags[key.Substring("flags.".Length)] = value;
                } else {
                    Debug.LogWarning("Unknown state_update key: " + key);
                }
            }
        }

        if (effects.vitalsUpdate != null) {
            var vitalsSource = FindFirstObjectByType<DefaultNamespace.VitalsDataSource>();
            vitalsSource.ApplyVitalsUpdate(effects.vitalsUpdate);
            Log("VITALS_CHANGE", JsonConvert.SerializeObject(effects.vitalsUpdate));
            CheckGlobalRules();
        }
    }

    void CheckGlobalRules() {
        if (scenario.rules == null || scenario.rules.globalRules == null) {
            return;
        }

        var alarmingByHotspot = new Dictionary<string, HashSet<string>>();
        var unscopedAlarms = new HashSet<string>();

        foreach (var rule in scenario.rules.globalRules) {
            if (rule.condition == null || rule.effects == null) {
                continue;
            }

            bool conditionsMet = true;
            var watchedVitals = new List<string>();

            foreach (var (conditionKey, operators) in rule.condition) {
                if (conditionKey.StartsWith("vitals.")) {
                    watchedVitals.Add(conditionKey.Substring("vitals.".Length));
                }

                double actual = ReadStateValue(conditionKey);
                foreach (var op in operators.Properties()) {
                    if (!EvaluateCondition(op.Name, actual, (double)op.Value)) {
                        conditionsMet = false;
                    }
                }
            }

            bool wasActive = activeRules.Contains(rule.id);

            if (conditionsMet) {
                activeRules.Add(rule.id);
            } else {
                activeRules.Remove(rule.id);
            }

            foreach (var effect in rule.effects) {
                if (effect.type == "ui_visual") {
                    if (string.IsNullOrEmpty(effect.target)) {
                        continue;
                    }

                    if (!alarmingByHotspot.TryGetValue(effect.target, out var vitals)) {
                        vitals = new HashSet<string>();
                        alarmingByHotspot[effect.target] = vitals;
                    }

                    if (!conditionsMet || effect.state == "normal") {
                        continue;
                    }

                    if (watchedVitals.Count == 0) {
                        unscopedAlarms.Add(effect.target);
                    }

                    foreach (var vital in watchedVitals) {
                        vitals.Add(vital);
                    }
                } else if (conditionsMet && !wasActive) {
                    ApplyRuleEffect(effect);
                }
            }
        }

        foreach (var (hotspotId, vitals) in alarmingByHotspot) {
            HotspotVisual.SetAlarming(hotspotId, vitals, unscopedAlarms.Contains(hotspotId));
        }
    }

    bool EvaluateCondition(string op, double actual, double threshold) {
        switch (op) {
        case "lt":
            return actual < threshold;
        case "lte":
            return actual <= threshold;
        case "gt":
            return actual > threshold;
        case "gte":
            return actual >= threshold;
        case "eq":
            return Mathf.Approximately((float)actual, (float)threshold);
        case "neq":
            return !Mathf.Approximately((float)actual, (float)threshold);
        }
        Debug.LogWarning("Unknown condition operator: " + op);
        return false;
    }

    double ReadStateValue(string key) {
        if (key.StartsWith("vitals.")) {
            var vitalsSource = FindFirstObjectByType<DefaultNamespace.VitalsDataSource>();
            return vitalsSource.GetVital(key.Substring("vitals.".Length));
        }
        if (key.StartsWith("flags.")) {
            string flagName = key.Substring("flags.".Length);
            return flags.ContainsKey(flagName) && flags[flagName] ? 1 : 0;
        }
        Debug.LogWarning("Unknown condition key: " + key);
        return 0;
    }

    void ApplyRuleEffect(Effect effect) {
        if (effect.type == "ui_toast") {
            UIManager.Instance.ShowToast(effect.message, effect.style == "danger");
        } else {
            Debug.LogWarning("Unknown rule effect type: " + effect.type);
        }
    }

    public void OnEhrSubmit(List<string> filledFieldKeys) {
        foreach (var key in filledFieldKeys) {
            documentedFields.Add(key);
        }
        Log("EHR_SUBMIT", string.Join(", ", filledFieldKeys));

        if (currentNode != null && currentNode.type == "gate") {
            TryPassGate(currentNode, filledFieldKeys);
        }
    }

    string DescribeField(string formId, string fieldId) {
        Form form = null;

        if (scenario.ehrConfig != null &&
            scenario.ehrConfig.forms != null &&
            scenario.ehrConfig.forms.TryGetValue(formId, out Form foundForm)) {
            form = foundForm;
        }

        if (form != null && form.fields != null && form.fields.Contains(fieldId)) {
            return form.title + " => " + fieldId;
        }

        return formId + "." + fieldId;
    }

    void TryPassGate(Node gate, List<string> submittedFieldKeys) {
        if (gate.gateRequirements?.requiredForms == null) {
            ApplyEffects(gate.effectsOnPass);
            GoToNode(gate.nextNodeId);
            return;
        }

        foreach (var required in gate.gateRequirements.requiredForms) {
            foreach (var field in required.fields) {
                if (!submittedFieldKeys.Contains(required.formId + "." + field)) {
                    UIManager.Instance.ShowToast(gate.feedbackBlocked);
                    return;
                }
            }
        }

        if (!string.IsNullOrEmpty(gate.feedbackSuccess)) {
            UIManager.Instance.ShowToast(gate.feedbackSuccess);
        }
        UIManager.Instance.CloseEHR();
        ApplyEffects(gate.effectsOnPass);
        GoToNode(gate.nextNodeId);
    }

    void ApplyActiveHotspots() {
        var ui = scenario.initialState.ui;
        if (ui == null) {
            return;
        }

        var interactables = FindObjectsByType<DefaultNamespace.GenericInteractable>(FindObjectsSortMode.None);
        foreach (var interactable in interactables) {
            bool isActive = ui.activeHotspots != null && ui.activeHotspots.Contains(interactable.HotspotId);
            interactable.SetInteractionEnabled(isActive);
        }

        if (ui.monitorAlert) {
            HotspotVisual.Apply("hs_monitor", "blinking_red");
        }
    }

    public void LogHotspot(string hotspotId) {
        Log("HOTSPOT_INTERACTION", hotspotId);
    }

    void FinishScenario(Node node) {
        var debrief = node.debriefConfig;
        var missedDocs = new List<string>();
        if (debrief != null && debrief.highlightMissedDocs) {
            foreach (var n in scenario.nodes) {
                if (n.type != "gate" || n.gateRequirements?.requiredForms == null) {
                    continue;
                }
                foreach (var required in n.gateRequirements.requiredForms) {
                    foreach (var field in required.fields) {
                        if (!documentedFields.Contains(required.formId + "." + field)) {
                            missedDocs.Add(DescribeField(required.formId, field));
                        }
                    }
                }
            }
        }

        var shownPath = new List<string>();
        if (debrief != null && debrief.showDecisionPath) {
            shownPath = decisionPath;
        }

        if (debrief != null && debrief.exportLog) {
            ExportLog();
        }

        UIManager.Instance.ShowDebrief(node.text, score, shownPath, missedDocs);
    }

    void Log(string eventType, string detail) {
        if (scenario == null || scenario.logging == null || !scenario.logging.enabled) {
            return;
        }
        if (scenario.logging.logEvents == null || !scenario.logging.logEvents.Contains(eventType)) {
            return;
        }

        logEntries.Add($"{GameTimer.Instance.GetFormattedTime()} | {eventType} | {detail}");
    }

    void ExportLog() {
        string path = Path.Combine(Application.persistentDataPath, scenario.meta.id + "_log.json");
        File.WriteAllText(path, JsonConvert.SerializeObject(logEntries, Formatting.Indented));
        Debug.Log("Action log exported to: " + path);
    }
}
