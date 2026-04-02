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
    [Header("⭐ НОВЫЕ УЛУЧШЕНИЯ ⭐")]
public TextMeshProUGUI routeLengthText;        // Текст длины маршрута
public TextMeshProUGUI distanceToTargetText;   // Текст расстояния до ТЭЦ
public Slider progressSlider;                  // Прогресс-бар
public GameObject hintPanel;                   // Панель для подсказок
public TextMeshProUGUI hintText;               // Текст подсказки
public TextMeshProUGUI successStatsText;
    
    [Header("Панели ошибок")]
    public GameObject panelRouteError; // Ваша готовая панель с фотографией
    
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
        
        // Инициализация панелей
        panelStartScreen.SetActive(false);
        panelSuccess.SetActive(false);
        panelObstacleWarning.SetActive(false);
        if (gasMissionPopup != null)
            gasMissionPopup.SetActive(false);
        if (pipeModePanel != null)
            pipeModePanel.SetActive(false);
        
        // ВАЖНО: Изначально панель ошибки должна быть скрыта
        if (panelRouteError != null)
            panelRouteError.SetActive(false);
        
        if (CameraController.Instance != null)
            CameraController.Instance.StartIntro();
    }
    
    // НОВЫЙ МЕТОД ДЛЯ ПОКАЗА УСПЕХА МАРШРУТА
    public void ShowRouteSuccess()
    {
        // Используем существующую панель успеха или создаем новую
        if (panelSuccess != null)
        {
            // Можно изменить текст в панели успеха, если нужно
            TextMeshProUGUI successText = panelSuccess.GetComponentInChildren<TextMeshProUGUI>();
            if (successText != null)
            {
                successText.text = "✓ МАРШРУТ УСПЕШНО ПОСТРОЕН! ✓\n\nГазопровод успешно подключен от источника газа к ТЭЦ.\n\nНажмите кнопку для запуска электростанции!";
            }
            
            panelSuccess.SetActive(true);
            StartCoroutine(AnimateSuccessPanel());
            
            // Звук успеха (если есть)
           // if (AudioManager.Instance != null)
                // nager.Instance.PlaySuccessSound();
        }
        else
        {
            Debug.LogWarning("PanelSuccess не назначен! Использую ShowObstacleWarning");
            ShowObstacleWarning("✓ МАРШРУТ УСПЕШНО ПОСТРОЕН! ✓\n\nГазопровод успешно подключен от источника газа к ТЭЦ.");
        }
        
        Debug.Log("Маршрут успешно проложен!");
    }

    public void ShowRouteSuccessWithStats(float length, float straightDistance, float efficiency)
{
    string efficiencyText = efficiency > 90f ? "⭐ Отлично!" : 
                           efficiency > 70f ? "👍 Хорошо" : 
                           efficiency > 50f ? "📐 Можно лучше" : "⚠️ Неэффективно";
    
    if (successStatsText != null)
    {
        successStatsText.text = $"✅ МАРШРУТ ПОСТРОЕН!\n\n" +
                                $"📏 Длина: {length:F0}м\n" +
                                $"📐 По прямой: {straightDistance:F0}м\n" +
                                $"💯 Эффективность: {efficiency:F0}%\n" +
                                $"🏆 Оценка: {efficiencyText}";
    }
    
    if (panelSuccess != null)
        panelSuccess.SetActive(true);
}

// ✅ Метод для показа временной подсказки
public void ShowHint(string message, float duration)
{
    if (hintPanel != null && hintText != null)
    {
        hintText.text = message;
        hintPanel.SetActive(true);
        StartCoroutine(HideHintAfterDelay(duration));
    }
}

IEnumerator HideHintAfterDelay(float delay)
{
    yield return new WaitForSeconds(delay);
    if (hintPanel != null)
        hintPanel.SetActive(false);
}

    public void UpdateRouteStats(float currentLength, float distanceToTarget)
{
    if (routeLengthText != null)
        routeLengthText.text = $"📏 Длина: {currentLength:F0}м";
    
    if (distanceToTargetText != null)
        distanceToTargetText.text = $"🎯 До ТЭЦ: {distanceToTarget:F0}м";
    
    if (progressSlider != null && BuildManager.Instance != null && BuildManager.Instance.tecDestinationPoint != null)
    {
        float totalDistance = Vector3.Distance(
            BuildManager.Instance.gasSourcePoint.position,
            BuildManager.Instance.tecDestinationPoint.position);
        progressSlider.value = Mathf.Clamp01(1f - (distanceToTarget / totalDistance));
    }
}
    
    // МЕТОД ДЛЯ ПОКАЗА ОШИБКИ МАРШРУТА
    public void ShowRouteError(string errorMessage)
    {
        if (panelRouteError == null)
        {
            Debug.LogError("PanelRouteError не назначен в UIManager!");
            ShowObstacleWarning(errorMessage);
            return;
        }
        
        // Ищем текстовые поля в панели
        TextMeshProUGUI errorTitle = panelRouteError.transform.Find("ErrorTitle")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI errorMessageText = panelRouteError.transform.Find("ErrorMessage")?.GetComponent<TextMeshProUGUI>();
        
        // Если нет TextMeshPro, ищем обычный Text
        if (errorTitle == null)
        {
            Text titleText = panelRouteError.transform.Find("ErrorTitle")?.GetComponent<Text>();
            if (titleText != null)
                titleText.text = GetTitleForError(errorMessage);
        }
        else
        {
            errorTitle.text = GetTitleForError(errorMessage);
        }
        
        // Устанавливаем текст ошибки
        if (errorMessageText != null)
        {
            errorMessageText.text = errorMessage;
        }
        else
        {
            Text msgText = panelRouteError.transform.Find("ErrorMessage")?.GetComponent<Text>();
            if (msgText != null)
                msgText.text = errorMessage;
        }
        
        // Находим кнопку "ИСПРАВИТЬ ОШИБКУ"
        Button okButton = panelRouteError.transform.Find("OkButton")?.GetComponent<Button>();
        if (okButton == null)
        {
            // Ищем любую кнопку в панели
            okButton = panelRouteError.GetComponentInChildren<Button>();
        }
        
        if (okButton != null)
        {
            // Удаляем старые обработчики
            okButton.onClick.RemoveAllListeners();
            // Добавляем новый
            okButton.onClick.AddListener(() => {
                CloseRouteError();
            });
        }
        
        // Показываем панель
        panelRouteError.SetActive(true);
        
        // Анимация появления
        StartCoroutine(AnimateErrorPanelAppear(panelRouteError));
        
        // Звук ошибки (если есть)
       // if (AudioManager.Instance != null)
          //  AudioManager.Instance.PlayErrorSound();
        
        Debug.LogError($"Ошибка маршрута: {errorMessage}");
    }
    
    // Выбор заголовка в зависимости от ошибки
    string GetTitleForError(string errorMessage)
    {
        if (errorMessage.Contains("ПРОЕКТ НЕ УТВЕРЖДЁН") || errorMessage.Contains("санитарной зоне"))
        {
            return "ПРОЕКТ НЕ УТВЕРЖДЁН!";
        }
        else if (errorMessage.Contains("начал") || errorMessage.Contains("конц"))
        {
            return "ОШИБКА МАРШРУТА!";
        }
        else if (errorMessage.Contains("превышен"))
        {
            return "БЮДЖЕТ ПРЕВЫШЕН!";
        }
        else if (errorMessage.Contains("река"))
        {
            return "ПРЕПЯТСТВИЕ!";
        }
        else
        {
            return "ВНИМАНИЕ! ОШИБКА!";
        }
    }
    
    // Анимация появления панели ошибки
    IEnumerator AnimateErrorPanelAppear(GameObject panel)
    {
        if (panel == null) yield break;
        
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = panel.AddComponent<CanvasGroup>();
        
        cg.alpha = 0f;
        float t = 0f;
        
        while (t < 0.3f)
        {
            t += Time.deltaTime * 10f;
            cg.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
        
        cg.alpha = 1f;
    }
    
    // Закрытие панели ошибки
    public void CloseRouteError()
    {
        if (panelRouteError != null)
        {
            StartCoroutine(AnimateErrorPanelDisappear());
        }
    }
    
    IEnumerator AnimateErrorPanelDisappear()
    {
        if (panelRouteError == null) yield break;
        
        CanvasGroup cg = panelRouteError.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime * 5f;
                cg.alpha = Mathf.Lerp(1f, 0f, t);
                yield return null;
            }
        }
        
        panelRouteError.SetActive(false);
        
        // Возвращаем в режим рисования
        if (BuildManager.Instance != null)
        {
            BuildManager.Instance.StartDrawingMode();
        }
    }
    
    // Остальные методы
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