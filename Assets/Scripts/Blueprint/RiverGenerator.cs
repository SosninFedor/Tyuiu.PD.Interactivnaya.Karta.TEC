using UnityEngine;
using System.Collections;

public class RiverGenerator : MonoBehaviour
{
    public GameObject segmentPrefab;
    public int segmentCount = 10;
    public float segmentLength = 2f;
    public float curveIntensity = 3f;
    public float width = 1f;
    
    private Coroutine updateCoroutine;

    // Вызывается при изменении значений в инспекторе
    void OnValidate()
    {
        // Отменяем предыдущий запуск
        if (updateCoroutine != null)
            StopCoroutine(updateCoroutine);
        
        // Запускаем с задержкой
        updateCoroutine = StartCoroutine(DelayedGenerate());
    }

    IEnumerator DelayedGenerate()
    {
        // Ждем кадр, чтобы избежать ошибок
        yield return null;
        
        // Удаляем старые сегменты
        foreach (Transform child in transform)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }

        // Создаем новые
        for (int i = 0; i < segmentCount; i++)
        {
            GameObject segment = Instantiate(segmentPrefab, transform);
            
            float x = i * segmentLength;
            float z = Mathf.Sin(i * 0.5f) * curveIntensity;
            
            segment.transform.position = new Vector3(x, 0, z);
            
            if (i > 0)
            {
                Vector3 direction = segment.transform.position - 
                                   transform.GetChild(i - 1).position;
                segment.transform.rotation = Quaternion.LookRotation(direction);
            }
            
            segment.transform.localScale = new Vector3(width, 0.1f, segmentLength);
        }
    }
}