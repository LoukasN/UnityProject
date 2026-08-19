public static class ScenarioLookup {
    public static Option GetOptionForHotspot(Node node, string hotspotId) {
        if (node?.options == null || hotspotId == null) {
            return null;
        }
        return node.options.Find(o => o.targetHotspot == hotspotId);
    }

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
