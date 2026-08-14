using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject vitalsMonitorPanel;
    public GameObject EHRPanel;

    public GameObject startScreenPanel;
    public TextMeshProUGUI startTitleText;
    public TextMeshProUGUI startDescriptionText;
    public TextMeshProUGUI startGoalsText;

    public GameObject endScreenPanel;
    public TextMeshProUGUI endTitleText;
    public TextMeshProUGUI endScoreText;
    public TextMeshProUGUI endDecisionPathText;
    public TextMeshProUGUI endMissedDocsText;

    public GameObject toastPanel;
    public TextMeshProUGUI toastText;
    public float toastDuration = 5f;

    Coroutine toastCoroutine;

    public PlayerMovement playerMovement;

    void Start()
    {
        playerMovement = FindFirstObjectByType<PlayerMovement>();

        OpenStartScreen();

        StartCoroutine(PushInitialVitalsWhenReady());
        StartCoroutine(PopulateStartScreenWhenReady());
    }

    IEnumerator PushInitialVitalsWhenReady()
    {
        var scenarioLoader = FindFirstObjectByType<ScenarioLoader>();
        var vitalsDataSource = FindFirstObjectByType<DefaultNamespace.VitalsDataSource>();

        while (scenarioLoader.CurrentScenario == null)
            yield return null;

        vitalsDataSource.UpdateVitals(scenarioLoader.CurrentScenario.initialState.vitals);
    }

    IEnumerator PopulateStartScreenWhenReady()
    {
        var scenarioLoader = FindFirstObjectByType<ScenarioLoader>();

        while (scenarioLoader.CurrentScenario == null)
            yield return null;

        var meta = scenarioLoader.CurrentScenario.meta;

        startTitleText.text = meta.title;
        startDescriptionText.text = meta.description;
        startGoalsText.text = string.Join("\n", meta.learningGoals);
    }

    void Awake()
    {
        Instance = this;
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


        playerMovement.canMove = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenStartScreen()
    {
        startScreenPanel.SetActive(true);

        playerMovement.canMove = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseStartScreen()
    {
        startScreenPanel.SetActive(false);

        playerMovement.canMove = true;
        GameTimer.Instance.StartTimer();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowDebrief(string endText, int score, List<string> decisionPath, List<string> missedDocs)
    {
        endTitleText.text = endText;
        endScoreText.text = $"Score: {score}";
        endDecisionPathText.text = string.Join("\n", decisionPath);
        endMissedDocsText.text = missedDocs.Count > 0 ? string.Join("\n", missedDocs) : "None";

        endScreenPanel.SetActive(true);

        playerMovement.canMove = false;
        GameTimer.Instance.Pause();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void GoToStart()
    {
        endScreenPanel.SetActive(false);

        GameTimer.Instance.ResetTimer();

        OpenStartScreen();
    }

    public void ShowToast(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        if (toastCoroutine != null)
            StopCoroutine(toastCoroutine);

        toastText.text = message;
        toastPanel.SetActive(true);

        toastCoroutine = StartCoroutine(HideToastAfterDelay());
    }

    IEnumerator HideToastAfterDelay()
    {
        yield return new WaitForSeconds(toastDuration);
        toastPanel.SetActive(false);
    }
}
