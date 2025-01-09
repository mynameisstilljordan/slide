using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    public static AudioClip _tap, _button, _win;
    static AudioSource audioSrc;

    // Start is called before the first frame update
    void Start() {
        _button = Resources.Load<AudioClip>("button");
        _tap = Resources.Load<AudioClip>("tap");
        _win = Resources.Load<AudioClip>("win");
        audioSrc = GetComponent<AudioSource>();
    }

    public static void PlaySound(string clip) {
        if (PlayerPrefs.GetInt("sound", 1) == 1) {
            switch (clip) {
                case "button":
                    audioSrc.PlayOneShot(_button);
                    break;
                case "tap":
                    audioSrc.PlayOneShot(_tap);
                    break;
                case "win":
                    audioSrc.PlayOneShot(_win);
                    break;
            }
        }
    }
}