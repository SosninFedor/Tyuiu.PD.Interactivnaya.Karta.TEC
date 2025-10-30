using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Основные кнопки")]
    public Button startButton;
    public Button exitButton;
    public Button restartButton;
    
    [Header("Панели меню")]
    public GameObject mainPanel;
    public GameObject settingsPanel;
    
    [Header("Настройки")]
    public float menuFadeTime = 1.0f;
    public Image backgroundImage;

   void Start()
{
    // Безопасная инициализация
    if (startButton != null)
        startButton.onClick.AddListener(StartGame);
    else
        Debug.LogError("StartButton not assigned!");
        
    if (exitButton != null)  
        exitButton.onClick.AddListener(ExitGame);
    else
        Debug.LogError("ExitButton not assigned!");
        
    if (restartButton != null)
        restartButton.onClick.AddListener(RestartGame);
        
    ShowMainPanel();
    
    if (backgroundImage != null)
    {
        Color color = backgroundImage.color;
        color.a = 1f;
        backgroundImage.color = color;
    }
}
    
    // Запуск новой игры
    public void StartGame()
    {
        Debug.Log("Запуск игры!");
        
        // Плавное исчезновение меню
        if (backgroundImage != null)
        {
            StartCoroutine(FadeOutMenu());
        }
        else
        {
            // Если нет анимации - сразу переходим к игре
            SceneManager.LoadScene("GameScene");
        }
    }
    
    // Перезапуск текущей сцены
    public void RestartGame()
    {
        Debug.Log("Перезапуск игры!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    // Выход из игры
    public void ExitGame()
    {
        Debug.Log("Выход из игры");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    // Показать настройки
    public void ShowSettings()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }
    
    // Вернуться в главное меню
    public void ShowMainPanel()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }
    
    // Плавное исчезновение меню (как в ТЗ)
    private System.Collections.IEnumerator FadeOutMenu()
    {
        float elapsedTime = 0f;
        Color startColor = backgroundImage.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        // Плавно уменьшаем прозрачность
        while (elapsedTime < menuFadeTime)
        {
            backgroundImage.color = Color.Lerp(startColor, targetColor, (elapsedTime / menuFadeTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Отключаем все UI элементы меню
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        
        // Переходим к игровой сцене
        SceneManager.LoadScene("GameScene");
    }
    
    // Для кнопки "Назад" в настройках
    public void BackToMainMenu()
    {
        ShowMainPanel();
    }
}