using UnityEngine;
using System;

public class GameTimer : MonoBehaviour {
    public static GameTimer Instance { get; private set; }

    private double timePassed = 0f;
    private bool isRunning = false;

    public double TimePassed => timePassed;
    public bool IsRunning => isRunning;

    // Notify events
    public event Action OnStarted;
    public event Action OnPaused;
    public event Action OnResumed;
    public event Action OnReset;

    // Keeps only one clock always available
    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Timer functions
    void Update() {
        if (isRunning) {
            timePassed += Time.deltaTime;
        }
    }

    public void StartTimer() {
        timePassed = 0f;
        isRunning = true;
        OnStarted?.Invoke();
    }

    public void Pause() {
        isRunning = false;
        OnPaused?.Invoke();
    }

    public void Resume() {
        isRunning = true;
        OnResumed?.Invoke();
    }

    public void ResetTimer() {
        timePassed = 0f;
        isRunning = false;
        OnReset?.Invoke();
    }

    // Time format to use in UI
    public string GetFormattedTime() {
        TimeSpan time = TimeSpan.FromSeconds(timePassed);
        return $"{time.Minutes:00}:{time.Seconds:00}";
    }
}
