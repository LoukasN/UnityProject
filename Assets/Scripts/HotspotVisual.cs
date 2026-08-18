using System.Collections.Generic;

// Sink for the JSON's "ui_visual" rule effects. ScenarioEngine calls Apply(...) without
// knowing what is on screen; this routes it to the VitalBlinkers registered under that
// hotspot id. Global namespace so ScenarioEngine can call it unqualified.
public static class HotspotVisual {
    // Whole-hotspot alarm: every blinker under this hotspot, whatever vital it shows.
    public static void Apply(string hotspotId, string state) {
        Apply(hotspotId, state, null);
    }

    // Scoped alarm: only the blinkers showing one of vitalKeys. A blinker that was left
    // with a blank Vital Key acts as a wildcard and always responds, so a monitor with a
    // single "alarm" element still works without per-vital wiring.
    public static void Apply(string hotspotId, string state, ICollection<string> vitalKeys) {
        if (string.IsNullOrEmpty(hotspotId))
            return;

        // The schema uses "blinking_red" to raise and "normal" to clear.
        bool alert = !string.IsNullOrEmpty(state) && state != "normal";

        var blinkers = VitalBlinker.Active;
        for (int i = blinkers.Count - 1; i >= 0; i--) {
            var blinker = blinkers[i];

            if (blinker.HotspotId != hotspotId)
                continue;

            if (vitalKeys != null &&
                !string.IsNullOrEmpty(blinker.VitalKey) &&
                !vitalKeys.Contains(blinker.VitalKey))
                continue;

            blinker.SetAlert(alert);
        }
    }
}
