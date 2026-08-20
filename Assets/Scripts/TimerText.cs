using UnityEngine;
using TMPro;

public class TimerDisplay : MonoBehaviour {
    [SerializeField]
    private TMP_Text timerText;

    void Update() {
        ScenarioEngine engine = ScenarioEngine.Instance;
        bool armed = engine != null && engine.TimeoutArmed;

        if (timerText.enabled != armed) {
            timerText.enabled = armed;
        }

        if (!armed) {
            return;
        }

        float remaining = Mathf.Max(0f, engine.TimeoutRemaining);
        timerText.text = $"{Mathf.FloorToInt(remaining / 60f):00}:{Mathf.FloorToInt(remaining % 60f):00}";
    }
}
