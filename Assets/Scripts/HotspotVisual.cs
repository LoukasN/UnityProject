using System.Collections.Generic;

public static class HotspotVisual {
    public static void Apply(string hotspotId, string state) {
        Apply(hotspotId, state, null);
    }

    public static void Apply(string hotspotId, string state, ICollection<string> vitalKeys) {
        if (string.IsNullOrEmpty(hotspotId)) {
            return;
        }

        bool alert = !string.IsNullOrEmpty(state) && state != "normal";

        var blinkers = VitalBlinker.Active;
        for (int i = blinkers.Count - 1; i >= 0; i--) {
            var blinker = blinkers[i];

            if (blinker.HotspotId != hotspotId) {
                continue;
            }

            if (vitalKeys != null && !string.IsNullOrEmpty(blinker.VitalKey) && !vitalKeys.Contains(blinker.VitalKey)) {
                continue;
            }

            blinker.SetAlert(alert);
        }
    }
}
