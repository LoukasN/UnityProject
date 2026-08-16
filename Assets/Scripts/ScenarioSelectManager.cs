using System.IO;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioSelectManager : MonoBehaviour
{
    public GameObject scenarioButtonPrefab;
    public Transform listContainer;

    void OnEnable()
    {
        PopulateList();
    }

    void PopulateList()
    {
        foreach (Transform child in listContainer)
            Destroy(child.gameObject);

        var loader = FindFirstObjectByType<ScenarioLoader>();
        foreach (string path in loader.GetScenarios())
        {
            string capturedPath = path;
            Scenario scenario = JsonConvert.DeserializeObject<Scenario>(File.ReadAllText(path));

            GameObject buttonObj = Instantiate(scenarioButtonPrefab, listContainer);
            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = scenario.meta.title;

            Button button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(() => LoadScenario(capturedPath));
        }
    }

    void LoadScenario(string path)
    {
        var loader = FindFirstObjectByType<ScenarioLoader>();
        loader.LoadScenario(path);

        UIManager.Instance.RefreshStartScreenText();
        UIManager.Instance.CloseScenarioSelect();
    }
}
