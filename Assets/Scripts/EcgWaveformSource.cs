using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Pulse.Unity;

namespace DefaultNamespace {
[ExecuteInEditMode]
public class EcgWaveformSource : PulseDataSource {
    const int FIELD_ECG = 0;

    [Tooltip("Drag Assets/PulsePhysiologyEngine/Data/ecg/StandardECG.json here.")]
    public TextAsset ecgJson;

    [Tooltip("Fallback pacing only, for the few frames before the scenario JSON has parsed " +
             "(or if this scene is opened on its own with no VitalsDataSource). The scenario's " +
             "real hr takes over as soon as it loads, whatever it is.")]
    public float fallbackHr = 75f;

    List<double> samples;
    int sampleIndex;
    float timer;
    float currentHr;
    VitalsDataSource vitals;

    void Start() {
        vitals = FindFirstObjectByType<VitalsDataSource>();
    }

    void Awake() {
        data = ScriptableObject.CreateInstance<PulseData>();
        data.fields = new StringList();
        data.fields.Add("ecg");

        data.timeStampList = new DoubleList();
        data.valuesTable = new List<DoubleList> { new DoubleList() };

        currentHr = fallbackHr;
        LoadSamples();
    }

    public void SetHeartRate(float hr) {
        if (hr > 0)
            currentHr = hr;
    }

    void Update() {
        if (!Application.isPlaying || samples == null || samples.Count == 0)
            return;

        if (UIManager.Instance != null && UIManager.Instance.IsPaused)
            return; // frozen while the pause menu is open

        if (vitals != null) {
            if (vitals.CurrentHr <= 0)
                return; // scenario not loaded yet - stay frozen instead of animating on fallbackHr
            SetHeartRate(vitals.CurrentHr);
        }

        float samplePeriod = (60f / currentHr) / samples.Count;

        timer += Time.deltaTime;
        while (timer >= samplePeriod) {
            timer -= samplePeriod;

            data.timeStampList.Clear();
            data.timeStampList.Add(Time.time);
            data.valuesTable[FIELD_ECG].Clear();
            data.valuesTable[FIELD_ECG].Add(samples[sampleIndex]);

            sampleIndex = (sampleIndex + 1) % samples.Count;
        }
    }

    void LoadSamples() {
        samples = null;
        if (ecgJson == null)
            return;

        var root = JObject.Parse(ecgJson.text);
        foreach (var waveform in (JArray)root["Waveforms"]) {
            if (waveform["Type"] != null)
                continue;

            samples = waveform["OriginalData"]["ArrayElectricPotential"]["Value"]["Value"]
                          .ToObject<List<double>>();
            return;
        }

        Debug.LogWarning("EcgWaveformSource: no waveform without a 'Type' (normal rhythm) found in " + ecgJson.name);
    }

#if UNITY_EDITOR
    void OnValidate() {
        if (data != null)
            LoadSamples();
    }
#endif
}
}
