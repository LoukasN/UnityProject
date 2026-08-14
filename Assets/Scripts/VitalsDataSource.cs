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

        // Called by whoever drives the scenario (rule engine / game logic)
        // whenever vitals change.
        public void UpdateVitals(Vitals v)
        {
            var (systolic, diastolic) = ParseBp(v.bp);
            double mean = diastolic + (systolic - diastolic) / 3.0;

            data.timeStampList.Clear();
            data.timeStampList.Add(Time.time);

            SetField(FIELD_HR, v.hr);
            SetField(FIELD_BP_SYSTOLIC, systolic);
            SetField(FIELD_BP_DIASTOLIC, diastolic);
            SetField(FIELD_BP_MEAN, mean);
            SetField(FIELD_SPO2, v.spo2);
            SetField(FIELD_RR, v.rr);
            SetField(FIELD_TEMP, v.temp);
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
