using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerPlantLauncher : MonoBehaviour
{
    public static PowerPlantLauncher Instance;

    [Header("ТЭЦ из сцены")]
    public GameObject realPowerPlant;

    private List<GameObject> activeEffects = new List<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    public void LaunchPowerPlant()
    {
        StartCoroutine(LaunchSequence());
    }

    IEnumerator LaunchSequence()
    {
        Debug.Log("🏭 Запуск ТЭЦ...");

        // Сначала пробуем реальный ТЭЦ из сцены
        GameObject powerPlant = realPowerPlant;

        // Если не назначен — ищем программно созданный
        if (powerPlant == null)
            powerPlant = GameObject.Find("Final_PowerPlant");
        
        // Если всё ещё не нашли - ищем LowPolyTec
        if (powerPlant == null)
            powerPlant = GameObject.Find("LowPolyTec");

        if (powerPlant == null)
        {
            Debug.LogWarning("❌ ТЭЦ не найден! Убедитесь, что объект называется 'LowPolyTec' или 'Final_PowerPlant'");
            yield break;
        }

        Debug.Log($"✅ ТЭЦ найден: {powerPlant.name} на позиции {powerPlant.transform.position}");

        // Движение камеры
        if (CameraController.Instance != null)
        {
            Debug.Log("📷 Камера начинает движение к ТЭЦ");
            CameraController.Instance.MoveToTec();
        }

        // 1. Свечение здания
        StartCoroutine(GlowBuilding(powerPlant));

        yield return new WaitForSeconds(0.5f);

        // 2. Дым из труб - улучшенный поиск труб
        List<Vector3> pipePositions = FindPipePositions(powerPlant);
        
        foreach (Vector3 pos in pipePositions)
        {
            StartCoroutine(SpawnSmoke(pos));
        }

        yield return new WaitForSeconds(0.5f);

        // 3. Вспышка света
        StartCoroutine(FlashLight(powerPlant.transform.position));

        // 4. Искры вокруг
        StartCoroutine(SpawnSparks(powerPlant.transform.position));

        Debug.Log("✅ ТЭЦ запущена! Эффекты активированы");
    }
    
    // Поиск позиций труб на ТЭЦ - ИСПРАВЛЕННАЯ ВЕРСИЯ
    List<Vector3> FindPipePositions(GameObject powerPlant)
    {
        List<Vector3> positions = new List<Vector3>();
        
        // Ищем все объекты, которые могут быть трубами
        Renderer[] renderers = powerPlant.GetComponentsInChildren<Renderer>();
        
        foreach (Renderer rend in renderers)
        {
            string objName = rend.gameObject.name.ToLower();
            
            // Если объект похож на трубу (цилиндр или высокий объект)
            if (objName.Contains("cylinder") || 
                objName.Contains("pipe") ||
                objName.Contains("chimney") ||
                objName.Contains("tube") ||
                (rend.bounds.size.y > rend.bounds.size.x * 1.5f && rend.bounds.size.y > 1f))
            {
                // Берем верхнюю часть трубы
                Vector3 topPos = rend.transform.position;
                topPos.y += rend.bounds.size.y * 0.5f;
                
                positions.Add(topPos);
                Debug.Log($"Найдена труба: {rend.gameObject.name} на позиции {topPos}");
            }
        }
        
        // Если ничего не нашли, проверяем дочерние объекты с высоким Y
        if (positions.Count == 0)
        {
            foreach (Transform child in powerPlant.transform)
            {
                if (child.localPosition.y > 1.5f)
                {
                    positions.Add(child.position + Vector3.up * 0.5f);
                    Debug.Log($"Найден потенциальный выход трубы: {child.name}");
                }
            }
        }
        
        // Если всё ещё ничего не нашли - добавляем стандартные позиции
        if (positions.Count == 0)
        {
            Debug.Log("Трубы не найдены, использую стандартные позиции");
            positions.Add(powerPlant.transform.position + new Vector3(0.6f, 3.5f, 0.2f));
            positions.Add(powerPlant.transform.position + new Vector3(-0.6f, 3.5f, 0.2f));
            positions.Add(powerPlant.transform.position + new Vector3(0f, 4f, 0.5f));
        }
        
        return positions;
    }

    // Свечение здания
    IEnumerator GlowBuilding(GameObject powerPlant)
    {
        Renderer[] renderers = powerPlant.GetComponentsInChildren<Renderer>();
        
        if (renderers.Length == 0)
        {
            Debug.LogWarning("Нет рендереров для свечения!");
            yield break;
        }
        
        float duration = 3f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float glow = (Mathf.Sin(t * 8f) + 1f) / 2f;
            Color glowColor = Color.Lerp(Color.white, new Color(1f, 0.8f, 0f), glow);

            foreach (Renderer r in renderers)
            {
                if (r.material.HasProperty("_EmissionColor"))
                {
                    r.material.EnableKeyword("_EMISSION");
                    r.material.SetColor("_EmissionColor", glowColor * glow * 2f);
                }
            }
            yield return null;
        }

        // Оставляем стабильное жёлтое свечение
        foreach (Renderer r in renderers)
        {
            if (r.material.HasProperty("_EmissionColor"))
            {
                r.material.SetColor("_EmissionColor", new Color(1f, 0.6f, 0f) * 0.8f);
            }
        }
    }

    // Дым из трубы
    IEnumerator SpawnSmoke(Vector3 position)
    {
        Debug.Log($"💨 Создание дыма на позиции {position}");
        
        for (int i = 0; i < 20; i++)
        {
            GameObject smoke = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            smoke.name = "Smoke";
            smoke.transform.SetParent(null);
            Destroy(smoke.GetComponent<Collider>());

            float size = Random.Range(0.3f, 0.8f);
            smoke.transform.localScale = Vector3.one * size;
            smoke.transform.position = position + new Vector3(
                Random.Range(-0.3f, 0.3f), 
                i * 0.1f,
                Random.Range(-0.3f, 0.3f)
            );

            Renderer r = smoke.GetComponent<Renderer>();
            float gray = Random.Range(0.5f, 0.8f);
            r.material.color = new Color(gray, gray, gray, 0.7f);
            
            // Настройка прозрачности
            r.material.SetFloat("_Mode", 3);
            r.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            r.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            r.material.EnableKeyword("_ALPHABLEND_ON");
            r.material.renderQueue = 3000;

            StartCoroutine(AnimateSmoke(smoke));

            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator AnimateSmoke(GameObject smoke)
    {
        if (smoke == null) yield break;

        float lifetime = Random.Range(1.5f, 3f);
        float t = 0f;
        Vector3 startPos = smoke.transform.position;
        Vector3 startScale = smoke.transform.localScale;
        float driftX = Random.Range(-0.5f, 0.5f);
        float driftZ = Random.Range(-0.5f, 0.5f);

        while (t < lifetime && smoke != null)
        {
            t += Time.deltaTime;
            float progress = t / lifetime;

            smoke.transform.position = startPos + new Vector3(
                driftX * progress,
                progress * 5f,
                driftZ * progress
            );

            float scale = Mathf.Lerp(startScale.x, startScale.x * 4f, progress);
            smoke.transform.localScale = Vector3.one * scale;

            Renderer r = smoke.GetComponent<Renderer>();
            if (r != null)
            {
                Color c = r.material.color;
                c.a = Mathf.Lerp(0.7f, 0f, progress);
                r.material.color = c;
            }

            yield return null;
        }

        if (smoke != null) Destroy(smoke);
    }

    // Вспышка света
    IEnumerator FlashLight(Vector3 position)
    {
        GameObject lightObj = new GameObject("PowerLight");
        Light light = lightObj.AddComponent<Light>();
        lightObj.transform.position = position + Vector3.up * 3f;

        light.type = LightType.Point;
        light.color = new Color(1f, 0.7f, 0.2f);
        light.range = 20f;
        light.intensity = 0f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            light.intensity = Mathf.Lerp(0f, 10f, t);
            yield return null;
        }

        for (int i = 0; i < 6; i++)
        {
            light.intensity = 10f;
            yield return new WaitForSeconds(0.08f);
            light.intensity = 3f;
            yield return new WaitForSeconds(0.08f);
        }

        t = 0f;
        while (t < 2f)
        {
            t += Time.deltaTime;
            light.intensity = Mathf.Lerp(5f, 0f, t / 2f);
            yield return null;
        }
        
        Destroy(lightObj);
    }

    // Искры вокруг ТЭЦ
    IEnumerator SpawnSparks(Vector3 center)
    {
        Debug.Log($"✨ Создание искр вокруг {center}");
        
        for (int i = 0; i < 30; i++)
        {
            GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spark.name = "Spark";
            Destroy(spark.GetComponent<Collider>());
            spark.transform.localScale = Vector3.one * 0.12f;

            Vector3 offset = new Vector3(
                Random.Range(-3f, 3f),
                Random.Range(0f, 4f),
                Random.Range(-3f, 3f)
            );
            spark.transform.position = center + offset;

            Renderer r = spark.GetComponent<Renderer>();
            r.material.color = new Color(1f, 0.5f + Random.Range(0f, 0.5f), 0f);
            r.material.EnableKeyword("_EMISSION");
            r.material.SetColor("_EmissionColor", new Color(1f, 0.5f, 0f) * 2f);

            StartCoroutine(AnimateSpark(spark));
            
            yield return new WaitForSeconds(0.03f);
        }
    }

    IEnumerator AnimateSpark(GameObject spark)
    {
        if (spark == null) yield break;

        float lifetime = Random.Range(0.8f, 1.8f);
        float t = 0f;
        Vector3 velocity = new Vector3(
            Random.Range(-3f, 3f),
            Random.Range(4f, 8f),
            Random.Range(-3f, 3f)
        );

        while (t < lifetime && spark != null)
        {
            t += Time.deltaTime;
            velocity += Vector3.down * 12f * Time.deltaTime;
            spark.transform.position += velocity * Time.deltaTime;
            
            float scale = Mathf.Lerp(0.12f, 0f, t / lifetime);
            spark.transform.localScale = Vector3.one * scale;

            Renderer r = spark.GetComponent<Renderer>();
            if (r != null)
            {
                Color c = r.material.color;
                c.a = Mathf.Lerp(1f, 0f, t / lifetime);
                r.material.color = c;
            }

            yield return null;
        }

        if (spark != null) Destroy(spark);
    }
    
    void OnDestroy()
    {
        foreach (GameObject effect in activeEffects)
        {
            if (effect != null) Destroy(effect);
        }
    }
}