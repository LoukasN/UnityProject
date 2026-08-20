using System.Collections.Generic;

public static class HotspotVisual {
    public static void ClearAll() {
        var blinkers = VitalBlinker.Active;
        for (int i = blinkers.Count - 1; i >= 0; i--) {
            blinkers[i].SetAlert(false);
        }
    }

    public static void SetAlarming(string hotspotId, ICollection<string> alarmingVitals, bool alarmAll) {
        if (string.IsNullOrEmpty(hotspotId)) {
            return;
        }

        var blinkers = VitalBlinker.Active;
        for (int i = blinkers.Count - 1; i >= 0; i--) {
            var blinker = blinkers[i];

            if (blinker.HotspotId != hotspotId) {
                continue;
            }

            bool alert;
            if (alarmAll) {
                alert = true;
            } else if (!blinker.HasVitalKeys) {
                alert = alarmingVitals.Count > 0;
            } else {
                alert = blinker.MatchesVital(alarmingVitals);
            }

            blinker.SetAlert(alert);
        }
    }

    public static void Apply(string hotspotId, string state) {
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

            blinker.SetAlert(alert);
        }
    }
}
