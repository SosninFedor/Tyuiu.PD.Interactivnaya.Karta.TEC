using UnityEngine;
using System.Collections;

public class PowerPlantLauncher : MonoBehaviour
{
    public static PowerPlantLauncher Instance;

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

        // Находим финальную точку ТЭЦ
        GameObject powerPlant = GameObject.Find("Final_PowerPlant");
        if (powerPlant == null)
        {
            Debug.LogWarning("Final_PowerPlant не найден!");
            yield break;
        }

        // 1. Свечение здания
        StartCoroutine(GlowBuilding(powerPlant));

        yield return new WaitForSeconds(0.5f);

        // 2. Дым из труб
        foreach (Transform child in powerPlant.transform)
        {
            if (child.name == "Cylinder" || child.localPosition.y > 1.5f)
            {
                StartCoroutine(SpawnSmoke(child.position + Vector3.up * 1.5f));
            }
        }

        // Дым из всех дочерних цилиндров (трубы)
        StartCoroutine(SpawnSmoke(powerPlant.transform.position + new Vector3(0.6f, 3.5f, 0.2f)));
        StartCoroutine(SpawnSmoke(powerPlant.transform.position + new Vector3(-0.6f, 3.5f, 0.2f)));

        yield return new WaitForSeconds(0.5f);

        // 3. Вспышка света
        StartCoroutine(FlashLight(powerPlant.transform.position));

        // 4. Искры вокруг
        StartCoroutine(SpawnSparks(powerPlant.transform.position));

        Debug.Log("✅ ТЭЦ запущена!");
    }

    // Свечение здания — мигает жёлтым
    IEnumerator GlowBuilding(GameObject powerPlant)
    {
        Renderer[] renderers = powerPlant.GetComponentsInChildren<Renderer>();
        float duration = 3f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float glow = (Mathf.Sin(t * 8f) + 1f) / 2f;
            Color glowColor = Color.Lerp(Color.white, new Color(1f, 0.8f, 0f), glow);

            foreach (Renderer r in renderers)
            {
                r.material.EnableKeyword("_EMISSION");
                r.material.SetColor("_EmissionColor", glowColor * glow * 2f);
            }
            yield return null;
        }

        // Оставляем стабильное жёлтое свечение
        foreach (Renderer r in renderers)
        {
            r.material.SetColor("_EmissionColor", new Color(1f, 0.6f, 0f) * 0.8f);
        }
    }

    // Дым из трубы
    IEnumerator SpawnSmoke(Vector3 position)
    {
        for (int i = 0; i < 30; i++)
        {
            GameObject smoke = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            smoke.name = "Smoke";
            Destroy(smoke.GetComponent<Collider>());

            float size = Random.Range(0.2f, 0.6f);
            smoke.transform.localScale = Vector3.one * size;
            smoke.transform.position = position + new Vector3(
                Random.Range(-0.2f, 0.2f), 0,
                Random.Range(-0.2f, 0.2f)
            );

            Renderer r = smoke.GetComponent<Renderer>();
            float gray = Random.Range(0.6f, 0.9f);
            r.material.color = new Color(gray, gray, gray, 0.8f);

            // Стандартный шейдер с прозрачностью
            r.material.SetFloat("_Mode", 2);
            r.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            r.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            r.material.EnableKeyword("_ALPHABLEND_ON");
            r.material.renderQueue = 3000;

            StartCoroutine(AnimateSmoke(smoke));

            yield return new WaitForSeconds(0.15f);
        }
    }

    IEnumerator AnimateSmoke(GameObject smoke)
    {
        if (smoke == null) yield break;

        float lifetime = Random.Range(2f, 4f);
        float t = 0f;
        Vector3 startPos = smoke.transform.position;
        Vector3 startScale = smoke.transform.localScale;
        float driftX = Random.Range(-0.3f, 0.3f);

        while (t < lifetime && smoke != null)
        {
            t += Time.deltaTime;
            float progress = t / lifetime;

            // Поднимается вверх и дрейфует
            smoke.transform.position = startPos + new Vector3(
                driftX * progress,
                progress * 4f,
                0
            );

            // Увеличивается и исчезает
            float scale = Mathf.Lerp(startScale.x, startScale.x * 3f, progress);
            smoke.transform.localScale = Vector3.one * scale;

            // Прозрачность
            Renderer r = smoke.GetComponent<Renderer>();
            if (r != null)
            {
                Color c = r.material.color;
                c.a = Mathf.Lerp(0.8f, 0f, progress);
                r.material.color = c;
            }

            yield return null;
        }

        if (smoke != null) Destroy(smoke);
    }

    // Вспышка — точечный свет
    IEnumerator FlashLight(Vector3 position)
    {
        GameObject lightObj = new GameObject("PowerLight");
        Light light = lightObj.AddComponent<Light>();
        lightObj.transform.position = position + Vector3.up * 2f;

        light.type = LightType.Point;
        light.color = new Color(1f, 0.8f, 0.2f);
        light.range = 15f;
        light.intensity = 0f;

        // Нарастание
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            light.intensity = Mathf.Lerp(0f, 8f, t);
            yield return null;
        }

        // Мигание
        for (int i = 0; i < 5; i++)
        {
            light.intensity = 8f;
            yield return new WaitForSeconds(0.1f);
            light.intensity = 2f;
            yield return new WaitForSeconds(0.1f);
        }

        // Стабильное свечение
        light.intensity = 3f;
    }

    // Искры вокруг ТЭЦ
    IEnumerator SpawnSparks(Vector3 center)
    {
        for (int i = 0; i < 20; i++)
        {
            GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spark.name = "Spark";
            Destroy(spark.GetComponent<Collider>());
            spark.transform.localScale = Vector3.one * 0.1f;

            Vector3 offset = new Vector3(
                Random.Range(-2f, 2f),
                Random.Range(0f, 2f),
                Random.Range(-2f, 2f)
            );
            spark.transform.position = center + offset;

            Renderer r = spark.GetComponent<Renderer>();
            r.material.color = new Color(1f, 0.6f, 0f);
            r.material.EnableKeyword("_EMISSION");
            r.material.SetColor("_EmissionColor", new Color(1f, 0.4f, 0f) * 3f);

            StartCoroutine(AnimateSpark(spark));
        }
        yield return null;
    }

    IEnumerator AnimateSpark(GameObject spark)
    {
        if (spark == null) yield break;

        float lifetime = Random.Range(0.5f, 1.5f);
        float t = 0f;
        Vector3 velocity = new Vector3(
            Random.Range(-2f, 2f),
            Random.Range(3f, 6f),
            Random.Range(-2f, 2f)
        );

        while (t < lifetime && spark != null)
        {
            t += Time.deltaTime;
            velocity += Vector3.down * 9.8f * Time.deltaTime;
            spark.transform.position += velocity * Time.deltaTime;

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
}