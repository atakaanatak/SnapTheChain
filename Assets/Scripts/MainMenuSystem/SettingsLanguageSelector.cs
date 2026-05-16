using TMPro;
using UnityEngine;

public class SettingsLanguageSelector : MonoBehaviour
{
    [Header("Language Value Text")]
    public TMP_Text languageValueText;

    [Header("CSV IDs")]
    public string englishLanguageId = "ST_EN";
    public string turkishLanguageId = "ST_TR";

    private void Start()
    {
        UpdateLanguageView();
    }

    public void NextLanguage()
    {
        Debug.Log("Right language button clicked.");
        ToggleLanguage();
    }

    public void PreviousLanguage()
    {
        Debug.Log("Left language button clicked.");
        ToggleLanguage();
    }

    public void ToggleLanguage()
    {
        if (LocalizationManager.Instance == null)
        {
            Debug.LogWarning("LocalizationManager not found.");
            return;
        }

        LocalizationManager.Instance.ToggleLanguage();
        UpdateLanguageView();
    }

    public void UpdateLanguageView()
    {
        if (LocalizationManager.Instance == null)
            return;

        if (languageValueText == null)
        {
            Debug.LogWarning("Language Value Text is missing.");
            return;
        }

        string selectedLanguageId = LocalizationManager.Instance.isEnglish
            ? englishLanguageId
            : turkishLanguageId;

        languageValueText.text = LocalizationManager.Instance.GetText(selectedLanguageId);
    }
}