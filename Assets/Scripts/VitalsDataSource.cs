using System.Collections.Generic;
using UnityEngine;
using Pulse.Unity;

namespace DefaultNamespace
{
    [ExecuteInEditMode]
    public class VitalsDataSource : PulseDataSource
    {
        // Field order here must match the dataFieldIndex you set on each
        // PulseDataNumberRenderer in the Inspector.
        const int FIELD_HR = 0;
        const int FIELD_BP_SYSTOLIC = 1;
        const int FIELD_BP_DIASTOLIC = 2;
        const int FIELD_BP_MEAN = 3;
        const int FIELD_SPO2 = 4;
        const int FIELD_RR = 5;
        const int FIELD_TEMP = 6;

        // Current values, kept around so partial updates (a node only
        // changing spo2 and hr, say) can be merged on top instead of
        // needing a full Vitals object every time.
        double currentHr;
        double currentSystolic;
        double currentDiastolic;
        double currentSpo2;
        double currentRr;
        double currentTemp;

        void Awake()
        {
            data = ScriptableObject.CreateInstance<PulseData>();

            data.fields = new StringList();
            data.fields.Add("hr");
            data.fields.Add("bp_systolic");
            data.fields.Add("bp_diastolic");
            data.fields.Add("bp_mean");
            data.fields.Add("spo2");
            data.fields.Add("rr");
            data.fields.Add("temp");

            data.timeStampList = new DoubleList();
            data.valuesTable = new List<DoubleList>
            {
                new DoubleList(), // hr
                new DoubleList(), // bp_systolic
                new DoubleList(), // bp_diastolic
                new DoubleList(), // bp_mean
                new DoubleList(), // spo2
                new DoubleList(), // rr
                new DoubleList(), // temp
            };
        }

        // Called once with the scenario's initial_state.vitals at scene start.
        public void UpdateVitals(Vitals v)
        {
            var (systolic, diastolic) = ParseBp(v.bp);

            currentHr = v.hr;
            currentSystolic = systolic;
            currentDiastolic = diastolic;
            currentSpo2 = v.spo2;
            currentRr = v.rr;
            currentTemp = v.temp;

            PushAll();
        }

        // Called by the rule engine whenever a node/effect only changes some
        // vitals (e.g. a decision's "vitals_update": { "spo2": 85, "hr": 120 }).
        // Any field not present in the dictionary keeps its last value.
        public void ApplyVitalsUpdate(Dictionary<string, int> updates)
        {
            if (updates == null)
                return;

            foreach (var kv in updates)
            {
                switch (kv.Key)
                {
                    case "hr": currentHr = kv.Value; break;
                    case "spo2": currentSpo2 = kv.Value; break;
                    case "rr": currentRr = kv.Value; break;
                    case "temp": currentTemp = kv.Value; break;
                    case "bp_systolic": currentSystolic = kv.Value; break;
                    case "bp_diastolic": currentDiastolic = kv.Value; break;
                    default:
                        Debug.LogWarning($"VitalsDataSource: unknown vitals_update key '{kv.Key}', ignored.");
                        break;
                }
            }

            PushAll();
        }

        public float CurrentHr => (float)currentHr;

        void PushAll()
        {
            double mean = currentDiastolic + (currentSystolic - currentDiastolic) / 3.0;

            data.timeStampList.Clear();
            data.timeStampList.Add(Time.time);

            SetField(FIELD_HR, currentHr);
            SetField(FIELD_BP_SYSTOLIC, currentSystolic);
            SetField(FIELD_BP_DIASTOLIC, currentDiastolic);
            SetField(FIELD_BP_MEAN, mean);
            SetField(FIELD_SPO2, currentSpo2);
            SetField(FIELD_RR, currentRr);
            SetField(FIELD_TEMP, currentTemp);
        }

        void SetField(int fieldIndex, double value)
        {
            data.valuesTable[fieldIndex].Clear();
            data.valuesTable[fieldIndex].Add(value);
        }

        (int systolic, int diastolic) ParseBp(string bp)
        {
            // "125/80" -> 125, 80
            var parts = bp.Split('/');
            return (int.Parse(parts[0]), int.Parse(parts[1]));
        }
    }
}
