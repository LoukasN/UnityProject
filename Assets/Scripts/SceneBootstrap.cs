using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBootstrap : MonoBehaviour
{
    [SerializeField]
    List<string> scenesToLoad = new List<string>();

    void Awake()
    {
        Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);

        foreach (var sceneName in scenesToLoad)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                continue;
            }

            if (SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                continue;
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }
    }
}
