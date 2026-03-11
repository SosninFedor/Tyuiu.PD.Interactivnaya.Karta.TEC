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
    
    [Header("Тексты")]
    public TextMeshProUGUI warningText;

    private GameObject resultsBlock;
    
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
                if (BuildManager.Instance != null)
                    BuildManager.Instance.StartDrawingMode();
            });

        SetupSuccessPanelStyle();

        panelStartScreen.SetActive(false);
        panelSuccess.SetActive(false);
        panelObstacleWarning.SetActive(false);
        if (gasMissionPopup != null)
            gasMissionPopup.SetActive(false);

        if (CameraController.Instance != null)
            CameraController.Instance.StartIntro();
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

    void SetupSuccessPanelStyle()
    {
        if (panelSuccess == null) return;
        Image panelImage = panelSuccess.GetComponent<Image>();
        if (panelImage != null)
            panelImage.color = new Color(0.05f, 0.08f, 0.15f, 0.97f);

        if (buttonLaunchPowerPlant != null)
        {
            Image btnImg = buttonLaunchPowerPlant.GetComponent<Image>();
            if (btnImg != null)
                btnImg.color = new Color(0.0f, 0.75f, 0.4f, 1f);

            TextMeshProUGUI btnText = buttonLaunchPowerPlant.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.color = Color.white;
                btnText.fontSize = 18;
                btnText.fontStyle = FontStyles.Bold;
            }

            ColorBlock colors = buttonLaunchPowerPlant.colors;
            colors.normalColor = new Color(0.0f, 0.75f, 0.4f, 1f);
            colors.highlightedColor = new Color(0.0f, 0.95f, 0.55f, 1f);
            colors.pressedColor = new Color(0.0f, 0.55f, 0.3f, 1f);
            buttonLaunchPowerPlant.colors = colors;
        }

        foreach (TextMeshProUGUI tmp in panelSuccess.GetComponentsInChildren<TextMeshProUGUI>())
            tmp.color = Color.white;
    }

    void CreateResultsBlock()
    {
        if (resultsBlock != null) Destroy(resultsBlock);

        Canvas canvas = FindObjectOfType<Canvas>();
        resultsBlock = new GameObject("ResultsBlock");
        resultsBlock.transform.SetParent(canvas.transform, false);

        RectTransform rt = resultsBlock.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, -80f);
        rt.sizeDelta = new Vector2(500f, 180f);

        Image bg = resultsBlock.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.7f);

        // Звёзды
        string starsText = RouteAnalyzer.Instance != null
            ? (RouteAnalyzer.Instance.stars == 3 ? "⭐⭐⭐" :
               RouteAnalyzer.Instance.stars == 2 ? "⭐⭐" : "⭐")
            : "⭐⭐⭐";

        string gradeText = RouteAnalyzer.Instance != null ? RouteAnalyzer.Instance.grade : "Отлично";
        float length = RouteAnalyzer.Instance != null ? RouteAnalyzer.Instance.totalLength : 0f;
        int bends = RouteAnalyzer.Instance != null ? RouteAnalyzer.Instance.totalBends : 0;

        // Цвет оценки
        Color gradeColor = gradeText == "Отлично" ? new Color(0.2f, 1f, 0.4f) :
                           gradeText == "Хорошо" ? new Color(1f, 0.8f, 0.1f) :
                           new Color(1f, 0.4f, 0.2f);

        // Текст результатов
        GameObject textObj = new GameObject("ResultsText");
        textObj.transform.SetParent(resultsBlock.transform, false);
        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(20f, 10f);
        textRt.offsetMax = new Vector2(-20f, -10f);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = $"<size=28>{starsText}</size>\n" +
                   $"<size=22><b>Оценка: <color=#{ColorUtility.ToHtmlStringRGB(gradeColor)}>{gradeText}</color></b></size>\n" +
                   $"<size=18>Длина маршрута: <b>{length:F0} м</b>   |   Изгибов: <b>{bends}</b></size>";
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        resultsBlock.SetActive(false);
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
        if (resultsBlock != null) resultsBlock.SetActive(false);
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
        CreateResultsBlock();
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

        // Показываем результаты с задержкой
        yield return new WaitForSeconds(0.5f);
        if (resultsBlock != null)
            StartCoroutine(AnimatePopup(resultsBlock));

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