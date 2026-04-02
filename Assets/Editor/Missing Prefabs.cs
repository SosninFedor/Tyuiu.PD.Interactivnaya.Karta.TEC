using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ReconnectPrefabs : EditorWindow
{
    [MenuItem("Tools/Reconnect Missing Prefabs")]
    static void Reconnect()
    {
        // Используем FindObjectsByType вместо устаревшего FindObjectsOfType
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int fixedCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj == null) continue;
            
            // Исправлено: убираем устаревший Disconnected статус
            PrefabInstanceStatus status = PrefabUtility.GetPrefabInstanceStatus(obj);
            
            // Проверка только на MissingAsset
            if (status == PrefabInstanceStatus.MissingAsset)
            {
                // Получаем имя префаба из имени объекта
                string prefabName = obj.name;
                prefabName = prefabName.Replace("(Missing Prefab)", "").Trim();
                
                if (string.IsNullOrEmpty(prefabName)) continue;
                
                // Поиск префаба в проекте
                string[] guids = AssetDatabase.FindAssets(prefabName + " t:Prefab");
                
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    
                    if (prefab != null)
                    {
                        try
                        {
                            // Переподключаем префаб
                            PrefabUtility.SaveAsPrefabAsset(obj, path);
                            fixedCount++;
                            Debug.Log($"Fixed: {obj.name} -> {path}");
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogWarning($"Could not fix {obj.name}: {e.Message}");
                        }
                    }
                }
            }
        }
        
        Debug.Log($"Fixed {fixedCount} missing prefabs!");
        
        if (fixedCount == 0)
        {
            Debug.Log("No missing prefabs found. Everything is OK!");
        }
    }
    
    // Альтернативный метод - просто удалить все missing prefabs
    [MenuItem("Tools/Remove All Missing Prefabs")]
    static void RemoveMissingPrefabs()
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int removedCount = 0;
        
        List<GameObject> toRemove = new List<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj == null) continue;
            
            PrefabInstanceStatus status = PrefabUtility.GetPrefabInstanceStatus(obj);
            if (status == PrefabInstanceStatus.MissingAsset)
            {
                toRemove.Add(obj);
            }
        }
        
        foreach (GameObject obj in toRemove)
        {
            Debug.Log($"Removing missing prefab: {obj.name}");
            DestroyImmediate(obj);
            removedCount++;
        }
        
        Debug.Log($"Removed {removedCount} missing prefabs!");
        
        if (removedCount > 0)
        {
            EditorUtility.DisplayDialog("Clean Complete", 
                $"Removed {removedCount} missing prefabs from the scene!", 
                "OK");
        }
        else
        {
            Debug.Log("No missing prefabs found!");
        }
    }
}