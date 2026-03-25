using UnityEngine;

public class PipeBuilder : MonoBehaviour
{
    public static PipeBuilder Instance;
    
    [Header("Префабы")]
    public GameObject pipeSegmentPrefab;
    public GameObject connectorPrefab;
    
    [Header("Настройки")]
    public float pipeRadius = 6f;
    public Color pipeColor = new Color(0.4f, 0.2f, 0.1f);
    
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
        
        GameObject pipelineParent = new GameObject("GasPipeline");
        
        for (int i = 0; i < lineRenderer.positionCount - 1; i++)
        {
            Vector3 start = lineRenderer.GetPosition(i);
            Vector3 end = lineRenderer.GetPosition(i + 1);
            
            CreatePipeSegment(start, end, pipelineParent.transform);
            
            if (i < lineRenderer.positionCount - 2 && connectorPrefab != null)
            {
                CreateConnector(end, pipelineParent.transform);
            }
        }
        
        Debug.Log($"Построено {lineRenderer.positionCount - 1} сегментов трубы");
    }
    
    void CreatePipeSegment(Vector3 start, Vector3 end, Transform parent)
    {
        float distance = Vector3.Distance(start, end);
        Vector3 middle = (start + end) / 2;
        
        GameObject pipe = Instantiate(pipeSegmentPrefab, middle, Quaternion.identity, parent);
        
        pipe.transform.rotation = Quaternion.LookRotation(end - start) * Quaternion.Euler(90, 0, 0);
        pipe.transform.localScale = new Vector3(pipeRadius, distance / 2, pipeRadius);
        
        Renderer renderer = pipe.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = pipeColor;
            renderer.material.SetFloat("_Metallic", 0.5f);
        }
        
        if (pipe.GetComponent<CapsuleCollider>() == null)
        {
            var collider = pipe.AddComponent<CapsuleCollider>();
            collider.radius = pipeRadius;
            collider.height = distance;
            collider.direction = 2;
        }
    }
    
    void CreateConnector(Vector3 position, Transform parent)
    {
        GameObject connector = Instantiate(connectorPrefab, position, Quaternion.identity, parent);
        connector.transform.localScale = Vector3.one * pipeRadius * 2;
        connector.GetComponent<Renderer>().material.color = pipeColor;
    }
}