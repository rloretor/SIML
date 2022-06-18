using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Exit : MonoBehaviour
{
    public Button ExitButton;

    void Start()
    {
        ExitButton.onClick.AddListener(TryClose);
    }

    private void TryClose()
    {
        var scene = SceneManager.GetSceneByName("lemmingsGPU");
        if (scene.IsValid())
        {
            StartCoroutine(UnloadScene(scene));
            return;
        }

        scene = SceneManager.GetSceneByName("lemmingsCPU");
        if (scene.IsValid())
        {
            StartCoroutine(UnloadScene(scene));
            return;
        }
#if UNITY_EDITOR
        EditorApplication.ExecuteMenuItem("Edit/Play");
#else
        Application.Quit();
#endif
    }

    private IEnumerator UnloadScene(Scene scene)
    {
        var asyncUnload = SceneManager.UnloadSceneAsync(scene);
        while (!asyncUnload.isDone)
        {
            yield return null;
        }

        GameObject loader = null;
        while (loader == null)
        {
            loader = GameObject.FindWithTag("Loader");
            yield return null;
        }

        var manager = loader.GetComponent<LemmingsSceneLoader>();
        manager.Init();
    }
}