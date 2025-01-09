using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandlerScript : MonoBehaviour {
    public Color32[] _colors = new Color32[9];
    int _nextPieceColor = -1;
    int _nextHoleColor = -1;

    PieceTheme[] _allThemes;
    PieceTheme _currentTheme;

    private void Awake() {

        Color32 red = new Color32(223, 127, 152, 255); //new Color32(255, 77, 77, 255);
        Color32 orange = new Color32(237, 177, 85, 255); //new Color32(255, 153, 51, 255);
        Color32 yellow = new Color32(242, 224, 107, 255); //new Color32(255, 255, 77, 255);
        Color32 green = new Color32(190, 227, 135, 255); //new Color32(77, 255, 77, 255);
        Color32 blue = new Color32(127, 182, 236, 255); //new Color32(77, 136, 255, 255);
        Color32 lightBlue = new Color32(148, 210, 241, 255);
        Color32 skyBlue = new Color32(167, 223, 242, 255);
        Color32 purple = new Color32(155, 135, 236, 255); //new Color32(153, 51, 255, 255);
        Color32 magenta = new Color32(218, 124, 234, 255); //new Color32(234, 0, 255, 255);
        Color32 pink = new Color32(245, 186, 255, 255);

        _allThemes = new PieceTheme[] {
            new PieceTheme(red, orange, yellow), //default theme
            new PieceTheme(red, blue, yellow), //primary theme
            new PieceTheme(blue, purple, green), //secondary theme
            new PieceTheme(red, green, blue), //RGB
            new PieceTheme(red, pink, magenta), //affection
            new PieceTheme(blue, lightBlue, skyBlue), //chilly
            new PieceTheme(red,Color.white,green), //festive
            new PieceTheme(orange, Color.black, red), //spooky
            new PieceTheme(green,blue,Color.white), //nature
            new PieceTheme(blue, magenta, yellow), //cmyk
            new PieceTheme(green, magenta, blue), //neon
            new PieceTheme(new Color32((byte)Random.Range(0,255), (byte)Random.Range(0,255),255, (byte)Random.Range(0,255)), new Color32((byte)Random.Range(0,255),(byte)Random.Range(0,255),(byte)Random.Range(0,255),255), new Color32((byte)Random.Range(0,255),(byte)Random.Range(0,255),(byte)Random.Range(0,255),255))
        };

        _currentTheme = _allThemes[PlayerPrefs.GetInt("theme", 0)];

        _colors = new Color32[]{
        _currentTheme.PrimaryColor, //primary
        _currentTheme.SecondaryColor, //secondary
        _currentTheme.TertiaryColor, //tertiary
        };

        for (int i = 0; i < _colors.Length; i++) {
            if (_colors[i] == Color.white) {
                if (GlobalGameHandlerScript.Instance.BackgroundColor.SecondaryColor == Color.white) {
                    _colors[i] = new Color32(53, 53, 53, 255);
                }
            }
            else if (_colors[i] == Color.black) {
                if (GlobalGameHandlerScript.Instance.BackgroundColor.SecondaryColor.Equals(new Color32(53, 53, 53, 255))) {
                    _colors[i] = Color.white;
                }
            }
        }
    }

    //this method returns the (int) index value of the next color in line

    /*
    public int NextPieceColor() {
        _nextPieceColor++; //increment the color index
        if (_nextPieceColor > 8) _nextPieceColor = 0; //cycle back to the start after all colors have been used
        return _nextPieceColor; //return the color
    }
    public int NextHoleColor() {
        _nextHoleColor++;
        if (_nextHoleColor > 8) _nextHoleColor = 0;
        return _nextHoleColor;
    }
    */

    //this method returns the requested color from the array
    public Color32 GetColor(int color) {
        return _colors[color]; //return the requested color
    }
}
