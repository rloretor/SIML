using System.Collections;
using Lemmings;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Button = UnityEngine.UI.Button;

public class LemmingsSceneLoader : MonoBehaviour
{
    public CanvasGroup MainCanvas;
    public Camera MainCamera;
    public Button CPUButton;
    public Button GPUButton;
    public TMP_InputField lemmingsInput;

    private int lemmingsAmount;

    void Start()
    {
        SceneManager.LoadSceneAsync("Exit", LoadSceneMode.Additive);
        Init();
    }

    public void Init()
    {
        MainCamera.gameObject.SetActive(true);
        MainCanvas.alpha = 1.0f;
        MainCanvas.interactable = true;
        AddListeners();
        ToggleButtons(false);
    }

    public void AddListeners()
    {
        CleanupListeners();
        CPUButton.onClick.AddListener(OpenCPUScene);
        GPUButton.onClick.AddListener(OpenGPUScene);
        lemmingsInput.onValueChanged.AddListener(CheckOnlyInt);
    }


    private void CleanupListeners()
    {
        CPUButton.onClick.RemoveAllListeners();
        GPUButton.onClick.RemoveAllListeners();
        lemmingsInput.onValueChanged.RemoveListener(CheckOnlyInt);
    }

    private void OpenCPUScene()
    {
        StartCoroutine(LoadScene("lemmingsCPU"));
    }

    private void OpenGPUScene()
    {
        StartCoroutine(LoadScene("lemmingsGPU"));
    }

    IEnumerator LoadScene(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, new LoadSceneParameters()
        {
            loadSceneMode = LoadSceneMode.Additive
        });
        ToggleButtons(false);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        MainCanvas.alpha = 0;
        MainCanvas.interactable = false;
        GameObject spawner = null;
        MainCamera.gameObject.SetActive(false);
        while (spawner == null)
        {
            spawner = GameObject.FindWithTag("Spawner");
            yield return null;
        }


        LemmingSimulationController controller = spawner.GetComponent<LemmingSimulationController>();
        controller.SimulationModel.LemmingInstances = lemmingsAmount;
        controller.enabled = true;
    }

    private void CheckOnlyInt(string input)
    {
        if (int.TryParse(input, out int n))
        {
            lemmingsAmount = n;
            ToggleButtons(true);
        }
        else
        {
            ToggleButtons(false);
        }
    }

    private void ToggleButtons(bool state)
    {
        CPUButton.interactable = state;
        GPUButton.interactable = state;
    }

    private void OnDisable()
    {
        CleanupListeners();
    }
}