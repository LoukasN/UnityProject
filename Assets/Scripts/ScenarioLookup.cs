public static class ScenarioLookup {
    public static Option GetOptionForHotspot(Node node, string hotspotId) {
        if (node?.options == null || hotspotId == null) {
            return null;
        }
        return node.options.Find(o => o.targetHotspot == hotspotId);
    }

    // A hotspot is "active" if the current node points at it in any way:
    // a decision node via an option's target_hotspot, or a gate node via
    // gate_requirements.target_hotspot. Gates have no options, so an
    // options-only check leaves the EHR unlit for the whole gate.
    public static bool IsHotspotActive(Node node, string hotspotId) {
        if (node == null || hotspotId == null) {
            return false;
        }
        if (GetOptionForHotspot(node, hotspotId) != null) {
            return true;
        }
        return node.gateRequirements != null
            && node.gateRequirements.targetHotspot == hotspotId;
    }
}
