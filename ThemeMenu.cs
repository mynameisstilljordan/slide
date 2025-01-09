using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
//using LionStudios.Suite.Debugging;

public class ThemeMenu : MonoBehaviour
{
    internal class Theme {
        public int Requirement { get; set; }

        public Button Button { get; set; }

        public Theme(int requirement, Button button) {
            this.Requirement = requirement;
            this.Button = button;
        }
    }

    [SerializeField] Button _toggleButton;
    [SerializeField] Button _useButton; 
    [SerializeField] Button[] _themeButtons;
    [SerializeField] Sprite[] _toggleSprites;
    Theme[] _themes = new Theme[12];
    private Color32[] _themeColors;

    // Start is called before the first frame update
    void Start()
    {
        //LionDebugger.Hide();
        int theme = PlayerPrefs.GetInt("backgroundTheme", 1);

        if (theme == 0) _toggleButton.image.sprite = _toggleSprites[0];
        else if (theme == 1) _toggleButton.image.sprite = _toggleSprites[1];
        else _toggleButton.image.sprite = _toggleSprites[2];
 
        _useButton.onClick.AddListener(UseButtonPressed); //add the listener to the use button
        _toggleButton.onClick.AddListener(OnToggleButtonPressed); //add listener to the toggle button

        int[] _requirements = GlobalGameHandlerScript.Instance.AllRequirements();

        //load the solves
        var solves = PlayerPrefs.GetInt("solves",0);

        //for all the theme buttons
        for (int i = 0; i < _themes.Length; i++) {
            _themes[i] = new Theme(_requirements[i], _themeButtons[i]); //instantiate theme with according values

            if (_themes[i].Requirement > solves) _themes[i].Button.interactable = false; //if the theme doesn't meet the requirement, lock the button

            //otherwise, add a listener to the button to update the theme according to the theme object's color
            else {
                var color = _themes[i].Button.transform.name;
                _themes[i].Button.onClick.AddListener(delegate { GlobalGameHandlerScript.Instance.SetTheme(color); PlayButtonSound(); ReturnToMenu(); });
            }
        }
    }

    void UseButtonPressed() {
        PlayButtonSound();
        ReturnToMenu();
    }

    void PlayButtonSound() {
        SoundManager.PlaySound("button");
    }

    void ReturnToMenu() {
        SceneManager.LoadScene(sceneName: "mainMenu");
    }

    //when the toggle button is pressed
    void OnToggleButtonPressed() {
        GlobalGameHandlerScript.Instance.ToggleGlobalTheme();

        int theme = PlayerPrefs.GetInt("backgroundTheme", 1);

        if (theme == 0) _toggleButton.image.sprite = _toggleSprites[0];
        else if (theme == 1) _toggleButton.image.sprite = _toggleSprites[1];
        else _toggleButton.image.sprite = _toggleSprites[2];

        Camera.main.gameObject.GetComponent<MainCameraScript>().UpdateColors(); //update the colors

        PlayButtonSound();
    }
}
