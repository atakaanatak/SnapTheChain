using TMPro;
using UnityEngine;

public class SoundSettingsController : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text soundValueText;

    [Header("Sound Settings")]
    [Range(0, 100)]
    public int soundValue = 100;

    public int stepAmount = 10;

    private const string SoundPrefKey = "MasterSoundValue";

    private void Start()
    {
        LoadSoundValue();
        ApplySoundValue();
        UpdateSoundText();
    }

    public void IncreaseSound()
    {
        soundValue += stepAmount;

        if (soundValue > 100)
        {
            soundValue = 100;
        }

        SaveAndApply();
    }

    public void DecreaseSound()
    {
        soundValue -= stepAmount;

        if (soundValue < 0)
        {
            soundValue = 0;
        }

        SaveAndApply();
    }

    private void SaveAndApply()
    {
        PlayerPrefs.SetInt(SoundPrefKey, soundValue);
        PlayerPrefs.Save();

        ApplySoundValue();
        UpdateSoundText();
    }

    private void LoadSoundValue()
    {
        if (PlayerPrefs.HasKey(SoundPrefKey))
        {
            soundValue = PlayerPrefs.GetInt(SoundPrefKey);
        }
        else
        {
            soundValue = 100;
            PlayerPrefs.SetInt(SoundPrefKey, soundValue);
            PlayerPrefs.Save();
        }
    }

    private void ApplySoundValue()
    {
        AudioListener.volume = soundValue / 100f;
    }

    private void UpdateSoundText()
    {
        if (soundValueText != null)
        {
            soundValueText.text = soundValue.ToString();
        }
    }
}