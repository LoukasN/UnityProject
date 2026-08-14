using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Pulse.Unity;

namespace DefaultNamespace
{
    // Drives the ECG line renderer by replaying a recorded waveform (from
    // StandardECG.json) in a loop, paced to a heart rate. Only the "normal"
    // (no "Type") waveform is used for now — VFib/VTach swapping is parked.
    [ExecuteInEditMode]
    public class EcgWaveformSource : PulseDataSource
    {
        const int FIELD_ECG = 0;

        [Tooltip("Drag Assets/PulsePhysiologyEngine/Data/ecg/StandardECG.json here.")]
        public TextAsset ecgJson;

        [Tooltip("Starting heart rate used to pace playback until something calls SetHeartRate().")]
        public float startingHr = 75f;

        List<double> samples;
        int sampleIndex;
        float timer;
        float currentHr;

        void Awake()
        {
            data = ScriptableObject.CreateInstance<PulseData>();
            data.fields = new StringList();
            data.fields.Add("ecg");

            data.timeStampList = new DoubleList();
            data.valuesTable = new List<DoubleList> { new DoubleList() };

            currentHr = startingHr;
            LoadSamples();
        }

        // Call this from the rule engine (or wire it to VitalsDataSource.CurrentHr)
        // to make the waveform speed up/slow down as HR changes mid-scenario.
        public void SetHeartRate(float hr)
        {
            if (hr > 0)
                currentHr = hr;
        }

        void Update()
        {
            if (!Application.isPlaying || samples == null || samples.Count == 0)
                return;

            float samplePeriod = (60f / currentHr) / samples.Count;

            timer += Time.deltaTime;
            while (timer >= samplePeriod)
            {
                timer -= samplePeriod;

                data.timeStampList.Clear();
                data.timeStampList.Add(Time.time);
                data.valuesTable[FIELD_ECG].Clear();
                data.valuesTable[FIELD_ECG].Add(samples[sampleIndex]);

                sampleIndex = (sampleIndex + 1) % samples.Count;
            }
        }

        void LoadSamples()
        {
            samples = null;
            if (ecgJson == null)
                return;

            var root = JObject.Parse(ecgJson.text);
            foreach (var waveform in (JArray)root["Waveforms"])
            {
                // The normal sinus rhythm waveform is the one with no "Type".
                if (waveform["Type"] != null)
                    continue;

                samples = waveform["OriginalData"]["ArrayElectricPotential"]["Value"]["Value"]
                    .ToObject<List<double>>();
                return;
            }

            Debug.LogWarning("EcgWaveformSource: no waveform without a 'Type' (normal rhythm) found in " + ecgJson.name);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (data != null)
                LoadSamples();
        }
#endif
    }
}
