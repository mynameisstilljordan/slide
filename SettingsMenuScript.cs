using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lofelt.NiceVibrations;
using UnityEngine.SceneManagement;
//using LionStudios.Suite.Debugging;

public class SettingsMenuScript : MonoBehaviour
{
    [SerializeField] Button _soundOnButton, _soundOffButton, _vibrationOnButton, _vibrationOffButton;
    // Start is called before the first frame update
    void Start()
    {
        //LionDebugger.Hide();
        LoadButtonStates();
        _soundOnButton.onClick.AddListener(SoundButtonOn);
        _soundOffButton.onClick.AddListener(SoundButtonOff);
        _vibrationOnButton.onClick.AddListener(VibrationButtonOn);
        _vibrationOffButton.onClick.AddListener(VibrationButtonOff);
    }

    private void LoadButtonStates() {
        if (PlayerPrefs.GetInt("sound", 1) == 1) {
            _soundOnButton.interactable = false;
        }
        else _soundOffButton.interactable = false;

        if (PlayerPrefs.GetInt("vibration", 1) == 1) {
            _vibrationOnButton.interactable = false;
        }
        else _vibrationOffButton.interactable = false;
    }

    void SoundButtonOn() {
        _soundOnButton.interactable = false;
        _soundOffButton.interactable = true;
        PlayerPrefs.SetInt("sound", 1);
        PlayButtonSound();
    }

    void SoundButtonOff() {
        _soundOffButton.interactable = false;
        _soundOnButton.interactable = true;
        PlayerPrefs.SetInt("sound", 0);
    }

    void VibrationButtonOn() {
        _vibrationOnButton.interactable = false;
        _vibrationOffButton.interactable = true;
        PlayerPrefs.SetInt("vibration", 1);
        HapticController.hapticsEnabled = true;
        PlayButtonSound();
    }

    void VibrationButtonOff() {
        _vibrationOffButton.interactable = false;
        _vibrationOnButton.interactable = true;
        PlayerPrefs.SetInt("vibration", 0);
        HapticController.hapticsEnabled = false;
        PlayButtonSound();
    }

    public void OnUseButtonPressed() {
        PlayButtonSound();
        SceneManager.LoadScene("mainMenu");
    }

    void PlayButtonSound() {
        SoundManager.PlaySound("button");
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact);
    }
}
