using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    private Vector3 posCityView = new Vector3(-95.6f, 610.5f, 710.1f);
    private Vector3 rotCityView = new Vector3(22.6f, 30.8f, 0f);
    private Vector3 posGasView = new Vector3(1661.495f, 262.9132f, 1885.103f);
    private Vector3 rotGasView = new Vector3(18.667f, 49.847f, 0f);
    private Vector3 posTopView = new Vector3(1260.338f, 2004.193f, 1961.617f);
    private Vector3 rotTopView = new Vector3(78.312f, 359.654f, 0f);

    [Header("Настройки")]
    public float moveDuration = 2.5f;

    void Awake()
    {
        Instance = this;
    }

    public void StartIntro()
    {
        StartCoroutine(IntroSequence());
    }

    IEnumerator IntroSequence()
    {
        transform.position = posCityView;
        transform.rotation = Quaternion.Euler(rotCityView);

        // Задержку убрали — меню появляется сразу
        // yield return new WaitForSeconds(3f);

        if (UIManager.Instance != null)
            UIManager.Instance.ShowStartScreen();
        
        yield return null;
    }


    public void MoveToTopView(System.Action onComplete = null)
    {
    StartCoroutine(MoveTo(posTopView, rotTopView, 2f, onComplete));
    }

    public void MoveToGasLine(System.Action onComplete = null)
    {
    StartCoroutine(MoveTo(posGasView, rotGasView, moveDuration, onComplete));
    }

    public void MoveToCityView(System.Action onComplete = null)
    {
        StartCoroutine(MoveTo(posCityView, rotCityView, moveDuration, onComplete));
    }

    public void FlyAlongPipe(LineRenderer lineRenderer)
    {
        StartCoroutine(CinematicFlight(lineRenderer));
    }

    IEnumerator CinematicFlight(LineRenderer lineRenderer)
{
    if (lineRenderer.positionCount < 2) yield break;

    // Находим границы маршрута
    Vector3 pipeStart = lineRenderer.GetPosition(0);
    Vector3 pipeEnd = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
    Vector3 pipeCenter = (pipeStart + pipeEnd) / 2f;

    // Считаем максимальный размер маршрута по всем точкам
    float maxDist = 0f;
    for (int i = 0; i < lineRenderer.positionCount; i++)
    {
        float d = Vector3.Distance(pipeCenter, lineRenderer.GetPosition(i));
        if (d > maxDist) maxDist = d;
    }

    // Камера поднимается так чтобы весь маршрут влез в кадр
    float height = maxDist * 2.2f;
    Vector3 overviewPos = pipeCenter + Vector3.up * height;
    Quaternion overviewRot = Quaternion.LookRotation(pipeCenter - overviewPos);

    yield return StartCoroutine(MoveTo(overviewPos, overviewRot.eulerAngles, 2.5f));

    if (UIManager.Instance != null)
        UIManager.Instance.ShowSuccessPanel();
}

    IEnumerator MoveTo(Vector3 targetPos, Vector3 targetRot, float duration, System.Action onComplete = null)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(targetRot);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPos, targetPos, smooth);
            transform.rotation = Quaternion.Lerp(startRot, endRot, smooth);

            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = endRot;

        onComplete?.Invoke();

    }
    
}