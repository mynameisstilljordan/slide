using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Lofelt.NiceVibrations;
//using LionStudios.Suite.Debugging;

public class MainMenuScript : MonoBehaviour {
    GlobalGameHandlerScript _gGHS;
    [SerializeField] TMP_Text _levelText, _themesText; 
    int _level;
    
    

    // Start is called before the first frame update
    void Start() {
        //LionDebugger.Hide();
        _levelText.text = "CURRENT LEVEL: " + GlobalGameHandlerScript.Instance.Level; //set the current level text
        _themesText.text = "NEXT THEME IN: " + GlobalGameHandlerScript.Instance.GetNextRequirement() + " SOLVES"; //set the themes text
        _gGHS = GameObject.FindGameObjectWithTag("globalGameHandler").GetComponent<GlobalGameHandlerScript>(); //get the reference to the gamehandler script

        GlobalGameHandlerScript.Instance.HideBanner(); //hide the banner
    }

    //this method is called when the play button is pressed
    public void OnPlayButtonPressed() {
        PlayButtonSound();
        SceneManager.LoadScene("ingame"); //load the ingame scene
    }
    public void OnThemesButtonPressed() {
        PlayButtonSound();
        SceneManager.LoadScene("themes"); //load the themes scene
    }

    public void OnSettingsButtonPressed() {
        PlayButtonSound();
        SceneManager.LoadScene("config");
    }

    void PlayButtonSound() {
        SoundManager.PlaySound("button");
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact);
    }

    public void OnLeaderBoardButtonPressed() {
        PlayButtonSound();
        //GameServices.Instance.ShowLeaderboadsUI(); //display the leaderboards ui
    }
}
