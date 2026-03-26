using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    [Header("Префабы")]
    public GameObject powerPlantPrefab;
    public GameObject gasPipePrefab;
    public GameObject lowPolyTecPrefab;

    [Header("Проверка маршрута")]
    public Transform gasSourcePoint;
    public Transform tecDestinationPoint;
    public float startPointRadius = 20f;
    public float endPointRadius = 20f;

    private bool routeValidationPassed = false;
    private Vector3? lastStartPoint;
    private Vector3? lastEndPoint;

    [Header("Настройки")]
    public LayerMask buildableLayers;
    public LayerMask forbiddenLayers;

    [Header("Рисование линии")]
    public LineRenderer lineRenderer;

    private BuildingType currentBuildingType = BuildingType.None;
    private GameObject currentPreview;
    private bool isBuilding = false;
    private bool isDrawingMode = false;

    public enum PipeMode { Underground, Overground }
    public PipeMode currentPipeMode = PipeMode.Overground;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Отладочная проверка
        Debug.Log("=== ПРОВЕРКА НАСТРОЕК В BUILDMANAGER ===");
        
        if (gasSourcePoint != null)
        {
            Debug.Log($"✅ Точка начала (ГАЗ): {gasSourcePoint.position}");
            Debug.Log($"   Радиус проверки: {startPointRadius}м");
        }
        else
        {
            Debug.LogError("❌ Точка газовой трубы НЕ НАЗНАЧЕНА!");
            Debug.LogError("   Перетащите объект GasSourcePoint в поле Gas Source Point в Inspector");
        }
        
        if (tecDestinationPoint != null)
        {
            Debug.Log($"✅ Точка конца (ТЭЦ): {tecDestinationPoint.position}");
            Debug.Log($"   Радиус проверки: {endPointRadius}м");
        }
        else
        {
            Debug.LogError("❌ Точка ТЭЦ НЕ НАЗНАЧЕНА!");
            Debug.LogError("   Перетащите объект TECDestinationPoint в поле Tec Destination Point в Inspector");
        }
        
        Debug.Log("=== КОНЕЦ ПРОВЕРКИ ===");
    }

    void Update()
    {
        if (isBuilding)
        {
            UpdateBuildingPreview();

            if (UnityEngine.Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                TryBuildAtMousePosition();

            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
                CancelBuilding();
        }

        if (isDrawingMode)
            UpdateDrawingMode();
    }

    public void StartBuildingPowerPlant()
    {
        currentBuildingType = BuildingType.PowerPlant;
        isBuilding = true;
        CreatePreview(powerPlantPrefab);
    }

    public void StartBuildingGasPipe()
    {
        currentBuildingType = BuildingType.GasPipe;
        isBuilding = true;
        CreatePreview(gasPipePrefab);
    }

    public void StartDrawingMode()
    {
        isDrawingMode = true;
        currentBuildingType = BuildingType.GasPipe;
        currentPipeMode = PipeMode.Overground;

        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.startWidth = 15f;
            lineRenderer.endWidth = 15f;
            lineRenderer.numCornerVertices = 5;
            lineRenderer.numCapVertices = 5;
            lineRenderer.enabled = true;
            UpdateLineStyle();
        }

        if (UIManager.Instance != null)
            UIManager.Instance.ShowPipeModeButtons();

        Debug.Log("РЕЖИМ РИСОВАНИЯ: Нажмите ЛКМ чтобы рисовать, ПРОБЕЛ для завершения");
    }

    public void SetPipeMode(PipeMode mode)
    {
        currentPipeMode = mode;
        UpdateLineStyle();
        Debug.Log($"Режим прокладки: {mode}");
    }

    void UpdateLineStyle()
    {
        if (lineRenderer == null) return;

        if (currentPipeMode == PipeMode.Overground)
        {
            lineRenderer.startColor = new Color(0.4f, 0.2f, 0.1f);
            lineRenderer.endColor = new Color(0.4f, 0.2f, 0.1f);
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
        else
        {
            lineRenderer.startColor = new Color(0.5f, 0.5f, 0.5f);
            lineRenderer.endColor = new Color(0.5f, 0.5f, 0.5f);

            Material dashedMat = new Material(Shader.Find("Sprites/Default"));
            Texture2D tex = new Texture2D(2, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.SetPixel(1, 0, Color.clear);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            dashedMat.mainTexture = tex;
            dashedMat.mainTextureScale = new Vector2(5f, 1f);
            lineRenderer.material = dashedMat;
        }
    }

    void UpdateDrawingMode()
    {
        if (!isDrawingMode) return;
        if (lineRenderer == null) return;

        lineRenderer.enabled = true;

        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 worldPos = hit.point;
                worldPos.y = hit.point.y + 0.5f;

                if (hit.collider.CompareTag("River") && currentPipeMode == PipeMode.Overground)
                {
                    SetPipeMode(PipeMode.Underground);
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ShowObstacleWarning("Река! Переключено на подземный режим.");
                        UIManager.Instance.UpdatePipeModeButtons();
                    }
                }

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
                        if (Vector3.Distance(worldPos, lastPos) > 10f)
                        {
                            lineRenderer.positionCount++;
                            lineRenderer.SetPosition(lineRenderer.positionCount - 1, worldPos);
                        }
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
            CompleteDrawing();
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
                UIManager.Instance.ShowObstacleWarning(warningMessage);
        }
    }

    void CompleteDrawing()
    {
        if (lineRenderer.positionCount < 2)
        {
            Debug.Log("Слишком короткая линия!");
            if (UIManager.Instance != null)
                UIManager.Instance.ShowObstacleWarning("Маршрут слишком короткий! Нужно провести линию минимум из 2 точек.");
            return;
        }

        Vector3 startPos = lineRenderer.GetPosition(0);
        Vector3 endPos = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
        
        Debug.Log($"=== ПРОВЕРКА МАРШРУТА ===");
        Debug.Log($"Начало маршрута: {startPos}");
        Debug.Log($"Конец маршрута: {endPos}");
        
        bool startValid = IsPointNearGasSource(startPos);
        bool endValid = IsPointNearTecDestination(endPos);
        
        Debug.Log($"Начало у газовой трубы: {startValid} (радиус {startPointRadius}м)");
        Debug.Log($"Конец у ТЭЦ: {endValid} (радиус {endPointRadius}м)");
        
        if (!startValid && !endValid)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowRouteError("ОШИБКА МАРШРУТА!\n\nМаршрут должен начинаться у газовой трубы и заканчиваться у ТЭЦ!\n\nНачало: ❌ Не у газовой трубы\nКонец: ❌ Не у ТЭЦ");
            return;
        }
        else if (!startValid)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowRouteError("ОШИБКА МАРШРУТА!\n\nМаршрут должен начинаться у газовой трубы!\n\nНачальная точка не совпадает с источником газа!");
            return;
        }
        else if (!endValid)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowRouteError("ОШИБКА МАРШРУТА!\n\nМаршрут должен заканчиваться у ТЭЦ!\n\nКонечная точка не совпадает с ТЭЦ!");
            return;
        }

        Debug.Log("✅ ВСЕ ПРОВЕРКИ ПРОЙДЕНЫ! Маршрут правильный.");
        
        routeValidationPassed = true;
        isDrawingMode = false;
        isBuilding = false;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HidePipeModeButtons();
            UIManager.Instance.ShowRouteSuccess();
        }

        if (RouteAnalyzer.Instance != null)
            RouteAnalyzer.Instance.Analyze(lineRenderer);

        if (PipeBuilder.Instance != null)
        {
            PipeBuilder.Instance.BuildPipelineFromLine(lineRenderer);
            Debug.Log("3D трубы построены!");
            CreateFinalConnectionPoints();
        }
        else
        {
            Debug.LogError("PipeBuilder.Instance не найден!");
        }

        if (CameraController.Instance != null)
            CameraController.Instance.FlyAlongPipe(lineRenderer);

        Debug.Log("Маршрут газопровода утвержден!");

        lastStartPoint = startPos;
        lastEndPoint = endPos;
    }

    // МЕТОДЫ ПРОВЕРКИ (ДОЛЖНЫ БЫТЬ ВНУТРИ КЛАССА BuildManager)
    bool IsPointNearGasSource(Vector3 point)
    {
        if (gasSourcePoint == null)
        {
            Debug.LogWarning("Gas Source Point не назначен в BuildManager!");
            GameObject gasSource = GameObject.FindGameObjectWithTag("GasSource");
            if (gasSource != null)
            {
                float distance = Vector3.Distance(point, gasSource.transform.position);
                return distance <= startPointRadius;
            }
            return false;
        }

        float dist = Vector3.Distance(point, gasSourcePoint.position);
        bool isValid = dist <= startPointRadius;

        if (!isValid)
            Debug.Log($"Точка начала слишком далеко от источника газа: {dist:F1}м (макс: {startPointRadius}м)");

        return isValid;
    }

    bool IsPointNearTecDestination(Vector3 point)
    {
        if (tecDestinationPoint == null)
        {
            Debug.LogWarning("TEC Destination Point не назначен в BuildManager!");
            GameObject tec = GameObject.FindGameObjectWithTag("TEC");
            if (tec != null)
            {
                float distance = Vector3.Distance(point, tec.transform.position);
                return distance <= endPointRadius;
            }
            return false;
        }

        float dist = Vector3.Distance(point, tecDestinationPoint.position);
        bool isValid = dist <= endPointRadius;

        if (!isValid)
            Debug.Log($"Точка конца слишком далеко от ТЭЦ: {dist:F1}м (макс: {endPointRadius}м)");

        return isValid;
    }

    void OnDrawGizmos()
    {
        if (gasSourcePoint != null)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Gizmos.DrawSphere(gasSourcePoint.position, startPointRadius);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(gasSourcePoint.position, startPointRadius);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(gasSourcePoint.position + Vector3.up * 3f, "ГАЗОВАЯ ТРУБА (НАЧАЛО)");
            #endif
        }
        
        if (tecDestinationPoint != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawSphere(tecDestinationPoint.position, endPointRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(tecDestinationPoint.position, endPointRadius);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(tecDestinationPoint.position + Vector3.up * 3f, "ТЭЦ (КОНЕЦ)");
            #endif
        }
    }

    public void ShowRoutePointsHint()
    {
        if (UIManager.Instance != null)
        {
            string hint = "🔵 СИНЯЯ СФЕРА: Начало маршрута (Источник газа)\n\n🔴 КРАСНАЯ СФЕРА: Конец маршрута (ТЭЦ)\n\nПроведите линию от синей сферы к красной!";
            UIManager.Instance.ShowObstacleWarning(hint);
        }
    }

    public bool IsRouteValid()
    {
        return routeValidationPassed;
    }

    public Vector3? GetLastStartPoint()
    {
        return lastStartPoint;
    }

    public Vector3? GetLastEndPoint()
    {
        return lastEndPoint;
    }

    void CreateFinalConnectionPoints()
    {
        Vector3 startPos = lineRenderer.GetPosition(0);
        GameObject finalStart = new GameObject("Final_GasSource");
        finalStart.transform.position = startPos;

        GameObject startPlatform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        startPlatform.transform.SetParent(finalStart.transform);
        startPlatform.transform.localPosition = new Vector3(0, 0.1f, 0);
        startPlatform.transform.localScale = new Vector3(10f, 0.5f, 10f);
        startPlatform.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f);
        Destroy(startPlatform.GetComponent<Collider>());

        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.transform.SetParent(finalStart.transform);
        ball.transform.localPosition = new Vector3(0, 5f, 0);
        ball.transform.localScale = new Vector3(5f, 5f, 5f);
        Renderer ballRend = ball.GetComponent<Renderer>();
        ballRend.material.color = new Color(1f, 0.2f, 0.2f);
        ballRend.material.EnableKeyword("_EMISSION");
        ballRend.material.SetColor("_EmissionColor", new Color(1f, 0f, 0f) * 1.5f);
        Destroy(ball.GetComponent<Collider>());
        ball.AddComponent<RotateIndicator>();

        Vector3 endPos = lineRenderer.GetPosition(lineRenderer.positionCount - 1);

        if (lowPolyTecPrefab != null)
        {
            Vector3 spawnPos = new Vector3(endPos.x, 0f, endPos.z);
            GameObject tec = Instantiate(lowPolyTecPrefab, spawnPos, lowPolyTecPrefab.transform.rotation);
            tec.name = "Final_PowerPlant";
            Debug.Log("Газопровод подключен к ТЭЦ (LowPolyTec)!");
        }
        else
        {
            Debug.LogWarning("lowPolyTecPrefab не назначен! Используется заглушка.");
            GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fallback.name = "Final_PowerPlant_Fallback";
            fallback.transform.position = new Vector3(endPos.x, 5f, endPos.z);
            fallback.transform.localScale = new Vector3(15f, 10f, 10f);
            fallback.GetComponent<Renderer>().material.color = new Color(0.6f, 0.6f, 0.6f);
            Destroy(fallback.GetComponent<Collider>());
        }
    }

    void ShowSuccess()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ShowSuccessPanel();
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

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float distance;
        if (groundPlane.Raycast(ray, out distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            worldPos.y = 0f;
            currentPreview.transform.position = worldPos;
        }
    }

    void TryBuildAtMousePosition()
    {
        Vector3 buildPos = currentPreview.transform.position;
        if (CanBuildHere(buildPos))
            BuildAtPosition(buildPos);
        else
        {
            Debug.Log("Здесь нельзя строить!");
            if (TooltipManager.Instance != null)
                TooltipManager.Instance.ShowTooltip("Невозможно построить здесь!");
        }
    }

    [ContextMenu("Auto Create Route Points")]
    void AutoCreateRoutePoints()
    {
        GameObject gasPipe = GameObject.FindGameObjectWithTag("GasPipe");
        if (gasPipe == null)
            gasPipe = GameObject.Find("GasPipe") ?? GameObject.Find("GasSource");
        
        if (gasPipe != null)
        {
            if (gasSourcePoint == null)
            {
                GameObject sourcePoint = new GameObject("GasSourcePoint");
                sourcePoint.transform.position = gasPipe.transform.position;
                sourcePoint.transform.position += Vector3.up * 0.5f;
                
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.transform.SetParent(sourcePoint.transform);
                marker.transform.localScale = Vector3.one * 2f;
                marker.GetComponent<Renderer>().material.color = Color.cyan;
                Destroy(marker.GetComponent<Collider>());
                
                gasSourcePoint = sourcePoint.transform;
                Debug.Log($"Создана точка газовой трубы на позиции: {gasSourcePoint.position}");
            }
        }
        else
        {
            Debug.LogError("Не найдена газовая труба!");
        }
        
        GameObject tec = GameObject.FindGameObjectWithTag("TEC");
        if (tec == null)
            tec = GameObject.Find("TEC") ?? GameObject.Find("PowerPlant");
        
        if (tec != null)
        {
            if (tecDestinationPoint == null)
            {
                GameObject destPoint = new GameObject("TECDestinationPoint");
                destPoint.transform.position = tec.transform.position;
                destPoint.transform.position += Vector3.up * 0.5f;
                
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.transform.SetParent(destPoint.transform);
                marker.transform.localScale = Vector3.one * 2f;
                marker.GetComponent<Renderer>().material.color = Color.red;
                Destroy(marker.GetComponent<Collider>());
                
                tecDestinationPoint = destPoint.transform;
                Debug.Log($"Создана точка ТЭЦ на позиции: {tecDestinationPoint.position}");
            }
        }
        else
        {
            Debug.LogError("Не найдена ТЭЦ!");
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