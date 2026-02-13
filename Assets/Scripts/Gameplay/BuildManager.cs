using UnityEngine;
using UnityEngine.EventSystems;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    [Header("Префабы")]
    public GameObject powerPlantPrefab;
    public GameObject gasPipePrefab;

    [Header("Настройки")]
    public LayerMask buildableLayers;
    public LayerMask forbiddenLayers;

    [Header("Рисование линии")]
    public LineRenderer lineRenderer;

    private BuildingType currentBuildingType = BuildingType.None;
    private GameObject currentPreview;
    private bool isBuilding = false;
    private bool isDrawingMode = false;
    private Vector3 lastDrawPosition;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {

        if (isBuilding)
        {
            UpdateBuildingPreview();

            if (UnityEngine.Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                TryBuildAtMousePosition();
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                CancelBuilding();
            }
        }

        // Режим рисования
        if (isDrawingMode)
        {
            UpdateDrawingMode();
        }
    }

    public void StartBuildingPowerPlant()
    {
        currentBuildingType = BuildingType.PowerPlant;
        isBuilding = true;
        CreatePreview(powerPlantPrefab);
        Debug.Log("Режим строительства электростанции");
    }

    public void StartBuildingGasPipe()
    {
        currentBuildingType = BuildingType.GasPipe;
        isBuilding = true;
        CreatePreview(gasPipePrefab);
        Debug.Log("Режим строительства газопровода");
    }

    public void StartDrawingMode()
{
    isDrawingMode = true;
    currentBuildingType = BuildingType.GasPipe;
    
    if (lineRenderer == null)
        lineRenderer = GetComponent<LineRenderer>();
    
    if (lineRenderer != null)
    {
        lineRenderer.positionCount = 0;
        
        // Красивые настройки
        lineRenderer.startColor = new Color(0.4f, 0.2f, 0.1f); // Коричневый
        lineRenderer.endColor = new Color(0.4f, 0.2f, 0.1f);
        lineRenderer.startWidth = 0.3f;  // Тонкая линия
        lineRenderer.endWidth = 0.3f;
        
        // Плавные соединения
        lineRenderer.numCornerVertices = 5;
        lineRenderer.numCapVertices = 5;
        
        lineRenderer.enabled = true;
        
        Debug.Log("✓ Красивый LineRenderer готов");
    }
    
    Debug.Log("🎮 РЕЖИМ РИСОВАНИЯ: isDrawingMode = " + isDrawingMode);
    Debug.Log("Нажмите ЛКМ чтобы рисовать, ПРОБЕЛ для завершения");
}

    void UpdateDrawingMode()
{
    if (!isDrawingMode) return;
    if (lineRenderer == null) return;
    
    // Принудительно включаем LineRenderer
    lineRenderer.enabled = true;
    
    if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
    {
        // Получаем позицию мыши
        Vector3 mousePos = Input.mousePosition;
        
        // Конвертируем в мировые координаты через камеру
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        Plane plane = new Plane(Vector3.forward, 0); // Плоскость на Z=0
        
        float distance;
        if (plane.Raycast(ray, out distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            worldPos.z = 0; // Фиксируем Z=0
            
            if (Input.GetMouseButtonDown(0))
            {
                // Начинаем новую линию
                lineRenderer.positionCount = 0;
                lineRenderer.positionCount = 1;
                lineRenderer.SetPosition(0, worldPos);
                Debug.Log("Начали рисовать в: " + worldPos);
            }
            else
            {
                // Проверяем расстояние от последней точки
                if (lineRenderer.positionCount > 0)
                {
                    Vector3 lastPos = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
                    if (Vector3.Distance(worldPos, lastPos) > 0.5f)
                    {
                        lineRenderer.positionCount++;
                        lineRenderer.SetPosition(lineRenderer.positionCount - 1, worldPos);
                        Debug.Log("Рисуем... Точек: " + lineRenderer.positionCount + " в " + worldPos);
                    }
                }
            }
        }
    }
    
    // Завершение по пробелу
    if (Input.GetKeyDown(KeyCode.Space))
    {
        CompleteDrawing();
    }
}

    void CheckObstacleAtPosition(Vector3 position)
    {
        Collider2D obstacle = Physics2D.OverlapCircle(position, 0.5f, forbiddenLayers);

        if (obstacle != null)
        {
            string warningMessage = "";

            switch (obstacle.tag)
            {
                case "Building":
                    warningMessage = "Проект не утвержден! Нарушение норм безопасности! Газопровод проложен в санитарной зоне жилой застройки. Жители под угрозой!";
                    break;
                case "Road":
                    warningMessage = "Проект не утвержден! Риск аварии на дороге! Пересечение магистрали без защитной гильзы недопустимо.";
                    break;
                case "Ravine":
                    warningMessage = "Проект не утвержден! Опасность оползня! В случае ливня трубу в овраге смоет или разорвет!";
                    break;
                case "Field":
                    warningMessage = "Проект не утвержден! Конфликт с сельхозпредприятием! Прокладка трубы по пашне уничтожает урожай и требует огромных компенсаций. Бюджет проекта превышен!";
                    break;
            }

            if (!string.IsNullOrEmpty(warningMessage) && UIManager.Instance != null)
            {
                UIManager.Instance.ShowObstacleWarning(warningMessage);
            }
        }
    }
     
    

     
    void CompleteDrawing()
    {
        if (lineRenderer.positionCount < 2)
        {
            Debug.Log("Слишком короткая линия!");
            return;
        }

        isDrawingMode = false;
        isBuilding = false;

        // Показываем панель успеха через 1.5 секунды
        if (UIManager.Instance != null)
        {
            Invoke(nameof(ShowSuccess), 1.5f);
        }

        Debug.Log("Маршрут газопровода утвержден!");
    }

    void ShowSuccess()
{
    if (UIManager.Instance != null)
    {
        UIManager.Instance.ShowSuccessPanel(); 
    }
}

    void CreatePreview(GameObject prefab)
    {
        if (currentPreview != null) Destroy(currentPreview);
        currentPreview = Instantiate(prefab);
        if (currentPreview.GetComponent<Collider2D>() != null)
            currentPreview.GetComponent<Collider2D>().enabled = false;
    }

    void UpdateBuildingPreview()
    {
        if (currentPreview == null) return;
        if (Camera.main == null) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        currentPreview.transform.position = mousePos;
    }

    void TryBuildAtMousePosition()
    {
        Vector3 buildPos = currentPreview.transform.position;

        if (CanBuildHere(buildPos))
        {
            BuildAtPosition(buildPos);
        }
        else
        {
            Debug.Log("Здесь нельзя строить!");
            if (TooltipManager.Instance != null)
                TooltipManager.Instance.ShowTooltip("Невозможно построить здесь!");
        }
    }

    bool CanBuildHere(Vector3 position)
    {
        // Проверяем коллизии с запрещенными объектами
        Collider2D forbiddenCollision = Physics2D.OverlapCircle(position, 0.5f, forbiddenLayers);
        if (forbiddenCollision != null) return false;

        // Проверяем что находимся на buildable слое
        Collider2D buildableCollision = Physics2D.OverlapCircle(position, 0.5f, buildableLayers);
        return buildableCollision != null;
    }

    void BuildAtPosition(Vector3 position)
    {
        GameObject newBuilding = null;

        switch (currentBuildingType)
        {
            case BuildingType.PowerPlant:
                newBuilding = Instantiate(powerPlantPrefab, position, Quaternion.identity);
                break;
            case BuildingType.GasPipe:
                newBuilding = Instantiate(gasPipePrefab, position, Quaternion.identity);
                break;
        }

        if (newBuilding != null)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayBuildSound();
            Debug.Log($"Построен {currentBuildingType} в позиции {position}");
        }

        CancelBuilding();
    }

    public void CancelBuilding()
    {
        isBuilding = false;
        currentBuildingType = BuildingType.None;
        if (currentPreview != null) Destroy(currentPreview);
    }

    public void OnPipeDestroyed(GasPipe pipe)
    {
        Debug.Log("Газопровод удален");
        // Здесь будет логика обновления подключений
    }
}

public enum BuildingType
{
    None,
    PowerPlant,
    GasPipe
}
