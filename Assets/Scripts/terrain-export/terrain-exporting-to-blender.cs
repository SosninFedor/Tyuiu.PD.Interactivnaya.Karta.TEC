using UnityEngine;
using System.IO;

public class TerrainExporter : MonoBehaviour
{
    public Terrain terrain;
    
    // Добавляем метод для вызова из редактора
    [ContextMenu("Export Terrain")]
    public void ExportTerrain()
    {
        if (terrain == null)
        {
            Debug.LogError("Terrain not assigned!");
            return;
        }
        
        TerrainData data = terrain.terrainData;
        int w = data.heightmapResolution;
        int h = data.heightmapResolution;
        
        float[,] heights = data.GetHeights(0, 0, w, h);
        
        // Сохраняем в папку проекта
        string filePath = Application.dataPath + "/terrain.raw";
        
        using (FileStream file = File.Create(filePath))
        {
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float height = heights[y, x];
                    short rawHeight = (short)(height * 65535.0f);
                    byte[] bytes = System.BitConverter.GetBytes(rawHeight);
                    file.Write(bytes, 0, 2);
                }
            }
        }
        
        Debug.Log("Terrain exported to: " + filePath);
    }
}