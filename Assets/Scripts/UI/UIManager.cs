using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    [Header("Панели")]
    public GameObject panelStartScreen;
    public GameObject panelSuccess;
    public GameObject panelObstacleWarning;
    public GameObject gasMissionPopup;

    [Header("Кнопки")]
    public Button buttonStartMission;
    public Button buttonLaunchPowerPlant;
    public Button buttonGasMissionOk;

    [Header("Кнопки режима трубы")]
    public GameObject pipeModePanel;
    public Button buttonOverground;
    public Button buttonUnderground;
    
    [Header("Тексты")]
    public TextMeshProUGUI warningText;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        buttonStartMission.onClick.AddListener(OnStartMissionClick);
        buttonLaunchPowerPlant.onClick.AddListener(OnLaunchPowerPlantClick);

        if (buttonGasMissionOk != null)
            buttonGasMissionOk.onClick.AddListener(() =>
            {
                if (gasMissionPopup != null)
                    gasMissionPopup.SetActive(false);
                
                if (CameraController.Instance != null)
                    CameraController.Instance.MoveToTopView(() =>
                    {
                        if (BuildManager.Instance != null)
                            BuildManager.Instance.StartDrawingMode();
                    });
                else
                    if (BuildManager.Instance != null)
                        BuildManager.Instance.StartDrawingMode();
            });

        if (buttonOverground != null)
            buttonOverground.onClick.AddListener(() =>
            {
                if (BuildManager.Instance != null)
                    BuildManager.Instance.SetPipeMode(BuildManager.PipeMode.Overground);
                UpdatePipeModeButtons();
            });

        if (buttonUnderground != null)
            buttonUnderground.onClick.AddListener(() =>
            {
                if (BuildManager.Instance != null)
                    BuildManager.Instance.SetPipeMode(BuildManager.PipeMode.Underground);
                UpdatePipeModeButtons();
            });

        panelStartScreen.SetActive(false);
        panelSuccess.SetActive(false);
        panelObstacleWarning.SetActive(false);
        if (gasMissionPopup != null)
            gasMissionPopup.SetActive(false);
        if (pipeModePanel != null)
            pipeModePanel.SetActive(false);

        if (CameraController.Instance != null)
            CameraController.Instance.StartIntro();
    }

    public void ShowPipeModeButtons()
    {
        if (pipeModePanel != null)
            pipeModePanel.SetActive(true);
        UpdatePipeModeButtons();
    }

    public void HidePipeModeButtons()
    {
        if (pipeModePanel != null)
            pipeModePanel.SetActive(false);
    }

    public void UpdatePipeModeButtons()
    {
        if (BuildManager.Instance == null) return;

        bool isOverground = BuildManager.Instance.currentPipeMode == BuildManager.PipeMode.Overground;

        if (buttonOverground != null)
        {
            Image img = buttonOverground.GetComponent<Image>();
            if (img != null)
                img.color = isOverground ? new Color(0.1f, 0.5f, 0.9f) : new Color(0.4f, 0.4f, 0.4f);
        }

        if (buttonUnderground != null)
        {
            Image img = buttonUnderground.GetComponent<Image>();
            if (img != null)
                img.color = !isOverground ? new Color(0.1f, 0.5f, 0.9f) : new Color(0.4f, 0.4f, 0.4f);
        }
    }

    public void ShowGasMissionPopup()
    {
        if (gasMissionPopup != null)
            StartCoroutine(AnimatePopup(gasMissionPopup));
    }

    IEnumerator AnimatePopup(GameObject popup)
    {
        popup.SetActive(true);
        CanvasGroup cg = popup.GetComponent<CanvasGroup>();
        if (cg == null) cg = popup.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            cg.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
        cg.alpha = 1f;
    }
    
    public void ShowStartScreen()
    {
        panelStartScreen.SetActive(true);
        panelSuccess.SetActive(false);
        panelObstacleWarning.SetActive(false);
    }
    
    void OnStartMissionClick()
    {
        panelStartScreen.SetActive(false);
        StartCoroutine(DelayedMoveToGas());
    }

    IEnumerator DelayedMoveToGas()
    {
        yield return new WaitForSeconds(2f);
        
        if (CameraController.Instance != null)
        {
            CameraController.Instance.MoveToGasLine(() =>
            {
                ShowGasMissionPopup();
            });
        }
        else
        {
            ShowGasMissionPopup();
        }
    }
    
    void OnLaunchPowerPlantClick()
    {
        panelSuccess.SetActive(false);
        if (PowerPlantLauncher.Instance != null)
            PowerPlantLauncher.Instance.LaunchPowerPlant();
        else
            Debug.LogWarning("PowerPlantLauncher не найден!");
    }
    
    public void ShowObstacleWarning(string message)
    {
        if (warningText != null)
            warningText.text = message;
        panelObstacleWarning.SetActive(true);
        Invoke("HideObstacleWarning", 3f);
    }
    
    void HideObstacleWarning()
    {
        panelObstacleWarning.SetActive(false);
    }
    
    public void ShowSuccessPanel()
    {
        panelSuccess.SetActive(true);
        StartCoroutine(AnimateSuccessPanel());
    }

    IEnumerator AnimateSuccessPanel()
    {
        CanvasGroup cg = panelSuccess.GetComponent<CanvasGroup>();
        if (cg == null) cg = panelSuccess.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            cg.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
        cg.alpha = 1f;

        if (buttonLaunchPowerPlant != null)
            StartCoroutine(PulseButton(buttonLaunchPowerPlant));
    }

    IEnumerator PulseButton(Button btn)
    {
        Vector3 originalScale = btn.transform.localScale;
        while (panelSuccess.activeSelf)
        {
            float t = (Mathf.Sin(Time.time * 3f) + 1f) / 2f;
            btn.transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.07f, t);
            yield return null;
        }
        btn.transform.localScale = originalScale;
    }
}