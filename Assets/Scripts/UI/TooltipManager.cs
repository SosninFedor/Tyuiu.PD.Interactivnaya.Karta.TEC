using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;
    
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipText;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void ShowTooltip(string message)
    {
        if (tooltipPanel != null) 
            tooltipPanel.SetActive(true);
        if (tooltipText != null)
            tooltipText.text = message;
    }
    
    public void HideTooltip()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }
}