using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Pulse.Unity;

namespace DefaultNamespace {
public class EcgWaveformSource : PulseDataSource {

    const int FIELD_ECG = 0;
    public TextAsset ecgJson;
    public float fallbackHr = 75f;

    List<double> samples;
    int sampleIndex;
    float timer;
    float currentHr;
    VitalsDataSource vitals;
    double clock;

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

        if (UIManager.Instance != null && !UIManager.Instance.GameplayActive)
            return;

        if (vitals != null) {
            if (vitals.CurrentHr <= 0)
                return;
            SetHeartRate(vitals.CurrentHr);
        }

        float samplePeriod = (60f / currentHr) / samples.Count;

        timer += Time.deltaTime;
        clock += Time.deltaTime;
        while (timer >= samplePeriod) {
            timer -= samplePeriod;
            data.timeStampList.Clear();
            data.timeStampList.Add(clock);
            data.valuesTable[FIELD_ECG].Clear();
            data.valuesTable[FIELD_ECG].Add(samples[sampleIndex]);
            sampleIndex = (sampleIndex + 1) % samples.Count;
        }
    }

    void LoadSamples() {
        samples = null;
        if (ecgJson == null) {
            return;
        }
        var root = JObject.Parse(ecgJson.text);
        foreach (var waveform in (JArray)root["Waveforms"]) {
            if (waveform["Type"] != null) {
                continue;
            }
            samples = waveform["OriginalData"]["ArrayElectricPotential"]["Value"]["Value"].ToObject<List<double>>();
            return;
        }
    }
}
}
