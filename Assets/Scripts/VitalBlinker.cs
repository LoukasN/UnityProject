using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Blinks one vitals row (or any Graphic) when a scenario rule raises an alarm on it.
// Put one on each row that should be able to alarm and fill in the Inspector fields;
// HotspotVisual finds them by hotspotId, so a scenario alarming on a different vital
// needs no code change, just a blinker on that row.
public class VitalBlinker : MonoBehaviour {
    static readonly List<VitalBlinker> active = new List<VitalBlinker>();
    public static IReadOnlyList<VitalBlinker> Active => active;

    [SerializeField]
    [Tooltip("Must match the hotspot id the JSON rule targets, e.g. \"hs_monitor\".")]
    string hotspotId;
    public string HotspotId => hotspotId;

    [SerializeField]
    [Tooltip("Which vital this row shows (\"spo2\", \"hr\", ...), matching the key in the " +
             "rule's condition. Only rules watching this vital will blink this row. " +
             "Leave blank to blink on any alarm for the hotspot.")]
    string vitalKey;
    public string VitalKey => vitalKey;

    [SerializeField]
    [Tooltip("The row's Text/Image. Leave empty to use the Graphic on this object.")]
    Graphic target;

    [SerializeField]
    Color alertColor = Color.red;

    [SerializeField]
    float blinkInterval = 0.5f;

    Color normalColor;
    bool alerting;
    bool blinkOn;
    float timer;

    void Awake() {
        if (target == null)
            target = GetComponent<Graphic>();

        if (target != null)
            normalColor = target.color;
    }

    void OnEnable() {
        active.Add(this);
        timer = 0f;
        ApplyColor(alerting);
    }

    void OnDisable() {
        active.Remove(this);
        ApplyColor(false);
    }

    public void SetAlert(bool on) {
        if (alerting == on)
            return;

        alerting = on;
        timer = 0f;
        ApplyColor(on);
    }

    void Update() {
        if (!alerting || target == null)
            return;

        timer += Time.deltaTime;
        if (timer < blinkInterval)
            return;

        timer -= blinkInterval;
        ApplyColor(!blinkOn);
    }

    void ApplyColor(bool showAlert) {
        blinkOn = showAlert;

        if (target != null)
            target.color = showAlert ? alertColor : normalColor;
    }
}
