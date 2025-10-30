using UnityEngine;

public class PowerPlant : MonoBehaviour
{
    [Header("Состояния станции")]
    public GameObject notBuiltState;
    public GameObject builtState; 
    public GameObject workingState;
    
    [Header("Радиус проверки ресурсов")]
    public float checkRadius = 3f;
    
    private bool hasWater = false;
    private bool hasGas = false;
    private bool hasCity = false;

    void Start()
    {
        SetState(notBuiltState);
    }
    
    void Update()
    {
        CheckResourceConditions();
        UpdatePowerPlantState();
    }
    
    void CheckResourceConditions()
    {
        hasWater = CheckForResource("Water");
        hasGas = CheckForResource("Gas");
        hasCity = CheckForResource("City");
    }
    
    bool CheckForResource(string resourceTag)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, checkRadius);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag(resourceTag))
                return true;
        }
        return false;
    }
    
    void UpdatePowerPlantState()
    {
        if (hasWater && hasGas && hasCity)
        {
            SetState(workingState);
        }
        else 
        {
            SetState(builtState);
        }
    }
    
    void SetState(GameObject stateToActivate)
    {
        if (notBuiltState != null) notBuiltState.SetActive(false);
        if (builtState != null) builtState.SetActive(false);
        if (workingState != null) workingState.SetActive(false);
        
        stateToActivate.SetActive(true);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}