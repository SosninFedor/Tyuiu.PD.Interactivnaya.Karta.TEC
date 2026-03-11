using UnityEngine;

public class RouteAnalyzer : MonoBehaviour
{
    public static RouteAnalyzer Instance;

    public float totalLength { get; private set; }
    public int totalBends { get; private set; }
    public string grade { get; private set; }
    public int stars { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public void Analyze(LineRenderer lineRenderer)
    {
        totalLength = 0f;
        totalBends = 0;

        int count = lineRenderer.positionCount;

        // Считаем длину
        for (int i = 0; i < count - 1; i++)
        {
            Vector3 a = lineRenderer.GetPosition(i);
            Vector3 b = lineRenderer.GetPosition(i + 1);
            totalLength += Vector3.Distance(a, b);
        }

        // Считаем изгибы (угол между сегментами > 20 градусов)
        for (int i = 1; i < count - 1; i++)
        {
            Vector3 prev = lineRenderer.GetPosition(i - 1);
            Vector3 curr = lineRenderer.GetPosition(i);
            Vector3 next = lineRenderer.GetPosition(i + 1);

            Vector3 dir1 = (curr - prev).normalized;
            Vector3 dir2 = (next - curr).normalized;
            float angle = Vector3.Angle(dir1, dir2);

            if (angle > 20f)
                totalBends++;
        }

        // Оценка
        if (totalBends <= 3)
        {
            grade = "Отлично";
            stars = 3;
        }
        else if (totalBends <= 7)
        {
            grade = "Хорошо";
            stars = 2;
        }
        else
        {
            grade = "Удовлетворительно";
            stars = 1;
        }

        Debug.Log($"Длина: {totalLength:F0}м | Изгибы: {totalBends} | Оценка: {grade}");
    }
}