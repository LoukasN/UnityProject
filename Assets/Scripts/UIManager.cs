using System.Collections;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject callDoctorPanel;
    public GameObject breathTubePanel;
    public GameObject vitalsMonitorPanel;

    public PlayerMovement playerMovement;

    void Start()
    {
        playerMovement = FindFirstObjectByType<PlayerMovement>();

        StartCoroutine(PushInitialVitalsWhenReady());
    }

    IEnumerator PushInitialVitalsWhenReady()
    {
        var scenarioLoader = FindFirstObjectByType<ScenarioLoader>();
        var vitalsDataSource = FindFirstObjectByType<DefaultNamespace.VitalsDataSource>();

        // Wait until ScenarioLoader has actually finished parsing the JSON,
        // regardless of which script's Start() happened to run first.
        while (scenarioLoader.CurrentScenario == null)
            yield return null;

        vitalsDataSource.UpdateVitals(scenarioLoader.CurrentScenario.initialState.vitals);
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

    public void OpenBreathTube()
    {
        breathTubePanel.SetActive(true);

        playerMovement.canMove = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseBreathTube()
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
}
