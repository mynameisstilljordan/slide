using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds;
using GoogleMobileAds.Api;
using Unity.VisualScripting;

public class Advertisements : MonoBehaviour{
    // These ad units are configured to always serve test ads.
#if UNITY_ANDROID
    private string _bannerId = "ca-app-pub-8559921247440230/7861482067";
    private string _interstitialId = "ca-app-pub-8559921247440230/3200182327";
    private string _rewardedId = "ca-app-pub-8559921247440230/5235318720";
#elif UNITY_IPHONE
    private string _bannerId = "";
    private string _interstitialId = "";
    private string _rewardedId = "";
#else
    private string _bannerId = "unused";
    private string _interstitialId = "unused";
    private string _rewardedId = "unused";
#endif

    private InterstitialAd _interstitialAd; //interstitial ad
    private RewardedAd _rewardedAd; //rewarded ad
    BannerView _bannerView; //the bannerview instance

    //this method initializes the ads to be displayed *THIS SHOULD BE CALLED ONCE AT THE START OF THE GAME
    public void InitializeAds() {
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize((InitializationStatus initStatus) => {
            // This callback is called once the MobileAds SDK is initialized.
        });
    }

    #region Banner

    //this method shows a banner ad
    public void ShowBannerAd() {
        // create an instance of a banner view first.
        if (_bannerView == null) {
            CreateBannerView();
        }

        // create our request used to load the ad.
        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        // send the request to load the ad.
        //Debug.Log("Loading banner ad.");
        _bannerView.LoadAd(adRequest);
    }

    //this method creates the banner view
    private void CreateBannerView() {
        //Debug.Log("Creating banner view");

        // If we already have a banner, destroy the old one.
        if (_bannerView != null) {
            HideBanner();
        }

        // Create a 320x50 banner at top of the screen
        _bannerView = new BannerView(_bannerId, AdSize.GetPortraitAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth), AdPosition.Bottom);
    }

    //this method destroys the pre-existing banner ad
    public void HideBanner() {
        if (_bannerView != null) {
            Debug.Log("Destroying banner ad.");
            _bannerView.Destroy();
            _bannerView = null;
        }
    }

    #endregion

    #region Interstitial

    //this method shows an interstitial ad
    public void ShowInterstitialAd() {
        if (_interstitialAd != null && _interstitialAd.CanShowAd()) {
            Debug.Log("Showing interstitial ad.");
            _interstitialAd.Show();
        }
        else {
            Debug.LogError("Interstitial ad is not ready yet.");
        }
    }

    private void LoadInterstitialAd() {
        // Clean up the old ad before loading a new one.
        if (_interstitialAd != null) {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        Debug.Log("Loading the interstitial ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        // send the request to load the ad.
        InterstitialAd.Load(_interstitialId, adRequest, (InterstitialAd ad, LoadAdError error) => {
            // if error is not null, the load request failed.
            if (error != null || ad == null) {
                Debug.LogError("interstitial ad failed to load an ad " + "with error : " + error);
                return;
            }

            Debug.Log("Interstitial ad loaded with response : " + ad.GetResponseInfo());

            _interstitialAd = ad;
        });
    }

    //listener for interstitial ads
    private void RegisterReloadHandler(InterstitialAd ad) {
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () => {
            Debug.Log("Interstitial Ad full screen content closed.");

            // Reload the ad so that we can show another as soon as possible.
            LoadInterstitialAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) => {
            Debug.LogError("Interstitial ad failed to open full screen content " + "with error : " + error);

            // Reload the ad so that we can show another as soon as possible.
            LoadInterstitialAd();
        };
    }

    #endregion

    #region Rewarded

    //this method shows a rewarded ad
    public void ShowRewardedAd() {
        const string rewardMsg = "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";

        if (_rewardedAd != null && _rewardedAd.CanShowAd()) {
            _rewardedAd.Show((Reward reward) => {
                //this method is called after the ad is finished
                GameManager.Instance.GiveUserReward();
            });
        }
    }

    private void LoadRewardedAd() {
        // Clean up the old ad before loading a new one.
        if (_rewardedAd != null) {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        Debug.Log("Loading the rewarded ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        // send the request to load the ad.
        RewardedAd.Load(_rewardedId, adRequest, (RewardedAd ad, LoadAdError error) => {
            // if error is not null, the load request failed.
            if (error != null || ad == null) {
                Debug.LogError("Rewarded ad failed to load an ad " + "with error : " + error);
                return;
            }

            Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());

            _rewardedAd = ad;
        });
    }


    //rewarded ad listener
    private void RegisterReloadHandler(RewardedAd ad) {
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () => {
            Debug.Log("Rewarded Ad full screen content closed.");

            // Reload the ad so that we can show another as soon as possible.
            LoadRewardedAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) => {
            Debug.LogError("Rewarded ad failed to open full screen content " + "with error : " + error);

            // Reload the ad so that we can show another as soon as possible.
            LoadRewardedAd();
        };
    }

    #endregion
}
