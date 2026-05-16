using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;

    [Header("Add All CSV Files Here")]
    public TextAsset[] csvFiles;

    [Header("Language")]
    public bool isEnglish = true;

    private Dictionary<string, string> localizedText = new Dictionary<string, string>();

    private const string LanguagePrefKey = "SelectedLanguage";

    private void Awake()
    {
        Instance = this;

        LoadSavedLanguage();
        LoadLanguageData();
    }

    private void Start()
    {
        RefreshAllTexts();
    }

    private void LoadSavedLanguage()
    {
        if (PlayerPrefs.HasKey(LanguagePrefKey))
        {
            isEnglish = PlayerPrefs.GetInt(LanguagePrefKey) == 1;
        }
        else
        {
            isEnglish = true;
            PlayerPrefs.SetInt(LanguagePrefKey, 1);
            PlayerPrefs.Save();
        }
    }

    public void LoadLanguageData()
    {
        localizedText.Clear();

        foreach (TextAsset csv in csvFiles)
        {
            if (csv == null)
                continue;

            string[] rows = csv.text.Split(
                new string[] { "\r\n", "\n", "\r" },
                StringSplitOptions.RemoveEmptyEntries
            );

            foreach (string row in rows)
            {
                string pattern = row.Contains(";")
                    ? @";(?=(?:[^""]*""[^""]*"")*[^""]*$)"
                    : @",(?=(?:[^""]*""[^""]*"")*[^""]*$)";

                string[] columns = Regex.Split(row, pattern);

                if (columns.Length >= 5)
                {
                    string id = columns[0].Trim().Replace("\"", "");

                    if (string.IsNullOrEmpty(id) || id.ToLower().Contains("id_key"))
                        continue;

                    string text = isEnglish ? columns[4] : columns[3];
                    text = text.Trim().Trim('"').Replace("\"\"", "\"");

                    if (!localizedText.ContainsKey(id))
                    {
                        localizedText.Add(id, text);
                    }
                    else
                    {
                        localizedText[id] = text;
                    }
                }
            }
        }

        Debug.Log("Localization system loaded. Total words: " + localizedText.Count);
    }

    public string GetText(string id)
    {
        if (string.IsNullOrEmpty(id))
            return "";

        string cleanId = id.Trim().Replace("\"", "");

        if (localizedText != null && localizedText.ContainsKey(cleanId))
        {
            return localizedText[cleanId];
        }

        return cleanId;
    }

    public void SetEnglish()
    {
        SetLanguage(true);
    }

    public void SetTurkish()
    {
        SetLanguage(false);
    }

    public void ToggleLanguage()
    {
        SetLanguage(!isEnglish);
    }

    public void SetLanguage(bool english)
    {
        isEnglish = english;

        PlayerPrefs.SetInt(LanguagePrefKey, isEnglish ? 1 : 0);
        PlayerPrefs.Save();

        LoadLanguageData();
        RefreshAllTexts();
    }

    public void RefreshAllTexts()
    {
        UITextLocalizer[] localizers = FindObjectsByType<UITextLocalizer>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (UITextLocalizer localizer in localizers)
        {
            localizer.UpdateText();
        }

        SettingsLanguageSelector[] selectors = FindObjectsByType<SettingsLanguageSelector>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (SettingsLanguageSelector selector in selectors)
        {
            selector.UpdateLanguageView();
        }
    }
}