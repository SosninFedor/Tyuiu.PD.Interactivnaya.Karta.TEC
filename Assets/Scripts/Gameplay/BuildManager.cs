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
            lineRenderer.startColor = new Color(0.4f, 0.2f, 0.1f);
            lineRenderer.endColor = new Color(0.4f, 0.2f, 0.1f);
            lineRenderer.startWidth = 10f;
            lineRenderer.endWidth = 10f;
            lineRenderer.numCornerVertices = 5;
            lineRenderer.numCapVertices = 5;
            lineRenderer.enabled = true;
        }

        Debug.Log("РЕЖИМ РИСОВАНИЯ: Нажмите ЛКМ чтобы рисовать, ПРОБЕЛ для завершения");
    }

   void UpdateDrawingMode()
{
    if (!isDrawingMode) return;
    if (lineRenderer == null) return;

    lineRenderer.enabled = true;

    if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        // Используем Physics.Raycast чтобы попасть прямо в террейн
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 worldPos = hit.point;
            worldPos.y = hit.point.y + 0.5f; // чуть выше поверхности чтобы труба не уходила в землю

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
        return;
    }

    isDrawingMode = false;
    isBuilding = false;

    // Анализируем маршрут
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
        GameObject finalEnd = new GameObject("Final_PowerPlant");
        finalEnd.transform.position = endPos;

        GameObject endPlatform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        endPlatform.transform.SetParent(finalEnd.transform);
        endPlatform.transform.localPosition = new Vector3(0, 0.1f, 0);
        endPlatform.transform.localScale = new Vector3(15f, 0.5f, 15f);
        endPlatform.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.2f);
        Destroy(endPlatform.GetComponent<Collider>());

        GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
        building.transform.SetParent(finalEnd.transform);
        building.transform.localPosition = new Vector3(0, 6f, 0);
        building.transform.localScale = new Vector3(12f, 10f, 10f);
        building.GetComponent<Renderer>().material.color = new Color(0.8f, 0.8f, 0.8f);
        Destroy(building.GetComponent<Collider>());

        GameObject chimney = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        chimney.transform.SetParent(finalEnd.transform);
        chimney.transform.localPosition = new Vector3(3f, 12f, 1f);
        chimney.transform.localScale = new Vector3(1.5f, 8f, 1.5f);
        chimney.GetComponent<Renderer>().material.color = new Color(0.4f, 0.4f, 0.4f);
        Destroy(chimney.GetComponent<Collider>());

        GameObject chimney2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        chimney2.transform.SetParent(finalEnd.transform);
        chimney2.transform.localPosition = new Vector3(-3f, 12f, 1f);
        chimney2.transform.localScale = new Vector3(1.5f, 8f, 1.5f);
        chimney2.GetComponent<Renderer>().material.color = new Color(0.4f, 0.4f, 0.4f);
        Destroy(chimney2.GetComponent<Collider>());

        Debug.Log("Газопровод подключен к ТЭЦ!");
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