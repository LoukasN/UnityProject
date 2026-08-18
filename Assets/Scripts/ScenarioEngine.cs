using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json;

// Walks the decision tree of the loaded scenario. Everything here is generic:
// it only reads the JSON's structure (node types, effects, rules), never a
// specific scenario's ids — so a new scenario is a new JSON file, zero new code.
public class ScenarioEngine : MonoBehaviour {
    public static ScenarioEngine Instance { get; private set; }

    private Scenario scenario;
    private Node currentNode;
    private int score;
    private Dictionary<string, bool> flags = new Dictionary<string, bool>();

    // "formId.field" for every EHR field the user has saved. A HashSet because
    // we only ever ask "was this documented?" — no order, no duplicates.
    private HashSet<string> documentedFields = new HashSet<string>();

    // Ids of global rules whose condition is currently true, so a rule fires
    // once when its condition BECOMES true instead of on every vitals change.
    private HashSet<string> activeRules = new HashSet<string>();

    private List<string> decisionPath = new List<string>();
    private List<string> logEntries = new List<string>();

    private float timeoutRemaining;
    private bool timeoutArmed;

    public Node CurrentNode => currentNode;
    public int Score => score;

    void Awake() {
        Instance = this;
    }

    // Called by UIManager.CloseStartScreen when the player presses Start.
    // Calling it again later IS the reset: it re-applies initial_state.
    public void StartScenario(Scenario loadedScenario) {
        scenario = loadedScenario;
        ApplyInitialState();
        GoToNode(scenario.nodes[0].id);
    }

    void ApplyInitialState() {
        score = scenario.initialState.currentScore;

        // Copy the flags instead of pointing at the dictionary inside the
        // scenario, so a reset starts from the untouched original values.
        flags.Clear();
        foreach (var (flagName, value) in scenario.initialState.flags) {
            flags[flagName] = value;
        }

        documentedFields.Clear();
        activeRules.Clear();
        decisionPath.Clear();
        logEntries.Clear();
        timeoutArmed = false;

        var vitalsSource = FindFirstObjectByType<DefaultNamespace.VitalsDataSource>();
        vitalsSource.UpdateVitals(scenario.initialState.vitals);

        ApplyActiveHotspots();

        // The scenario may already start in an alarm state (template: spo2 88).
        CheckGlobalRules();
    }

    // ---------- walking the tree ----------

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
        currentNode = FindNode(nodeId);
        Debug.Log(currentNode);
        if (currentNode == null) {
            return;
        }

        Log("NODE_ENTER", currentNode.id);
        EnterNode(currentNode);
    }

    // The one place that decides what each node type means.
    void EnterNode(Node node) {
        timeoutArmed = false;

        if (node.type == "message") {
            // Waits for the Continue button (UIManager.OnContinueClicked),
            // otherwise the text would be replaced before anyone reads it.
        }
        else if (node.type == "decision") {
            if (node.timeout != null) {
                timeoutRemaining = node.timeout.seconds;
                timeoutArmed = true;
            }
            // Now we wait: GenericInteractable calls ChooseOption().
        }
        else if (node.type == "gate") {
            // Now we wait: the EHR Save button calls OnEhrSubmit().
        }
        else if (node.type == "end") {
            FinishScenario(node);
        }
        else {
            Debug.LogWarning("Unknown node type: " + node.type);
        }
    }

    // ContextMenu makes this callable by right-clicking the component in the
    // Inspector during Play, so message nodes can be passed before the node
    // panel UI is wired up.
    [ContextMenu("Continue (message nodes)")]
    public void ContinueFromMessage() {
        if (currentNode != null && currentNode.type == "message") {
            GoToNode(currentNode.nextNodeId);
        }
    }

    // ---------- decisions ----------

    public void ChooseOption(Option option) {
        timeoutArmed = false;
        decisionPath.Add($"{currentNode.id}: {option.label}");
        Log("OPTION_SELECTED", option.id);
        ApplyEffects(option.effects);
        GoToNode(option.nextNodeId);
    }

    void Update() {
        // Enter advances message nodes: in first-person mode the cursor is
        // locked, so the on-screen Continue button alone would be unreachable.
        // Only while the cursor IS locked — otherwise typing Enter into an
        // EHR input field would silently skip message nodes.
        if (currentNode != null && currentNode.type == "message"
            && Cursor.lockState == CursorLockMode.Locked
            && Keyboard.current != null
            && (Keyboard.current.enterKey.wasPressedThisFrame
                || Keyboard.current.numpadEnterKey.wasPressedThisFrame)) {
            ContinueFromMessage();
        }

        // Timeout countdown for decision nodes (the scenario's χρονικός κανόνας).
        if (!timeoutArmed || !GameTimer.Instance.IsRunning) {
            return;
        }

        timeoutRemaining -= Time.deltaTime;
        if (timeoutRemaining <= 0f) {
            timeoutArmed = false;
            decisionPath.Add($"{currentNode.id}: (timeout)");
            Log("OPTION_SELECTED", "timeout");
            ApplyEffects(currentNode.timeout.onTimeoutEffects);
            GoToNode(currentNode.timeout.nextNodeId);
        }
    }

    // ---------- effects (options, timeouts and gates all share this shape) ----------

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
                // Keys look like "flags.assessment_complete".
                if (key.StartsWith("flags.")) {
                    flags[key.Substring("flags.".Length)] = value;
                }
                else {
                    Debug.LogWarning("Unknown state_update key: " + key);
                }
            }
        }

        if (effects.vitalsUpdate != null) {
            var vitalsSource = FindFirstObjectByType<DefaultNamespace.VitalsDataSource>();
            vitalsSource.ApplyVitalsUpdate(effects.vitalsUpdate);
            Log("VITALS_CHANGE", JsonConvert.SerializeObject(effects.vitalsUpdate));
            CheckGlobalRules(); // vitals moved, rules may turn on or off
        }
    }

    // ---------- global rules ----------

    void CheckGlobalRules() {
        if (scenario.rules == null || scenario.rules.globalRules == null) {
            return;
        }

        foreach (var rule in scenario.rules.globalRules) {
            if (rule.condition == null) {
                continue;
            }

            bool conditionsMet = true;
            foreach (var (conditionKey, operators) in rule.condition) {
                // conditionKey: "vitals.spo2"    operators: { "lt": 90 }
                double actual = ReadStateValue(conditionKey);
                foreach (var op in operators.Properties()) {
                    if (!EvaluateCondition(op.Name, actual, (double)op.Value)) {
                        conditionsMet = false;
                    }
                }
            }

            if (conditionsMet && !activeRules.Contains(rule.id)) {
                activeRules.Add(rule.id);
                foreach (var effect in rule.effects) {
                    ApplyRuleEffect(effect);
                }
            }
            else if (!conditionsMet && activeRules.Contains(rule.id)) {
                // Condition cleared (spo2 back over 90): stop the visual alarm.
                activeRules.Remove(rule.id);
                foreach (var effect in rule.effects) {
                    if (effect.type == "ui_visual") {
                        // HotspotVisual.Apply(effect.target, "normal");
                    }
                }
            }
        }
    }

    bool EvaluateCondition(string op, double actual, double threshold) {
        if (op == "lt") return actual < threshold;
        if (op == "lte") return actual <= threshold;
        if (op == "gt") return actual > threshold;
        if (op == "gte") return actual >= threshold;
        if (op == "eq") return actual == threshold;
        if (op == "neq") return actual != threshold;
        Debug.LogWarning("Unknown condition operator: " + op);
        return false;
    }

    // Turns a condition key from the JSON into the live value it refers to.
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

    // Rule effects have their own shape (type/target/state/message),
    // different from node effects — hence a second method.
    void ApplyRuleEffect(Effect effect) {
        if (effect.type == "ui_visual") {
            // HotspotVisual.Apply(effect.target, effect.state);
        }
        else if (effect.type == "ui_toast") {
            UIManager.Instance.ShowToast(effect.message);
        }
        else {
            Debug.LogWarning("Unknown rule effect type: " + effect.type);
        }
    }

    // ---------- EHR + gates ----------

    // Called by EhrFormsPanel's Save button with every "formId.field" the user
    // has filled in. Documentation is remembered even when it is saved before
    // any gate asks for it — like a real chart.
    public void OnEhrSubmit(List<string> filledFieldKeys) {
        foreach (var key in filledFieldKeys) {
            documentedFields.Add(key);
        }
        Log("EHR_SUBMIT", string.Join(", ", filledFieldKeys));

        if (currentNode != null && currentNode.type == "gate") {
            TryPassGate(currentNode);
        }
    }

    string DescribeField(string formId, string fieldId){
        Form form = null;
    
        if (scenario.ehrConfig != null &&
            scenario.ehrConfig.forms != null &&
            scenario.ehrConfig.forms.TryGetValue(formId, out Form foundForm)){
            form = foundForm;
        }
    
        if (form != null && form.fields != null && form.fields.Contains(fieldId)) {
            return form.title + " => " + fieldId;
        }
    
        return formId + "." + fieldId;
    }

    void TryPassGate(Node gate) {
        var missing = new List<string>();
        foreach (var required in gate.gateRequirements.requiredForms) {
            foreach (var field in required.fields) {
                if (!documentedFields.Contains(required.formId + "." + field)) {
                    missing.Add(DescribeField(required.formId, field));
                }
            }
        }

        // Naming what is missing is the difference between "something is
        // wrong" and "here is what to fix".
        if (missing.Count > 0) {
            UIManager.Instance.ShowToast(gate.feedbackBlocked + "\nΛείπει: " + string.Join(", ", missing));
            return;
        }

        if (!string.IsNullOrEmpty(gate.feedbackSuccess)) {
            UIManager.Instance.ShowToast(gate.feedbackSuccess);
        }
        // Documentation accepted: hand the player back to the room so the
        // next node is visible immediately (the EHR can be reopened anytime).
        UIManager.Instance.CloseEHR();
        ApplyEffects(gate.effectsOnPass);
        GoToNode(gate.nextNodeId);
    }

    // Test helper ONLY (remove or ignore for the final demo): pretends the
    // EHR fields of the current gate were filled and saved, so the flow can
    // be tested before the EHR form UI is built. Right-click the component
    // header during Play to use it.
    [ContextMenu("DEBUG: fill current gate's required fields")]
    void DebugFillCurrentGate() {
        if (currentNode == null || currentNode.type != "gate") {
            Debug.Log("DEBUG: not on a gate node right now.");
            return;
        }

        var keys = new List<string>();
        foreach (var required in currentNode.gateRequirements.requiredForms) {
            foreach (var field in required.fields) {
                keys.Add(required.formId + "." + field);
            }
        }
        OnEhrSubmit(keys);
    }

    // ---------- hotspots ----------

    void ApplyActiveHotspots() {
        var interactables = FindObjectsByType<DefaultNamespace.GenericInteractable>(FindObjectsSortMode.None);
        foreach (var interactable in interactables) {
            bool isActive = scenario.initialState.ui.activeHotspots.Contains(interactable.HotspotId);
            interactable.SetInteractionEnabled(isActive);
        }

        if (scenario.initialState.ui.monitorAlert) {
            // HotspotVisual.Apply("hs_monitor", "blinking_red");
        }
    }

    public void LogHotspot(string hotspotId) {
        Log("HOTSPOT_INTERACTION", hotspotId);
    }

    // ---------- end of scenario ----------

    void FinishScenario(Node node) {
        var debrief = node.debriefConfig;

        // "Missed docs" checks EVERY gate in the scenario, not only visited
        // ones: skipping the escalation branch also skips its documentation,
        // and the debrief must point that out.
        var missedDocs = new List<string>();
        if (debrief != null && debrief.highlightMissedDocs) {
            foreach (var n in scenario.nodes) {
                if (n.type != "gate") {
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

    // ---------- logging ----------

    void Log(string eventType, string detail) {
    return;
        // if (scenario == null || scenario.logging == null || !scenario.logging.enabled) {
        //     return;
        // }
        // if (!scenario.logging.logEvents.Contains(eventType)) {
        //     return;
        // }
        //
        // logEntries.Add($"{GameTimer.Instance.GetFormattedTime()} | {eventType} | {detail}");
    }

    void ExportLog() {
    return;
        // string path = Path.Combine(Application.persistentDataPath, scenario.meta.id + "_log.json");
        // File.WriteAllText(path, JsonConvert.SerializeObject(logEntries, Formatting.Indented));
        // Debug.Log("Action log exported to: " + path);
    }
}
