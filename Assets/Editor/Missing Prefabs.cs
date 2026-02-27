using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ReconnectPrefabs : EditorWindow
{
    [MenuItem("Tools/Reconnect Missing Prefabs")]
    static void Reconnect()
    {
        // Найти все объекты с Missing Prefab
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int fixedCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            // Проверить префаб статус
            PrefabInstanceStatus status = PrefabUtility.GetPrefabInstanceStatus(obj);
            if (status == PrefabInstanceStatus.MissingAsset)
            {
                // Найти имя префаба
                string prefabName = obj.name.Replace("(Missing Prefab)", "").Trim();
                
                // Поиск префаба в проекте
                string[] guids = AssetDatabase.FindAssets(prefabName + " t:Prefab");
                
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    
                    if (prefab != null)
                    {
                        // Переподключить префаб
                        PrefabUtility.SaveAsPrefabAsset(obj, path);
                        fixedCount++;
                        Debug.Log($"Fixed: {obj.name} -> {path}");
                    }
                }
            }
        }
        
        Debug.Log($"Fixed {fixedCount} missing prefabs!");
    }
}