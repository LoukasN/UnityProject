using UnityEngine;
using TMPro;

public class TimerDisplay : MonoBehaviour {
    [SerializeField]
    private TMP_Text timerText;

    void Update() {
        if (GameTimer.Instance != null) {
            timerText.text = GameTimer.Instance.GetFormattedTime();
        }
    }
}
