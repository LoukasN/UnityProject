using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour {
    public static UIManager Instance;

    public GameObject vitalsMonitorPanel;
    public GameObject EHRPanel;

    public GameObject startScreenPanel;
    public TextMeshProUGUI startTitleText;
    public TextMeshProUGUI startDescriptionText;
    public TextMeshProUGUI startGoalsText;

    public GameObject scenarioSelectPanel;

    public GameObject pauseMenuPanel;
    bool isPaused;
    bool gameplayStarted;
    public bool IsPaused => isPaused;
    bool wasMovableBeforePause;
    bool wasTimerRunningBeforePause;

    public GameObject endScreenPanel;
    public TextMeshProUGUI endTitleText;
    public TextMeshProUGUI endScoreText;
    public TextMeshProUGUI endDecisionPathText;
    public TextMeshProUGUI endMissedDocsText;

    public GameObject objectivePanel;
    public TextMeshProUGUI objectiveText;
    public TextMeshProUGUI objectiveDescriptionText;

    public GameObject toastPanel;
    public TextMeshProUGUI toastText;
    public UnityEngine.UI.Image toastBackground;
    public Color toastDefaultColor = Color.black;
    public Color toastDangerColor = Color.red;
    public float toastDuration = 5f;

    Coroutine toastCoroutine;
    bool hasObjectiveText;
    Node lastObjectiveNode;

    public PlayerMovement playerMovement;

    void Start() {
        playerMovement = FindFirstObjectByType<PlayerMovement>();

        OpenScenarioSelect();

        StartCoroutine(PushInitialVitalsWhenReady());
    }

    IEnumerator PushInitialVitalsWhenReady() {
        var scenarioLoader = FindFirstObjectByType<ScenarioLoader>();
        var vitalsDataSource = FindFirstObjectByType<DefaultNamespace.VitalsDataSource>();

        while (scenarioLoader.CurrentScenario == null) {
            yield return null;
        }

        vitalsDataSource.UpdateVitals(scenarioLoader.CurrentScenario.initialState.vitals);
    }

    public void RefreshStartScreenText() {
        var scenarioLoader = FindFirstObjectByType<ScenarioLoader>();
        var meta = scenarioLoader.CurrentScenario.meta;

        startTitleText.text = meta.title;
        startDescriptionText.text = meta.description;
        startGoalsText.text = string.Join("\n", meta.learningGoals);
    }

    void Awake() {
        Instance = this;
    }

    void Update() {
        PollObjectiveText();

        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) {
            return;
        }

        if (isPaused) {
            ClosePauseMenu();
        } else if (gameplayStarted) {
            OpenPauseMenu();
        }
    }

    void PollObjectiveText() {
        if (ScenarioEngine.Instance == null) {
            return;
        }

        Node node = ScenarioEngine.Instance.CurrentNode;
        if (node == lastObjectiveNode) {
            return;
        }

        lastObjectiveNode = node;

        if (node != null && node.type != "end") {
            SetObjectiveText(node.text, node.description);
        }
    }

    public void OpenPauseMenu() {
        if (isPaused) {
            return;
        }

        wasMovableBeforePause = playerMovement.canMove;
        wasTimerRunningBeforePause = GameTimer.Instance.IsRunning;

        pauseMenuPanel.SetActive(true);
        objectivePanel.SetActive(false);

        playerMovement.canMove = false;
        if (wasTimerRunningBeforePause) {
            GameTimer.Instance.Pause();
        }
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ClosePauseMenu() {
        pauseMenuPanel.SetActive(false);
        isPaused = false;

        playerMovement.canMove = wasMovableBeforePause;
        if (wasTimerRunningBeforePause) {
            GameTimer.Instance.Resume();
        }

        if (wasMovableBeforePause) {
            RefreshObjectiveVisibility();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void QuitGame() {
        Application.Quit();
    }

    public void PauseMenuGoToMainMenu() {
        pauseMenuPanel.SetActive(false);
        isPaused = false;

        vitalsMonitorPanel.SetActive(false);
        EHRPanel.SetActive(false);
        toastPanel.SetActive(false);

        GameTimer.Instance.ResetTimer();

        OpenScenarioSelect();
    }

    public void OpenVitalsMonitor() {
        vitalsMonitorPanel.SetActive(true);
        objectivePanel.SetActive(false);

        playerMovement.canMove = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseVitalsMonitor() {
        vitalsMonitorPanel.SetActive(false);
        RefreshObjectiveVisibility();

        playerMovement.canMove = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenEHR() {
        EHRPanel.SetActive(true);
        objectivePanel.SetActive(false);

        playerMovement.canMove = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseEHR() {
        EHRPanel.SetActive(false);
        RefreshObjectiveVisibility();

        playerMovement.canMove = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenStartScreen() {
        startScreenPanel.SetActive(true);
        objectivePanel.SetActive(false);

        playerMovement.canMove = false;
        gameplayStarted = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseStartScreen() {
        startScreenPanel.SetActive(false);
        RefreshObjectiveVisibility();

        playerMovement.canMove = true;
        GameTimer.Instance.StartTimer();
        gameplayStarted = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        var scenarioLoader = FindFirstObjectByType<ScenarioLoader>();
        ScenarioEngine.Instance.StartScenario(scenarioLoader.CurrentScenario);
    }

    public void OpenScenarioSelect() {
        startScreenPanel.SetActive(false);
        scenarioSelectPanel.SetActive(true);
        objectivePanel.SetActive(false);

        playerMovement.canMove = false;
        gameplayStarted = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseScenarioSelect() {
        scenarioSelectPanel.SetActive(false);
        OpenStartScreen();
    }

    public void ShowDebrief(string endText, int score, List<string> decisionPath, List<string> missedDocs) {
        endTitleText.text = endText;
        endScoreText.text = $"Score: {score}";
        endDecisionPathText.text = "Επιλογές: \n\n" + string.Join("\n", decisionPath);
        endMissedDocsText.text = missedDocs.Count > 0 ? "Χαμένες επιλογές: " + string.Join("\n", missedDocs) : "Καμία χαμένη επιλογή";

        vitalsMonitorPanel.SetActive(false);
        EHRPanel.SetActive(false);
        pauseMenuPanel.SetActive(false);
        isPaused = false;

        endScreenPanel.SetActive(true);
        objectivePanel.SetActive(false);

        playerMovement.canMove = false;
        GameTimer.Instance.Pause();
        gameplayStarted = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void GoToStart() {
        endScreenPanel.SetActive(false);

        GameTimer.Instance.ResetTimer();

        OpenStartScreen();
    }

    public void SetObjectiveText(string text, string description = null) {
        objectiveText.text = text;
        hasObjectiveText = !string.IsNullOrEmpty(text);
        RefreshObjectiveVisibility();

        if (objectiveDescriptionText != null) {
            bool hasDescription = !string.IsNullOrEmpty(description);
            objectiveDescriptionText.text = description;
            objectiveDescriptionText.gameObject.SetActive(hasDescription);
        }
    }

    void RefreshObjectiveVisibility() {
        objectivePanel.SetActive(hasObjectiveText);
    }

    public void ShowToast(string message) {
        ShowToast(message, false);
    }

    public void ShowToast(string message, bool isDanger) {
        if (string.IsNullOrEmpty(message))
            return;

        if (toastCoroutine != null)
            StopCoroutine(toastCoroutine);

        toastText.text = message;
        toastBackground.color = isDanger ? toastDangerColor : toastDefaultColor;
        toastPanel.SetActive(true);

        toastCoroutine = StartCoroutine(HideToastAfterDelay());
    }

    IEnumerator HideToastAfterDelay() {
        float remaining = toastDuration;
        while (remaining > 0f) {
            if (!isPaused)
                remaining -= Time.deltaTime;
            yield return null;
        }
        toastPanel.SetActive(false);
    }
}
