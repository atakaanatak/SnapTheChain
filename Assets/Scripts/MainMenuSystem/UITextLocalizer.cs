using UnityEngine;
using TMPro;

public class UITextLocalizer : MonoBehaviour
{
    [Header("ID Key From CSV")]
    public string textID;

    private TMP_Text myText;

    private void Awake()
    {
        myText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        UpdateText();
    }

    private void Start()
    {
        UpdateText();
    }

    public void UpdateText()
    {
        if (myText == null)
        {
            myText = GetComponent<TMP_Text>();
        }

        if (LocalizationManager.Instance != null && myText != null)
        {
            myText.text = LocalizationManager.Instance.GetText(textID);
        }
    }
}