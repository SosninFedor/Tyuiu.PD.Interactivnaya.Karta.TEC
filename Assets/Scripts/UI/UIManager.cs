using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    [Header("Панели")]
    public GameObject panelStartScreen;
    public GameObject panelGasMission;
    public GameObject panelSuccess;
    public GameObject panelObstacleWarning;
    
    [Header("Кнопки")]
    public Button buttonStartMission;
    public Button buttonStartGas;
    public Button buttonLaunchPowerPlant;
    
    [Header("Тексты")]
    public TextMeshProUGUI warningText;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        buttonStartMission.onClick.AddListener(OnStartMissionClick);
        buttonStartGas.onClick.AddListener(OnStartGasClick);
        buttonLaunchPowerPlant.onClick.AddListener(OnLaunchPowerPlantClick);
        
        SetupSuccessPanelStyle();
        ShowStartScreen();
    }
    
    void SetupSuccessPanelStyle()
    {
        if (panelSuccess == null) return;

        // Фон панели — тёмно-синий с прозрачностью
        Image panelImage = panelSuccess.GetComponent<Image>();
        if (panelImage != null)
            panelImage.color = new Color(0.05f, 0.08f, 0.15f, 0.97f);

        // Стилизуем кнопку "ЗАПУСТИТЬ ТЭЦ!"
        if (buttonLaunchPowerPlant != null)
        {
            Image btnImg = buttonLaunchPowerPlant.GetComponent<Image>();
            if (btnImg != null)
                btnImg.color = new Color(0.0f, 0.75f, 0.4f, 1f); // Зелёный акцент

            TextMeshProUGUI btnText = buttonLaunchPowerPlant.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.color = Color.white;
                btnText.fontSize = 18;
                btnText.fontStyle = FontStyles.Bold;
            }

            // Hover эффект
            ColorBlock colors = buttonLaunchPowerPlant.colors;
            colors.normalColor = new Color(0.0f, 0.75f, 0.4f, 1f);
            colors.highlightedColor = new Color(0.0f, 0.95f, 0.55f, 1f);
            colors.pressedColor = new Color(0.0f, 0.55f, 0.3f, 1f);
            buttonLaunchPowerPlant.colors = colors;
        }

        // Все тексты внутри панели — белые
        foreach (TextMeshProUGUI tmp in panelSuccess.GetComponentsInChildren<TextMeshProUGUI>())
        {
            tmp.color = Color.white;
        }
    }
    
    public void ShowStartScreen()
    {
        panelStartScreen.SetActive(true);
        panelGasMission.SetActive(false);
        panelSuccess.SetActive(false);
        panelObstacleWarning.SetActive(false);
    }
    
    void OnStartMissionClick()
    {
        panelStartScreen.SetActive(false);
        panelGasMission.SetActive(true);
        Debug.Log("Показываем миссию с газопроводом");
    }
    
    void OnStartGasClick()
    {
        panelGasMission.SetActive(false);
        if (BuildManager.Instance != null)
        {
            BuildManager.Instance.StartDrawingMode();
            Debug.Log("=== РЕЖИМ РИСОВАНИЯ АКТИВИРОВАН ===");
        }
    }
    
    void OnLaunchPowerPlantClick()
{
    panelSuccess.SetActive(false);
    
    if (PowerPlantLauncher.Instance != null)
        PowerPlantLauncher.Instance.LaunchPowerPlant();
    else
        Debug.LogWarning("PowerPlantLauncher не найден! Добавь его на сцену.");
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
        // Плавное появление панели
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

        // Пульсация кнопки
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