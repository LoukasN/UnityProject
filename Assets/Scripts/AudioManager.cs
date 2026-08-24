using UnityEngine;

public class AudioManager : MonoBehaviour {
    public static AudioManager Instance { get; private set; }

    [SerializeField]
    AudioSource heartbeatSource;

    [SerializeField]
    AudioSource alarmSource;

    bool heartbeatStarted;
    bool alarmStarted;
    bool alarmRequested;

    void Awake() {
        Instance = this;
    }

    void Update() {
        bool gameplayActive = UIManager.Instance != null && UIManager.Instance.GameplayActive;

        SetPlaying(heartbeatSource, gameplayActive, ref heartbeatStarted);
        SetPlaying(alarmSource, gameplayActive && (alarmRequested || AnyBlinkerAlerting()), ref alarmStarted);
    }

    public void RequestAlarm(bool on) {
        alarmRequested = on;
    }

    public void PlayOneShot(AudioClip clip, float volume = 1f) {
        if (clip == null || alarmSource == null)
            return;

        alarmSource.PlayOneShot(clip, volume);
    }

    static bool AnyBlinkerAlerting() {
        var blinkers = VitalBlinker.Active;
        for (int i = 0; i < blinkers.Count; i++) {
            if (blinkers[i].IsAlerting)
                return true;
        }
        return false;
    }

    static void SetPlaying(AudioSource source, bool shouldPlay, ref bool started) {
        if (source == null)
            return;

        if (!shouldPlay) {
            if (source.isPlaying)
                source.Pause();
            return;
        }

        if (source.isPlaying)
            return;

        if (started) {
            source.UnPause();
        } else {
            source.Play();
            started = true;
        }
    }
}
