using System.Collections;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject callDoctorPanel;
    public GameObject breathTubePanel;
    public GameObject vitalsMonitorPanel;
    public GameObject EHRPanel;

    public GameObject startScreenPanel;
    public TextMeshProUGUI startTitleText;
    public TextMeshProUGUI startDescriptionText;
    public TextMeshProUGUI startGoalsText;

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

    public void OpenCallDoctor()
    {
        callDoctorPanel.SetActive(true);

        playerMovement.canMove = false;

        GameTimer.Instance.Pause();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseCallDoctor()
    {
        callDoctorPanel.SetActive(false);

        playerMovement.canMove = true;
        GameTimer.Instance.Resume();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenVentilator()
    {
        breathTubePanel.SetActive(true);

        playerMovement.canMove = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseVentilator()
    {
        breathTubePanel.SetActive(false);


        playerMovement.canMove = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
