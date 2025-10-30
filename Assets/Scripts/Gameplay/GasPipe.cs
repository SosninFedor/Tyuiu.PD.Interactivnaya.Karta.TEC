using UnityEngine;

public class GasPipe : MonoBehaviour
{
    [Header("Настройки газопровода")]
    public bool isMainPipeline = false;
    public bool isConnectedToStation = false;
    
    void Start()
    {
        // Если это основная магистраль - отмечаем
        if (isMainPipeline)
        {
            gameObject.tag = "Gas";
        }
    }
    
    public void ConnectToStation()
    {
        isConnectedToStation = true;
        Debug.Log("Газопровод подключен к станции!");
    }
    
    public void DisconnectFromStation()
    {
        isConnectedToStation = false;
        Debug.Log("Газопровод отключен от станции");
    }
    
    void OnDestroy()
    {
        // При удалении трубы уведомляем менеджер
        BuildManager.Instance?.OnPipeDestroyed(this);
    }
}