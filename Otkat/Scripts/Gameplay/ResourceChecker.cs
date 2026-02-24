using UnityEngine;

public class ResourceChecker : MonoBehaviour
{
    [Header("Радиусы проверки")]
    public float waterCheckRadius = 3f;
    public float gasCheckRadius = 3f;
    public float cityCheckRadius = 5f;
    
    public bool CheckWaterResources(Vector3 position)
    {
        return CheckResourceInRadius(position, waterCheckRadius, "Water");
    }
    
    public bool CheckGasResources(Vector3 position)
    {
        return CheckResourceInRadius(position, gasCheckRadius, "Gas");
    }
    
    public bool CheckCityResources(Vector3 position)
    {
        return CheckResourceInRadius(position, cityCheckRadius, "City");
    }
    
    public string GetMissingResources(Vector3 position)
    {
        string missing = "";
        
        if (!CheckWaterResources(position))
            missing += "Вода ";
        if (!CheckGasResources(position))
            missing += "Газ ";
        if (!CheckCityResources(position))
            missing += "Город ";
            
        return missing.Trim();
    }
    
    private bool CheckResourceInRadius(Vector3 position, float radius, string tag)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, radius);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag(tag))
                return true;
        }
        return false;
    }
}