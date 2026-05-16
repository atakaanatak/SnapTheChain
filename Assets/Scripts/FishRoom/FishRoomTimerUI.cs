using UnityEngine;
using TMPro;

public class FishRoomTimerUI : MonoBehaviour
{
    public static FishRoomTimerUI Instance; 
    
    [HideInInspector] public TextMeshProUGUI timerText;

    private void Awake()
    {
        Instance = this;
        timerText = GetComponent<TextMeshProUGUI>();
        
        gameObject.SetActive(false); 
    }
}