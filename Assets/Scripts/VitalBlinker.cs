using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VitalBlinker : MonoBehaviour {
    static readonly List<VitalBlinker> active = new List<VitalBlinker>();
    public static IReadOnlyList<VitalBlinker> Active => active;

    [SerializeField]
    string hotspotId;
    public string HotspotId => hotspotId;

    [SerializeField]
    string vitalKey;
    public string VitalKey => vitalKey;

    [SerializeField]
    Graphic target;

    [SerializeField]
    Color alertColor = Color.red;

    [SerializeField]
    float blinkInterval = 0.5f;

    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    bool loopSoundWhileAlerting = true;

    Color normalColor;
    bool alerting;
    bool blinkOn;
    float timer;

    void Awake() {
        if (target == null)
            target = GetComponent<Graphic>();

        if (target != null)
            normalColor = target.color;

        if (audioSource == null)
            audioSource = GetComponentInChildren<AudioSource>();
    }

    void OnEnable() {
        active.Add(this);
        timer = 0f;
        ApplyColor(alerting);
    }

    void OnDisable() {
        active.Remove(this);
        ApplyColor(false);
    }

    public void SetAlert(bool on) {
        if (alerting == on)
            return;

        alerting = on;
        timer = 0f;
        ApplyColor(on);
        if (audioSource != null) {
            if (on) {
                audioSource.loop = loopSoundWhileAlerting;
                Debug.Log("Sound Playing?");
                audioSource.Play();
            } else {
                Debug.Log("Sound Not Playing?");
                audioSource.Stop();
            }
        }
    }

    void Update() {
        if (!alerting || target == null)
            return;

        if (UIManager.Instance != null && UIManager.Instance.IsPaused)
            return;

        timer += Time.deltaTime;
        if (timer < blinkInterval)
            return;

        timer -= blinkInterval;
        ApplyColor(!blinkOn);
    }

    void ApplyColor(bool showAlert) {
        blinkOn = showAlert;

        if (target != null)
            target.color = showAlert ? alertColor : normalColor;
    }
}
