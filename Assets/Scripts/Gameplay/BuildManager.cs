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
            lineRenderer.startWidth = 0.3f;
            lineRenderer.endWidth = 0.3f;
            
            // Плавные соединения
            lineRenderer.numCornerVertices = 5;
            lineRenderer.numCapVertices = 5;
            
            lineRenderer.enabled = true;
            
            Debug.Log("✓ Красивый LineRenderer готов");
        }
        
      
        
        Debug.Log("🎮 РЕЖИМ РИСОВАНИЯ: Нажмите ЛКМ чтобы рисовать, ПРОБЕЛ для завершения");
    }

    void UpdateDrawingMode()
    {
        if (!isDrawingMode) return;
        if (lineRenderer == null) return;
        
        lineRenderer.enabled = true;
        
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Vector3 mousePos = Input.mousePosition;
            
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            Plane plane = new Plane(Vector3.forward, 0);
            
            float distance;
            if (plane.Raycast(ray, out distance))
            {
                Vector3 worldPos = ray.GetPoint(distance);
                worldPos.z = 0;
                
                if (Input.GetMouseButtonDown(0))
                {
                    lineRenderer.positionCount = 0;
                    lineRenderer.positionCount = 1;
                    lineRenderer.SetPosition(0, worldPos);
                }
                else
                {
                    if (lineRenderer.positionCount > 0)
                    {
                        Vector3 lastPos = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
                        if (Vector3.Distance(worldPos, lastPos) > 0.5f)
                        {
                            lineRenderer.positionCount++;
                            lineRenderer.SetPosition(lineRenderer.positionCount - 1, worldPos);
                        }
                    }
                }
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CompleteDrawing();
        }
    }

    // ===== НОВЫЙ МЕТОД: СОЗДАНИЕ ТОЧЕК ПОДКЛЮЧЕНИЯ =====
    void AddGrassAround(Vector3 center, float radius, int count = 30)
{
    for (int i = 0; i < count; i++)
    {
        // Случайное положение по кругу
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float dist = Random.Range(radius * 0.3f, radius);
        float x = Mathf.Cos(angle) * dist;
        float z = Mathf.Sin(angle) * dist;
        
        Vector3 pos = center + new Vector3(x, 0, z);
        
        // Создаем травинку (без сложной проверки)
        GameObject grass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        grass.name = "Grass";
        grass.transform.position = pos;
        
        float height = Random.Range(0.5f, 1.2f);
        grass.transform.localScale = new Vector3(0.12f, height, 0.12f);
        
        // Разный цвет травы (ярче)
        Color grassColor = Color.Lerp(
            new Color(0.1f, 0.8f, 0.1f), 
            new Color(0.2f, 0.5f, 0.0f), 
            Random.value
        );
        
        grass.GetComponent<Renderer>().material.color = grassColor;
        Destroy(grass.GetComponent<Collider>());
        
        // Иногда добавляем цветочек (чаще)
        if (Random.value < 0.3f)
        {
            GameObject flower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flower.transform.SetParent(grass.transform);
            flower.transform.localPosition = new Vector3(0, 1.4f, 0);
            flower.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            
            Color flowerColor = Random.value > 0.5f ? Color.yellow : Color.red;
            flower.GetComponent<Renderer>().material.color = flowerColor;
            flower.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
            flower.GetComponent<Renderer>().material.SetColor("_EmissionColor", flowerColor * 2f);
            Destroy(flower.GetComponent<Collider>());
        }
    }
    
    Debug.Log($"🌿 Посажено {count} травинок вокруг {center}");
}
    
    // Пунктирная линия-подсказка
    void DrawHintLine(Vector3 start, Vector3 end)
{
    GameObject hintLine = new GameObject("HintLine");
    LineRenderer lr = hintLine.AddComponent<LineRenderer>();
    
    lr.positionCount = 2;
    lr.SetPosition(0, start);
    lr.SetPosition(1, end);
    
    lr.startColor = new Color(1f, 1f, 1f, 0.3f);
    lr.endColor = new Color(1f, 1f, 1f, 0.3f);
    lr.startWidth = 0.2f;
    lr.endWidth = 0.2f;
    
    // Создаем пунктирный материал
    Material dashedMaterial = new Material(Shader.Find("Sprites/Default"));
    Texture2D tex = new Texture2D(2, 1);
    tex.SetPixel(0, 0, Color.white);
    tex.SetPixel(1, 0, Color.clear);
    tex.Apply();
    tex.wrapMode = TextureWrapMode.Repeat;
    dashedMaterial.mainTexture = tex;
    dashedMaterial.mainTextureScale = new Vector2(10f, 1f);
    
    lr.material = dashedMaterial;
    
    // Линия исчезнет через 2 секунды
    Destroy(hintLine, 2f);
}
    
    Texture2D CreateDottedTexture()
    {
        Texture2D texture = new Texture2D(2, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.SetPixel(1, 0, Color.clear);
        texture.Apply();
        texture.wrapMode = TextureWrapMode.Repeat;
        return texture;
    }
    // ===== КОНЕЦ НОВОГО МЕТОДА =====

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

        if (PipeBuilder.Instance != null)
        {
            PipeBuilder.Instance.BuildPipelineFromLine(lineRenderer);
            Debug.Log("3D трубы построены!");
            
            // ===== СОЗДАЕМ КОНЕЧНЫЕ ТОЧКИ (улучшенные) =====
            CreateFinalConnectionPoints();
            // ==============================================
        }
        else
        {
            Debug.LogError("PipeBuilder.Instance не найден! Есть ли объект PipeBuilder в сцене?");
        }

        if (UIManager.Instance != null)
        {
            Invoke(nameof(ShowSuccess), 1.5f);
        }

        Debug.Log("Маршрут газопровода утвержден!");
    }

    // ===== ФИНАЛЬНЫЕ ТОЧКИ (после завершения) =====
   void CreateFinalConnectionPoints()
{
    // Стартовая точка (источник) - УМЕНЬШЕННАЯ
    Vector3 startPos = lineRenderer.GetPosition(0);
    GameObject finalStart = new GameObject("Final_GasSource");
    finalStart.transform.position = startPos;
    
    // Основание (маленькое)
    GameObject startPlatform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
    startPlatform.transform.SetParent(finalStart.transform);
    startPlatform.transform.localPosition = new Vector3(0, 0.1f, 0);
    startPlatform.transform.localScale = new Vector3(1.5f, 0.1f, 1.5f); // Было 2.5f
    startPlatform.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f);
    Destroy(startPlatform.GetComponent<Collider>());
    
    // Шар (поменьше)
    GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    ball.transform.SetParent(finalStart.transform);
    ball.transform.localPosition = new Vector3(0, 0.8f, 0); // Было 1f
    ball.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f); // Было 1.2f
    
    Renderer ballRend = ball.GetComponent<Renderer>();
    ballRend.material.color = new Color(1f, 0.2f, 0.2f);
    ballRend.material.EnableKeyword("_EMISSION");
    ballRend.material.SetColor("_EmissionColor", new Color(1f, 0f, 0f) * 1.5f);
    Destroy(ball.GetComponent<Collider>());
    
    ball.AddComponent<RotateIndicator>();
    
    // Конечная точка (ТЭЦ) - УМЕНЬШЕННАЯ
    Vector3 endPos = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
    GameObject finalEnd = new GameObject("Final_PowerPlant");
    finalEnd.transform.position = endPos;
    
    // Платформа (поменьше)
    GameObject endPlatform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
    endPlatform.transform.SetParent(finalEnd.transform);
    endPlatform.transform.localPosition = new Vector3(0, 0.1f, 0);
    endPlatform.transform.localScale = new Vector3(2.5f, 0.1f, 2.5f); // Было 4f
    endPlatform.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.2f);
    Destroy(endPlatform.GetComponent<Collider>());
    
    // Здание (поменьше)
    GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
    building.transform.SetParent(finalEnd.transform);
    building.transform.localPosition = new Vector3(0, 1f, 0); // Было 1.2f
    building.transform.localScale = new Vector3(2f, 1.8f, 1.8f); // Было 3f, 2.4f, 2.5f
    building.GetComponent<Renderer>().material.color = new Color(0.8f, 0.8f, 0.8f);
    Destroy(building.GetComponent<Collider>());
    
    // Труба (поменьше)
    GameObject chimney = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
    chimney.transform.SetParent(finalEnd.transform);
    chimney.transform.localPosition = new Vector3(0.6f, 2f, 0.2f); // Было 0.8f, 2.5f, 0.3f
    chimney.transform.localScale = new Vector3(0.2f, 1.4f, 0.2f); // Было 0.3f, 1.8f, 0.3f
    chimney.GetComponent<Renderer>().material.color = new Color(0.4f, 0.4f, 0.4f);
    Destroy(chimney.GetComponent<Collider>());
    
    // Вторая труба для красоты
    GameObject chimney2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
    chimney2.transform.SetParent(finalEnd.transform);
    chimney2.transform.localPosition = new Vector3(-0.6f, 2f, 0.2f);
    chimney2.transform.localScale = new Vector3(0.2f, 1.4f, 0.2f);
    chimney2.GetComponent<Renderer>().material.color = new Color(0.4f, 0.4f, 0.4f);
    Destroy(chimney2.GetComponent<Collider>());
    
    Debug.Log("✅ Газопровод подключен к ТЭЦ! (уменьшенные размеры)");
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
        Collider2D forbiddenCollision = Physics2D.OverlapCircle(position, 0.5f, forbiddenLayers);
        if (forbiddenCollision != null) return false;

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
    }
}

public enum BuildingType
{
    None,
    PowerPlant,
    GasPipe
}

public class RotateIndicator : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(Vector3.up * 100 * Time.deltaTime);
    }
}