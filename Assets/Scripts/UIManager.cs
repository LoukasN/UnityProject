using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject vitalsMonitorPanel;
    public GameObject EHRPanel;

    public GameObject startScreenPanel;
    public TextMeshProUGUI startTitleText;
    public TextMeshProUGUI startDescriptionText;
    public TextMeshProUGUI startGoalsText;
    public TextMeshProUGUI scenarioInformationText;

    public GameObject scenarioSelectPanel;

    public GameObject pauseMenuPanel;
    bool isPaused;
    bool gameplayStarted;
    public bool GameplayActive => gameplayStarted && !isPaused;
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
    bool hasToastText;
    bool hasObjectiveText;
    Node lastObjectiveNode;

    public PlayerMovement playerMovement;

    void Start()
    {
        playerMovement = FindFirstObjectByType<PlayerMovement>();

        OpenScenarioSelect();
    }

    public void RefreshStartScreenText()
    {
        var scenarioLoader = FindFirstObjectByType<ScenarioLoader>();
        var meta = scenarioLoader.CurrentScenario.meta;

        var goals = meta.learningGoals;

        startTitleText.text = meta.title;
        startDescriptionText.text = meta.description;
        startGoalsText.text = goals == null || goals.Count == 0 ? "" : "• " + string.Join("\n• ", goals);

        if (scenarioInformationText != null)
        {
            scenarioInformationText.text = $"Δυσκολία: {meta.difficulty}\n\nΕκτιμώμενη διάρκεια: {meta.estimatedDurationMinutes} λεπτά";
        }
    }

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        PollObjectiveText();

        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            return;
        }

        if (isPaused)
        {
            ClosePauseMenu();
        }
        else if (gameplayStarted)
        {
            OpenPauseMenu();
        }
    }

    void PollObjectiveText()
    {
        if (ScenarioEngine.Instance == null)
        {
            return;
        }

        Node node = ScenarioEngine.Instance.CurrentNode;
        if (node == lastObjectiveNode)
        {
            return;
        }

        lastObjectiveNode = node;

        if (node != null && node.type != "end")
        {
            SetObjectiveText(node.text, node.description);
        }
    }

    public void OpenPauseMenu()
    {
        if (isPaused)
        {
            return;
        }

        wasMovableBeforePause = playerMovement.canMove;
        wasTimerRunningBeforePause = GameTimer.Instance.IsRunning;

        pauseMenuPanel.SetActive(true);
        objectivePanel.SetActive(false);
        toastPanel.SetActive(false);

        playerMovement.canMove = false;
        if (wasTimerRunningBeforePause)
        {
            GameTimer.Instance.Pause();
        }
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ClosePauseMenu()
    {
        pauseMenuPanel.SetActive(false);
        isPaused = false;

        playerMovement.canMove = wasMovableBeforePause;
        if (wasTimerRunningBeforePause)
        {
            GameTimer.Instance.Resume();
        }

        RefreshObjectiveVisibility();
        RefreshToastVisibility();

        if (wasMovableBeforePause)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void PauseMenuGoToMainMenu()
    {
        pauseMenuPanel.SetActive(false);
        isPaused = false;

        vitalsMonitorPanel.SetActive(false);
        EHRPanel.SetActive(false);
        ClearToast();
        ClearObjective();

        GameTimer.Instance.ResetTimer();

        if (ScenarioEngine.Instance != null)
        {
            ScenarioEngine.Instance.AbortScenario();
        }

        OpenScenarioSelect();
    }

    public void OpenVitalsMonitor()
    {
        vitalsMonitorPanel.SetActive(true);

        playerMovement.canMove = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseVitalsMonitor()
    {
        vitalsMonitorPanel.SetActive(false);
        RefreshObjectiveVisibility();

        playerMovement.canMove = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenEHR()
    {
        EHRPanel.SetActive(true);

        playerMovement.canMove = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseEHR()
    {
        EHRPanel.SetActive(false);
        RefreshObjectiveVisibility();

        playerMovement.canMove = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenStartScreen()
    {
        startScreenPanel.SetActive(true);
        objectivePanel.SetActive(false);

        playerMovement.canMove = false;
        gameplayStarted = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseStartScreen()
    {
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

    public void OpenScenarioSelect()
    {
        startScreenPanel.SetActive(false);
        scenarioSelectPanel.SetActive(true);
        objectivePanel.SetActive(false);

        playerMovement.canMove = false;
        gameplayStarted = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseScenarioSelect()
    {
        scenarioSelectPanel.SetActive(false);
        OpenStartScreen();
    }

    public void ShowDebrief(string endText, int score, List<string> decisionPath, List<string> missedDocs)
    {
        endTitleText.text = endText;
        endScoreText.text = $"Score: {score}";
        endDecisionPathText.text = "Οι επιλογές σου\n\n" + string.Join("\n", decisionPath);
        endMissedDocsText.text = missedDocs.Count > 0 ? "Τεκμηρίωση που δεν ολοκληρώθηκε\n\n" + string.Join("\n", missedDocs) : "Η τεκμηρίωση ολοκληρώθηκε πλήρως";

        vitalsMonitorPanel.SetActive(false);
        EHRPanel.SetActive(false);
        pauseMenuPanel.SetActive(false);
        isPaused = false;

        endScreenPanel.SetActive(true);
        ClearObjective();
        ClearToast();

        playerMovement.canMove = false;
        GameTimer.Instance.Pause();
        gameplayStarted = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void GoToStart()
    {
        endScreenPanel.SetActive(false);
        ClearObjective();
        ClearToast();

        GameTimer.Instance.ResetTimer();

        if (ScenarioEngine.Instance != null)
        {
            ScenarioEngine.Instance.AbortScenario();
        }

        OpenStartScreen();
    }

    public void SetObjectiveText(string text, string description = null)
    {
        objectiveText.text = text;
        hasObjectiveText = !string.IsNullOrEmpty(text);
        RefreshObjectiveVisibility();

        if (objectiveDescriptionText != null)
        {
            bool hasDescription = !string.IsNullOrEmpty(description);
            objectiveDescriptionText.text = description;
            objectiveDescriptionText.gameObject.SetActive(hasDescription);
        }
    }

    void RefreshObjectiveVisibility()
    {
        objectivePanel.SetActive(hasObjectiveText);
    }

    void ClearObjective()
    {
        hasObjectiveText = false;
        lastObjectiveNode = null;
        objectiveText.text = "";
        objectivePanel.SetActive(false);
    }

    void RefreshToastVisibility()
    {
        toastPanel.SetActive(hasToastText);
    }

    void ClearToast()
    {
        hasToastText = false;
        if (toastCoroutine != null)
        {
            StopCoroutine(toastCoroutine);
            toastCoroutine = null;
        }
        toastPanel.SetActive(false);
    }

    public void ShowToast(string message)
    {
        ShowToast(message, false);
    }

    public void ShowToast(string message, bool isDanger)
    {
        if (string.IsNullOrEmpty(message))
            return;

        if (toastCoroutine != null)
            StopCoroutine(toastCoroutine);

        toastText.text = message;
        toastBackground.color = isDanger ? toastDangerColor : toastDefaultColor;
        hasToastText = true;
        toastPanel.SetActive(true);

        toastCoroutine = StartCoroutine(HideToastAfterDelay());
    }

    IEnumerator HideToastAfterDelay()
    {
        float remaining = toastDuration;
        while (remaining > 0f)
        {
            if (!isPaused)
                remaining -= Time.deltaTime;
            yield return null;
        }
        toastCoroutine = null;
        ClearToast();
    }
}
