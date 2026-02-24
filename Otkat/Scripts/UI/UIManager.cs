using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    [Header("Панели")]
    public GameObject panelStartScreen;
    public GameObject panelGasMission;
    public GameObject panelSuccess;
    public GameObject panelObstacleWarning;
    
    [Header("Кнопки")]
    public Button buttonStartMission;     // Кнопка "Приступить!" в Panel_StartScreen
    public Button buttonStartGas;        // Кнопка "Приступить!" в Panel_GasMission
    public Button buttonLaunchPowerPlant; // Кнопка "ЗАПУСТИТЬ ТЭЦ!" в Panel_Success
    
    [Header("Тексты")]
    public TextMeshProUGUI warningText;  // Текст для предупреждений
    
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
    // Настраиваем кнопки
    buttonStartMission.onClick.AddListener(OnStartMissionClick);
    buttonStartGas.onClick.AddListener(OnStartGasClick);
    buttonLaunchPowerPlant.onClick.AddListener(OnLaunchPowerPlantClick);
    
    // Начальное состояние: только стартовая панель активна
    ShowStartScreen();
}
    
    // Показать стартовую панель
    public void ShowStartScreen()
    {
        panelStartScreen.SetActive(true);
        panelGasMission.SetActive(false);
        panelSuccess.SetActive(false);
        panelObstacleWarning.SetActive(false);
    }
    
    // Нажатие на "Приступить!" в стартовой панели
    void OnStartMissionClick()
    {
        panelStartScreen.SetActive(false);
        panelGasMission.SetActive(true);
        
        // Здесь будет перемещение камеры к газовой линии
        Debug.Log("Показываем миссию с газопроводом");
    }
    
    // Нажатие на "Приступить!" в панели газовой миссии
    void OnStartGasClick()
{
    panelGasMission.SetActive(false);
    
    if (BuildManager.Instance != null)
    {
        BuildManager.Instance.StartDrawingMode();
        Debug.Log("=== РЕЖИМ РИСОВАНИЯ АКТИВИРОВАН ==="); 
    }
}
    
    // Нажатие на "ЗАПУСТИТЬ ТЭЦ!"
    void OnLaunchPowerPlantClick()
    {
        panelSuccess.SetActive(false);
        Debug.Log("ТЭЦ запущена!");
        
        // Здесь можно добавить следующую миссию
        // Сейчас просто показываем стартовую панель
        ShowStartScreen();
    }
    
    // Показать предупреждение о препятствии
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
    
    // Показать панель успеха
    public void ShowSuccessPanel()
    {
        panelSuccess.SetActive(true);
    }
}