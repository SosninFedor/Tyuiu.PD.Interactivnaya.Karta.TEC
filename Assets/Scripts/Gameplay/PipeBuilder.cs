using UnityEngine;

public class PipeBuilder : MonoBehaviour
{
    public static PipeBuilder Instance;
    
    [Header("Префабы")]
    public GameObject pipeSegmentPrefab; // Цилиндр
    public GameObject connectorPrefab;   // Шар для стыков (опционально)
    
    [Header("Настройки")]
    public float pipeRadius = 0.3f;
    public Color pipeColor = new Color(0.4f, 0.2f, 0.1f); // Коричневый
    
    void Awake()
    {
        Instance = this;
    }
    
    // Главный метод - вызываем когда маршрут утвержден
    public void BuildPipelineFromLine(LineRenderer lineRenderer)
{
    if (lineRenderer.positionCount < 2) return;
    
    GameObject pipelineParent = new GameObject("GasPipeline");
    
    // Берем первую и последнюю точку
    Vector3 start = lineRenderer.GetPosition(0);
    Vector3 end = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
    
    // Создаем одну трубу
    GameObject pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
    pipe.transform.SetParent(pipelineParent.transform);
    pipe.name = "MainPipe";
    
    // Позиция по центру
    pipe.transform.position = (start + end) / 2;
    
    // Поворот к конечной точке
    pipe.transform.rotation = Quaternion.LookRotation(end - start) * Quaternion.Euler(90, 0, 0);
    
    // Длина = расстояние между точками
    float distance = Vector3.Distance(start, end);
    pipe.transform.localScale = new Vector3(0.3f, distance / 2, 0.3f);
    
    // Материал
    Renderer rend = pipe.GetComponent<Renderer>();
    rend.material.color = new Color(0.4f, 0.2f, 0.1f);
    rend.material.SetFloat("_Metallic", 0.7f);
    
    Debug.Log($"✅ Создана прямая труба длиной {distance}");
}
    
    void CreatePipeSegment(Vector3 start, Vector3 end, Transform parent)
    {
        float distance = Vector3.Distance(start, end);
        Vector3 middle = (start + end) / 2;
        
        // Создаем сегмент трубы
        GameObject pipe = Instantiate(pipeSegmentPrefab, middle, Quaternion.identity, parent);
        
        // Поворачиваем вдоль направления
        pipe.transform.rotation = Quaternion.LookRotation(end - start) * Quaternion.Euler(90, 0, 0);
        
        // Растягиваем по длине
        pipe.transform.localScale = new Vector3(pipeRadius, distance / 2, pipeRadius);
        
        // Красим
        Renderer renderer = pipe.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = pipeColor;
            // Если есть металлик
            renderer.material.SetFloat("_Metallic", 0.5f);
        }
        
        // Добавляем коллайдер для физики
        if (pipe.GetComponent<CapsuleCollider>() == null)
        {
            var collider = pipe.AddComponent<CapsuleCollider>();
            collider.radius = pipeRadius;
            collider.height = distance;
            collider.direction = 2; // Z-axis
        }
    }
    
    void CreateConnector(Vector3 position, Transform parent)
    {
        GameObject connector = Instantiate(connectorPrefab, position, Quaternion.identity, parent);
        connector.transform.localScale = Vector3.one * pipeRadius * 2;
        connector.GetComponent<Renderer>().material.color = pipeColor;
    }
    
    void CreateDisturbedGround(LineRenderer lineRenderer, Transform parent)
    {
        // Создаем "вспаханную" землю под трубой
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            Vector3 pos = lineRenderer.GetPosition(i);
            
            // Плоский цилиндр как след от стройки
            GameObject groundMark = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            groundMark.transform.position = pos - Vector3.up * 0.2f;
            groundMark.transform.localScale = new Vector3(1.5f, 0.1f, 1.5f);
            groundMark.transform.SetParent(parent);
            
            // Темно-коричневый цвет
            groundMark.GetComponent<Renderer>().material.color = new Color(0.3f, 0.15f, 0.05f);
            
            // Добавляем немного шума (мелкие кубики)
            for (int j = 0; j < 3; j++)
            {
                GameObject dirt = GameObject.CreatePrimitive(PrimitiveType.Cube);
                dirt.transform.position = pos + new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
                dirt.transform.localScale = Vector3.one * Random.Range(0.1f, 0.3f);
                dirt.transform.SetParent(parent);
                dirt.GetComponent<Renderer>().material.color = new Color(0.4f, 0.2f, 0.1f);
                
                // Немного поднимаем некоторые куски
                if (Random.value > 0.5f)
                    dirt.transform.position += Vector3.up * Random.Range(0.1f, 0.3f);
            }
        }
    }
}