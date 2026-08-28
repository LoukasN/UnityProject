using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class Scenario {
    [JsonProperty("schema_version")]
    public string schemaVersion;

    [JsonProperty("scenario_meta")]
    public Meta meta;

    [JsonProperty("initial_state")]
    public InitialState initialState;

    [JsonProperty("hotspots")]
    public List<Hotspot> hotspots;

    [JsonProperty("ehr_config")]
    public EhrConfig ehrConfig;

    public Rules rules;

    public List<Node> nodes;

    public Logging logging;
}

public class Meta {
    public string id;
    public string title;
    public string description;

    [JsonProperty("estimated_duration_minutes")]
    public int estimatedDurationMinutes;

    public string difficulty;

    [JsonProperty("learning_goals")]
    public List<string> learningGoals;
}

public class InitialState {
    [JsonProperty("time_elapsed")]
    public int timeElapsed;

    [JsonProperty("current_score")]
    public int currentScore;

    public Dictionary<string, bool> flags;
    public Vitals vitals;
    public Ui ui;
}

public class Vitals {
    public int hr;
    public int spo2;
    public int rr;
    public string bp;
    public double temp;
}

public class Ui {
    [JsonProperty("active_hotspots")]
    public List<string> activeHotspots;

    [JsonProperty("monitor_alert")]
    public bool monitorAlert;
}

public class Hotspot {
    public string id;
    public string label;
}

public class EhrConfig {
    public Dictionary<string, Form> forms;
}

public class Form {
    public string title;
    public List<string> fields;
}

public class Rules {
    [JsonProperty("global_rules")]
    public List<GlobalRule> globalRules;
}

public class GlobalRule {
    public string id;
    public Dictionary<string, JObject> condition;
    public List<Effect> effects;
}

public class Effect {
    public string type;
    // Any of the below can be null
    public string target;
    public string state;
    public string style;
    public string message;
}

public class Node {
    public string id;
    public string type;
    public string text;
    // Only on gates
    public string description;

    [JsonProperty("next_node_id")]
    public string nextNodeId;

    public List<Option> options;
    public Timeout timeout;

    [JsonProperty("gate_requirements")]
    public GateRequirements gateRequirements;

    [JsonProperty("feedback_blocked")]
    public string feedbackBlocked;

    [JsonProperty("feedback_success")]
    public string feedbackSuccess;

    [JsonProperty("effects_on_pass")]
    public Effects effectsOnPass;

    [JsonProperty("debrief_config")]
    public DebriefConfig debriefConfig;
}

public class Option {
    public string id;
    public string label;

    [JsonProperty("target_hotspot")]
    public string targetHotspot;

    public Effects effects;

    [JsonProperty("next_node_id")]
    public string nextNodeId;
}

public class Effects {
    [JsonProperty("score_delta")]
    public int scoreDelta;

    public string toast;

    [JsonProperty("state_update")]
    public Dictionary<string, bool> stateUpdate;

    [JsonProperty("vitals_update")]
    public Dictionary<string, JValue> vitalsUpdate;
}

public class Timeout {
    public int seconds;

    [JsonProperty("on_timeout_effects")]
    public Effects onTimeoutEffects;

    [JsonProperty("next_node_id")]
    public string nextNodeId;
}

public class GateRequirements {
    [JsonProperty("target_hotspot")]
    public string targetHotspot;

    [JsonProperty("required_forms")]
    public List<RequiredForm> requiredForms;
}

public class RequiredForm {
    [JsonProperty("form_id")]
    public string formId;

    public List<string> fields;
}

public class DebriefConfig {
    [JsonProperty("show_score")]
    public bool showScore;

    [JsonProperty("show_decision_path")]
    public bool showDecisionPath;

    [JsonProperty("highlight_missed_docs")]
    public bool highlightMissedDocs;

    [JsonProperty("export_log")]
    public bool exportLog;
}

public class Logging {
    public bool enabled;

    [JsonProperty("log_events")]
    public List<string> logEvents;

    [JsonProperty("export_format")]
    public string exportFormat;
}
