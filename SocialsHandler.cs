using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lofelt.NiceVibrations;

//place this on the social-holder gameobject
public class SocialsHandler : MonoBehaviour
{
    Button _websiteButton, _discordButton, _instagramButton, _tiktokButton;

    // Start is called before the first frame update
    void Start()
    {
        _websiteButton = transform.GetChild(0).GetComponent<Button>();
        _discordButton = transform.GetChild(1).GetComponent<Button>();
        _instagramButton = transform.GetChild(2).GetComponent<Button>();
        _tiktokButton = transform.GetChild(3).GetComponent<Button>();

        _websiteButton.onClick.AddListener(OnWebsiteButtonPressed);
        _discordButton.onClick.AddListener(OnDiscordButtonPressed);
        _instagramButton.onClick.AddListener(OnInstagramButtonPressed);
        _tiktokButton.onClick.AddListener(OnTikTokButtonPressed);
    }

    void OnWebsiteButtonPressed() {
        PlayButtonSound();
        Application.OpenURL("https://totem-io.com");
    }

    //when the discord button is pressed
    void OnDiscordButtonPressed() {
        PlayButtonSound();
        Application.OpenURL("https://discord.gg/ajvAVM8dS7");
    }

    void OnInstagramButtonPressed() {
        PlayButtonSound();
        Application.OpenURL("https://instagram.com/totem.io");
    }

    void OnTikTokButtonPressed() {
        PlayButtonSound();
        Application.OpenURL("https://tiktok.com/@totem.io");
    }

    void PlayButtonSound() {
        SoundManager.PlaySound("button");
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact);
    }
}
