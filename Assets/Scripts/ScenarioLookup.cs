public static class ScenarioLookup {
    public static Option GetOptionForHotspot(Node node, string hotspotId) {
        if (node?.options == null || hotspotId == null) {
            return null;
        }
        return node.options.Find(o => o.targetHotspot == hotspotId);
    }
}
