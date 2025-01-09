using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainCameraScript : MonoBehaviour
{
    BackgroundTheme _bg;
    Camera _cam;
    TMP_Text[] _allText;
    [SerializeField] Material mat;
    // Start is called before the first frame update
    void Start()
    {
        if (GlobalGameHandlerScript.Instance != null) {
            _cam = GetComponent<Camera>();
            _cam.backgroundColor = GlobalGameHandlerScript.Instance.BackgroundColor.PrimaryColor; //set camera background color to theme color
            _bg = GlobalGameHandlerScript.Instance.BackgroundColor;
            if (mat.color != _bg.SecondaryColor) mat.color = _bg.SecondaryColor;

            _allText = FindObjectsOfType<TMP_Text>();

            foreach (TMP_Text text in _allText) {
                text.color = _bg.SecondaryColor;
            }
        }
    }

    //this method is called when the theme is updated
    public void UpdateColors() {
        BackgroundTheme bg = GlobalGameHandlerScript.Instance.BackgroundColor;
        _cam.backgroundColor = bg.PrimaryColor;

        if (mat.color != bg.SecondaryColor) mat.color = bg.SecondaryColor;

        _allText = FindObjectsOfType<TMP_Text>();

        foreach (TMP_Text text in _allText) {
            text.color = bg.SecondaryColor;
        }
    }
}
