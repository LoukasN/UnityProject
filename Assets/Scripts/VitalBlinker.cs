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

    [SerializeField]
    List<Graphic> targets = new List<Graphic>();

    [SerializeField]
    Color alertColor = Color.red;

    [SerializeField]
    float blinkInterval = 0.5f;

    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    AudioSource passiveAudioSource;

    [SerializeField]
    bool loopSoundWhileAlerting = true;

    readonly List<string> keys = new List<string>();
    readonly List<Graphic> resolvedTargets = new List<Graphic>();
    readonly List<Color> normalColors = new List<Color>();
    bool alerting;
    bool blinkOn;
    float timer;

    public bool HasVitalKeys => keys.Count > 0;

    public bool MatchesVital(ICollection<string> vitalKeys) {
        foreach (var key in keys) {
            if (vitalKeys.Contains(key)) {
                return true;
            }
        }
        return false;
    }

    void Awake() {
        keys.Clear();
        if (!string.IsNullOrWhiteSpace(vitalKey)) {
            foreach (var part in vitalKey.Split(',')) {
                string trimmed = part.Trim();
                if (trimmed.Length > 0) {
                    keys.Add(trimmed);
                }
            }
        }

        resolvedTargets.Clear();
        foreach (var target in targets) {
            if (target != null) {
                resolvedTargets.Add(target);
            }
        }

        if (resolvedTargets.Count == 0) {
            var own = GetComponent<Graphic>();
            if (own != null) {
                resolvedTargets.Add(own);
            } else if (GetComponent<RectTransform>() != null) {
                resolvedTargets.AddRange(GetComponentsInChildren<Graphic>(true));
            }
        }

        normalColors.Clear();
        foreach (var target in resolvedTargets) {
            normalColors.Add(target.color);
        }

        if (audioSource == null)
            audioSource = GetComponentInChildren<AudioSource>();

        if (passiveAudioSource == null)
            passiveAudioSource = GetComponentInChildren<AudioSource>();
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
        if (audioSource != null && passiveAudioSource != null) {
            if (on) {
                audioSource.loop = loopSoundWhileAlerting;
                passiveAudioSource.Stop();
                audioSource.Play();
            } else {
                audioSource.Stop();
                passiveAudioSource.Play();
            }
        }
    }

    void Update() {
        if (!alerting)
            return;

        bool gameplayActive = UIManager.Instance == null || UIManager.Instance.GameplayActive;

        if (audioSource != null && passiveAudioSource != null) {
            if (!gameplayActive && audioSource.isPlaying && !passiveAudioSource.isPlaying) {
                audioSource.Pause();
                passiveAudioSource.UnPause();
            } else if (gameplayActive && !audioSource.isPlaying) {
                passiveAudioSource.Pause();
                audioSource.UnPause();
            }
        }

        if (!gameplayActive || resolvedTargets.Count == 0)
            return;

        timer += Time.deltaTime;
        if (timer < blinkInterval)
            return;

        timer -= blinkInterval;
        ApplyColor(!blinkOn);
    }

    void ApplyColor(bool showAlert) {
        blinkOn = showAlert;

        for (int i = 0; i < resolvedTargets.Count; i++) {
            resolvedTargets[i].color = showAlert ? alertColor : normalColors[i];
        }
    }
}
