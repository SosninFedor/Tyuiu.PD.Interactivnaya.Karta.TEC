using UnityEngine;

public class PipeBuilder : MonoBehaviour
{
    public static PipeBuilder Instance;

    [Header("Префабы")]
    public GameObject pipeSegmentPrefab;
    public GameObject connectorPrefab;

    [Header("Настройки надземной трубы")]
    public float overgroundRadius = 1.5f;
    public Color overgroundColor = new Color(0.4f, 0.2f, 0.1f);

    [Header("Настройки подземной трубы")]
    public float undergroundRadius = 1.2f;
    public Color undergroundColor = new Color(0.45f, 0.45f, 0.5f);

    void Awake()
    {
        Instance = this;
    }

    public void BuildPipelineFromLine(LineRenderer lineRenderer)
    {
        if (lineRenderer.positionCount < 2)
        {
            Debug.Log("Слишком мало точек для трубы");
            return;
        }

        // Читаем текущий режим напрямую из BuildManager
        bool isUnderground = BuildManager.Instance != null &&
                             BuildManager.Instance.currentPipeMode == BuildManager.PipeMode.Underground;

        string modeName = isUnderground ? "Подземный" : "Надземный";
        GameObject pipelineParent = new GameObject($"GasPipeline_{modeName}");

        for (int i = 0; i < lineRenderer.positionCount - 1; i++)
        {
            Vector3 start = lineRenderer.GetPosition(i);
            Vector3 end   = lineRenderer.GetPosition(i + 1);

            CreatePipeSegment(start, end, pipelineParent.transform, isUnderground);

            if (i < lineRenderer.positionCount - 2 && connectorPrefab != null)
                CreateConnector(end, pipelineParent.transform, isUnderground);
        }

        Debug.Log($"Построено {lineRenderer.positionCount - 1} сегментов [{modeName}]");
    }

    void CreatePipeSegment(Vector3 start, Vector3 end, Transform parent, bool isUnderground)
    {
        float  radius = isUnderground ? undergroundRadius : overgroundRadius;
        Color  color  = isUnderground ? undergroundColor  : overgroundColor;

        float   distance = Vector3.Distance(start, end);
        Vector3 middle   = (start + end) / 2f;

        GameObject pipe = Instantiate(pipeSegmentPrefab, middle, Quaternion.identity, parent);
        pipe.transform.rotation   = Quaternion.LookRotation(end - start) * Quaternion.Euler(90f, 0f, 0f);
        pipe.transform.localScale = new Vector3(radius, distance / 2f, radius);

        // Если труба подземная — слегка опускаем под землю
        if (isUnderground)
        {
            Vector3 pos = pipe.transform.position;
            pos.y -= radius * 0.5f;
            pipe.transform.position = pos;
        }

        Renderer rend = pipe.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material = new Material(rend.material);
            rend.material.color = color;
            rend.material.SetFloat("_Metallic",   isUnderground ? 0.3f : 0.7f);
            rend.material.SetFloat("_Smoothness", isUnderground ? 0.2f : 0.5f);
        }

        // Коллайдер в локальных координатах цилиндра
        CapsuleCollider col = pipe.GetComponent<CapsuleCollider>();
        if (col == null) col = pipe.AddComponent<CapsuleCollider>();
        col.radius    = 0.5f;
        col.height    = 1f;
        col.direction = 1; // Y-axis
    }

    void CreateConnector(Vector3 position, Transform parent, bool isUnderground)
    {
        float  radius = isUnderground ? undergroundRadius : overgroundRadius;
        Color  color  = isUnderground ? undergroundColor  : overgroundColor;

        GameObject connector = Instantiate(connectorPrefab, position, Quaternion.identity, parent);
        connector.transform.localScale = Vector3.one * radius * 2.2f;

        if (isUnderground)
        {
            Vector3 pos = connector.transform.position;
            pos.y -= radius * 0.5f;
            connector.transform.position = pos;
        }

        Renderer rend = connector.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material = new Material(rend.material);
            rend.material.color = color;
        }
    }
}