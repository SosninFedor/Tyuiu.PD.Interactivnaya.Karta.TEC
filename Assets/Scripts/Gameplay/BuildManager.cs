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
    
    private BuildingType currentBuildingType = BuildingType.None;
    private GameObject currentPreview;
    private bool isBuilding = false;

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