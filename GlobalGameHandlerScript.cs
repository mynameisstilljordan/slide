using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lofelt.NiceVibrations;
using com.adjust.sdk;
//using LionStudios.Suite.Debugging;
//using LionStudios.Suite.Analytics;
public class BackgroundTheme{

    public Color32 PrimaryColor { get; set; }
    public Color32 SecondaryColor { get; set; }

    public BackgroundTheme(Color32 primary, Color32 secondary) {
        this.PrimaryColor = primary;
        this.SecondaryColor = secondary;
    }
};

public class PieceTheme {
    public Color32 PrimaryColor { get; set; }
    public Color32 SecondaryColor { get; set; }
    public Color32 TertiaryColor { get; set; }
    public PieceTheme(Color32 primary, Color32 secondary, Color32 tertiary) {
        PrimaryColor = primary;
        SecondaryColor = secondary;
        TertiaryColor = tertiary;
    }
}

public class GlobalGameHandlerScript : MonoBehaviour {
    public static GlobalGameHandlerScript Instance; //the instance var
    int[] _requirements = new int[] {
            0,50,150,300,500,750,1050,1400,1800,2250,2750,5000
        };
    string _npaValue;

    public int Level { get; set; }
    public Color32 Theme { get; set; }

    public BackgroundTheme BackgroundColor { get; set; }

    BackgroundTheme[] _backgroundColors = new BackgroundTheme[] {
        new BackgroundTheme(new Color32(10,10,10,255),new Color32(53,53,53,255)), //dark
        new BackgroundTheme(new Color32(191, 177, 165, 255),new Color32(211, 204, 198, 255)), //standard
        new BackgroundTheme(new Color32(202,202,202,255),Color.white), //light
    };

    private void Awake() {
        if (Instance == null) Instance = this; //if instance is null, make this the instance
        else Destroy(gameObject); //otherwise, destroy this to avoid duplicates
        DontDestroyOnLoad(this); //dont destroy this on load
    }

    public int LevelAttempt { get; set; }

    private void Start() {
        //LionAnalytics.SetWhitelistPriorityLevel(EventPriorityLevel.P3);

        LevelAttempt = PlayerPrefs.GetInt("attempt", 0);

        if (PlayerPrefs.GetInt("vibration", 1) == 1) HapticController.hapticsEnabled = true;
        else HapticController.hapticsEnabled = false;

        BackgroundColor = _backgroundColors[PlayerPrefs.GetInt("backgroundTheme", 1)];
        Level = PlayerPrefs.GetInt("level", 1);

        Camera.main.backgroundColor = BackgroundColor.PrimaryColor; //this is called at the beginning of the game because the first main camera initializes before this gameobject

        //Advertisements.Instance.Initialize(); //initialize ads

        //GameServices.Instance.LogIn(); //google play game services sign in

        //MaxSdk.SetSdkKey("IKie4ofsUHPvLH0UNLqoUSsLrMWDcKTgMLdzBnTuQsmUKkEt8btMX2FKx0z2HeTNvs1z6B1KME0w5W4w0GRchz");
        //MaxSdk.SetUserId(SystemInfo.deviceUniqueIdentifier);
        //MaxSdk.SetVerboseLogging(true);
        //MaxSdk.InitializeSdk();

#if UNITY_IOS
        /* Mandatory - set your iOS app token here */
        InitAdjust("YOUR_IOS_APP_TOKEN_HERE");
#elif UNITY_ANDROID
        /* Mandatory - set your Android app token here */
        InitAdjust("oa1artyc01kw");
#endif
    }

    public void ToggleGlobalTheme() {
        if (BackgroundColor == _backgroundColors[0]) {
            BackgroundColor = _backgroundColors[1];
            PlayerPrefs.SetInt("backgroundTheme", 1);
        }
        else if (BackgroundColor == _backgroundColors[1]) {
            BackgroundColor = _backgroundColors[2];
            PlayerPrefs.SetInt("backgroundTheme", 2);
        }
        else {
            BackgroundColor = _backgroundColors[0];
            PlayerPrefs.SetInt("backgroundTheme", 0);
        }
    }

    public void IncrementLevelAttempt() {
        PlayerPrefs.SetInt("attempt", LevelAttempt + 1);
        LevelAttempt = PlayerPrefs.GetInt("attempt");
    }

    public void ResetLevelAttempt() {
        PlayerPrefs.SetInt("attempt",0);
        LevelAttempt = PlayerPrefs.GetInt("attempt");
    }

    public void SetTheme(string color) {
        PlayerPrefs.SetInt("theme", int.Parse(color));
    }

    public void IncrementLevel() {
        Level++; PlayerPrefs.SetInt("level", Level);
    }

    //this method shows the banner
    public void ShowBanner() {
        //if (!Advertisements.Instance.IsBannerOnScreen()) //if there is no banner on screen
        Advertisements.Instance.ShowBanner(BannerPosition.BOTTOM); //show a banner on the bottom of the screen
    }

    //this method hides the banner
    public void HideBanner() {
        if (Advertisements.Instance.IsBannerOnScreen()) Advertisements.Instance.HideBanner();
    }

    //this method returns all the requirements
    public int[] AllRequirements() {
        return _requirements;
    }

    //this method returns the next requirement
    public int GetNextRequirement() {
        //solves temp variable
        var solves = PlayerPrefs.GetInt("level", 1) - 1;

        //for all requirements
        for (int i = 0; i < _requirements.Length; i++) {
            if (solves < _requirements[i]) return _requirements[i] - solves; //if solves is less than next requirement, return the difference of the two
        }
        return 0; //if all checks passed, return 0
    }

    private void InitAdjust(string adjustAppToken) {
        var adjustConfig = new AdjustConfig(
            adjustAppToken,
            AdjustEnvironment.Production, // AdjustEnvironment.Sandbox to test in dashboard
            true
        );
        adjustConfig.setLogLevel(AdjustLogLevel.Info); // AdjustLogLevel.Suppress to disable logs
        adjustConfig.setSendInBackground(true);
        new GameObject("Adjust").AddComponent<Adjust>(); // do not remove or rename
        // Adjust.addSessionCallbackParameter("foo", "bar"); // if requested to set session-level parameters
        //adjustConfig.setAttributionChangedDelegate((adjustAttribution) => {
        //  Debug.LogFormat("Adjust Attribution Callback: ", adjustAttribution.trackerName);
        //});
        Adjust.start(adjustConfig);
    }
}
