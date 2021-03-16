using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using ExitGames.Client.Photon.Chat;
using UnityEngine.SceneManagement;
using PlayFab.ClientModels;
using PlayFab;
using System.Collections.Generic;
#if UNITY_ANDROID || UNITY_IOS
using UnityEngine.Advertisements;
#endif
using AssemblyCSharp;

using GoogleMobileAds;
using GoogleMobileAds.Api;

public class InitMenuScript : MonoBehaviour
{
    public static InitMenuScript instance;
    public GameObject playerName;
    public GameObject videoRewardText;
    public GameObject playerAvatar;
    public GameObject fbFriendsMenu;
    public GameObject matchPlayer;
    public GameObject backButtonMatchPlayers;
    public GameObject MatchPlayersCanvas;
    public GameObject menuCanvas;
    public GameObject tablesCanvas;
    public GameObject gameTitle;
    public GameObject changeDialog;
    public GameObject inputNewName;
    public GameObject tooShortText;
    public GameObject coinsText;
    public GameObject coinsTextShop;
    public GameObject coinsTab;
    public GameObject redeemValidation;
    public Text redeemValidationText;
    public GameObject dialog;
    // Use this for initialization
    public GameObject fbLogin;
    public GameObject logOut;
    public GameObject offlinePlay;
    public GameObject tutorialStart;
    public GameObject playStart;
    public GameObject message_text;
    private bool loginOn = false;
    private bool friendOn = false;
    private bool switchOn = false;
    private bool offlineOn = false;

    private InterstitialAd interstitial;
    private RewardBasedVideoAd rewardBasedVideo;

    private void Awake()
    {
        instance = this;
    }
    string appId;
    void Start()
    {


        GameManager.Instance.dialog = dialog;
        videoRewardText.GetComponent<Text>().text = "+" + StaticStrings.rewardForVideoAd;
        GameManager.Instance.tablesCanvas = tablesCanvas;
        GameManager.Instance.facebookFriendsMenu = fbFriendsMenu.GetComponent<FacebookFriendsMenu>(); ;
        GameManager.Instance.matchPlayerObject = matchPlayer;
        GameManager.Instance.backButtonMatchPlayers = backButtonMatchPlayers;
        playerName.GetComponent<Text>().text = GameManager.Instance.nameMy;
        GameManager.Instance.MatchPlayersCanvas = MatchPlayersCanvas;

        if (GameManager.Instance.avatarMy != null)
            playerAvatar.GetComponent<Image>().sprite = GameManager.Instance.avatarMy;


        GameManager.Instance.coinsTextMenu = coinsText;
        GameManager.Instance.coinsTextShop = coinsTextShop;
        GameManager.Instance.playfabManager.updateCoinsTextMenu();
        GameManager.Instance.playfabManager.updateCoinsTextShop();
        GameManager.Instance.initMenuScript = this;

        if (StaticStrings.hideCoinsTabInShop)
        {
            coinsTab.SetActive(false);
        }


    }
    private void OnEnable()
    {
#if UNITY_WEBGL
        coinsTab.SetActive(false);
#endif
        //coinsText.GetComponent<Text>().text = GameManager.Instance.coinsCount + "";
#if UNITY_ANDROID
        appId = "ca-app-pub-6815533330672884~7331924526";
        //string appId = "pub-6815533330672884";
#elif UNITY_IPHONE
        string appId = "ca-app-pub-6815533330672884~7331924526";
#else
        string appId = "Free Cash Pool";
#endif
        MobileAds.Initialize(appId);

        this.rewardBasedVideo = RewardBasedVideoAd.Instance;
        // Called when an ad request has successfully loaded.
        rewardBasedVideo.OnAdLoaded += HandleRewardBasedVideoLoaded;
        // Called when an ad request failed to load.
        rewardBasedVideo.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
        // Called when an ad is shown.
        rewardBasedVideo.OnAdOpening += HandleRewardBasedVideoOpened;
        // Called when the ad starts to play.
        rewardBasedVideo.OnAdStarted += HandleRewardBasedVideoStarted;
        // Called when the user should be rewarded for watching a video.
        rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
        // Called when the ad is closed.
        rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;
        // Called when the ad click caused the user to leave the application.
        rewardBasedVideo.OnAdLeavingApplication += HandleRewardBasedVideoLeftApplication;

        //  HandleShowResult();
        //var options = new ShowOptions { resultCallback = HandleShowResult };

        this.RequestRewardBasedVideo();
    }

    private void OnDisable()
    {
        rewardBasedVideo.OnAdLoaded -= HandleRewardBasedVideoLoaded;
        // Called when an ad request failed to load.
        rewardBasedVideo.OnAdFailedToLoad -= HandleRewardBasedVideoFailedToLoad;
        // Called when an ad is shown.
        rewardBasedVideo.OnAdOpening -= HandleRewardBasedVideoOpened;
        // Called when the ad starts to play.
        rewardBasedVideo.OnAdStarted -= HandleRewardBasedVideoStarted;
        // Called when the user should be rewarded for watching a video.
        rewardBasedVideo.OnAdRewarded -= HandleRewardBasedVideoRewarded;
        // Called when the ad is closed.
        rewardBasedVideo.OnAdClosed -= HandleRewardBasedVideoClosed;
        // Called when the ad click caused the user to leave the application.
        rewardBasedVideo.OnAdLeavingApplication -= HandleRewardBasedVideoLeftApplication;
    }
    // Update is called once per frame
    void Update()
    {
        if (loginOn == false)
            fbLogin.transform.localScale = new Vector3(1, 1, 1);
        if (friendOn == false)
            fbFriendsMenu.transform.localScale = new Vector3(1, 1, 1);
        if (switchOn == false)
            logOut.transform.localScale = new Vector3(1, 1, 1);
        if (offlineOn == false)
            offlinePlay.transform.localScale = new Vector3(1, 1, 1);
    }

    //public void showAdStore() {  adm
    //    if (StaticStrings.showAdOnStoreScene)
    //        GameManager.Instance.adsScript.ShowAd();
    //}

    public void backToMenuFromTableSelect()
    {
        tablesCanvas.SetActive(false);
        menuCanvas.SetActive(true);
        gameTitle.SetActive(true);
    }

    public void showSelectTableScene(bool challengeFriend)
    {
        if (!challengeFriend)
            GameManager.Instance.inviteFriendActivated = false;
        //if (StaticStrings.showAdOnSelectTableScene)
        // GameManager.Instance.adsScript.ShowAd(); adm
        menuCanvas.SetActive(false);
        tablesCanvas.SetActive(true);
        gameTitle.SetActive(false);
    }

    public void LoginPointEnter()
    {

        fbLogin.transform.localScale = fbLogin.transform.localScale * 1.1f;
        loginOn = true;
    }
    public void LoginPointExit()
    {
        loginOn = false;
        fbLogin.transform.localScale = fbLogin.transform.localScale;

    }
    public void LoginPointUp(bool challengeFriend)
    {
        fbLogin.transform.localScale = fbLogin.transform.localScale;
        loginOn = false;
        if (!challengeFriend)
            GameManager.Instance.inviteFriendActivated = false;
        //if (StaticStrings.showAdOnSelectTableScene)
        // GameManager.Instance.adsScript.ShowAd(); adm
        menuCanvas.SetActive(false);
        tablesCanvas.SetActive(true);
        gameTitle.SetActive(false);

    }
    public void LoginPointDown()
    {
        loginOn = true;
        fbLogin.transform.localScale = fbLogin.transform.localScale * 1.1f;
    }

    public void playOffline()
    {
        GameManager.Instance.tableNumber = 0;
        GameManager.Instance.offlineMode = true;
        GameManager.Instance.roomOwner = true;
        SceneManager.LoadScene("GameScene");
    }
    
    public void offlinePointEnter()
    {

        offlinePlay.transform.localScale = offlinePlay.transform.localScale * 1.1f;
        offlineOn = true;
    }
    public void offlinePointExit()
    {
        offlineOn = false;
        offlinePlay.transform.localScale = offlinePlay.transform.localScale;

    }
    public void offlinePointUp()
    {
        offlinePlay.transform.localScale = offlinePlay.transform.localScale;
        offlineOn = false;
        GameManager.Instance.tableNumber = 0;
        GameManager.Instance.offlineMode = true;
        GameManager.Instance.roomOwner = true;
        SceneManager.LoadScene("GameScene");

    }
    public void offlinePointDown()
    {
        offlineOn = true;
        offlinePlay.transform.localScale = offlinePlay.transform.localScale * 1.1f;
    }

    public void switchUser()
    {
        GameManager.Instance.playfabManager.destroy();
        GameManager.Instance.facebookManager.destroy();
        GameManager.Instance.connectionLost.destroy();
        //  GameManager.Instance.adsScript.destroy(); adm
        GameManager.Instance.avatarMy = null;
        PhotonNetwork.Disconnect();

        PlayerPrefs.DeleteAll();
        GameManager.Instance.resetAllData();
        GameManager.Instance.coinsCount = 0;
        SceneManager.LoadScene("LoginSplash");
    }

    public void switchPointEnter()
    {

        logOut.transform.localScale = logOut.transform.localScale * 1.1f;
        switchOn = true;
    }
    public void switchPointExit()
    {
        switchOn = false;
        logOut.transform.localScale = logOut.transform.localScale;

    }
    public void switchPointUp()
    {
        logOut.transform.localScale = logOut.transform.localScale;
        switchOn = false;
        //GameManager.Instance.playfabManager.destroy();
        //GameManager.Instance.facebookManager.destroy();
        //GameManager.Instance.connectionLost.destroy();
        ////  GameManager.Instance.adsScript.destroy(); adm
        GameManager.Instance.avatarMy = null;
        PhotonNetwork.Disconnect();

        PlayerPrefs.DeleteAll();
        GameManager.Instance.resetAllData();
        GameManager.Instance.coinsCount = 0;
        SceneManager.LoadScene("LoginSplash");

    }
    public void switchPointDown()
    {
        switchOn = true;
        logOut.transform.localScale = logOut.transform.localScale * 1.1f;
    }

    public void showChangeDialog()
    {
        changeDialog.SetActive(true);
    }

    public void changeUserName()
    {
        Debug.Log("Change Nickname");

        string newName = inputNewName.GetComponent<Text>().text;
        if (newName.Equals(StaticStrings.addCoinsHackString))
        {
            GameManager.Instance.playfabManager.addCoinsRequest(1000000);
            changeDialog.SetActive(false);
        }
        else
        {
            if (newName.Length > 0)
            {
                UpdateUserTitleDisplayNameRequest displayNameRequest = new UpdateUserTitleDisplayNameRequest()
                {
                    //DisplayName = newName
                    DisplayName = GameManager.Instance.playfabManager.PlayFabId
                };

                PlayFabClientAPI.UpdateUserTitleDisplayName(displayNameRequest, (response) => {
                    Dictionary<string, string> data = new Dictionary<string, string>();
                    data.Add("PlayerName", newName);
                    UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
                    {
                        Data = data,
                        Permission = UserDataPermission.Public
                    };

                    PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) => {
                        Debug.Log("Data updated successfull ");
                        Debug.Log("Title Display name updated successfully");
                        PlayerPrefs.SetString("GuestPlayerName", newName);
                        PlayerPrefs.Save();
                        GameManager.Instance.nameMy = newName;
                        playerName.GetComponent<Text>().text = newName;
                    }, (error1) => {
                        Debug.Log("Data updated error " + error1.ErrorMessage);
                    }, null);

                }, (error) => {
                    Debug.Log("Title Display name updated error: " + error.Error);

                }, null);

                changeDialog.SetActive(false);
            }
            else
            {
                tooShortText.SetActive(true);
            }
        }



    }

    public void startQuickGame()
    {
        GameManager.Instance.facebookManager.startRandomGame();
    }

    public void startQuickGameTableNumer(int tableNumer, int fee)
    {
        GameManager.Instance.payoutCoins = fee;
        GameManager.Instance.payoutCoinsWithoutComission = fee;
        GameManager.Instance.tableNumber = tableNumer;
        GameManager.Instance.facebookManager.startRandomGame();
    }

    public void showFacebookFriends()
    {
        //  if (StaticStrings.showAdOnFriendsScene)
        //  GameManager.Instance.adsScript.ShowAd(); adm
        GameManager.Instance.playfabManager.GetPlayfabFriends();
    }

    public void friendsPointUp()
    {
        fbFriendsMenu.transform.localScale = fbFriendsMenu.transform.localScale;
        showFacebookFriends();
        friendOn = false;

    }
    public void frinedsPointDown()
    {
        friendOn = true;
        fbFriendsMenu.transform.localScale = fbFriendsMenu.transform.localScale * 1.1f;
    }
    public void friendsPointEnter()
    {
        friendOn = true;
        fbFriendsMenu.transform.localScale = fbFriendsMenu.transform.localScale * 1.1f;
    }
    public void friendsPointExit()
    {
        fbFriendsMenu.transform.localScale = fbFriendsMenu.transform.localScale;
        friendOn = false;
    }

    public void setTableNumber()
    {
        GameManager.Instance.tableNumber = Int32.Parse(GameObject.Find("TextTableNumber").GetComponent<Text>().text);
    }


    public void ShowRewardedAd()
    {
        adValidation_ = false;
#if UNITY_ANDROID || UNITY_IOS
        //if (Advertisement.IsReady("rewardedVideo"))
        //{
        //    var options = new ShowOptions { resultCallback = HandleShowResult };
        //    Advertisement.Show("rewardedVideo", options);
        //}
#endif
        Debug.Log("sssssssss");
        if (this.rewardBasedVideo.IsLoaded())
        {
            Debug.Log("HHHHH");
             HandleShowResult();
           // var options = new ShowOptions { resultCallback = HandleShowResult };
            this.rewardBasedVideo.Show();
        }
        else
        {
          //  MobileAds.Initialize(appId);
            MonoBehaviour.print("Rewarded ad is not ready yet");
            StartCoroutine("AdisNotReadyMssgShow");
        }
    }

    public IEnumerator AdisNotReadyMssgShow()
    {
        message_text.transform.GetChild(0).GetComponent<Text>().text = "Rewarded ad is not ready yet";
        message_text.gameObject.SetActive(true);
        yield return new WaitForSeconds(3.5f);
        message_text.transform.GetChild(0).GetComponent<Text>().text = string.Empty;
        message_text.gameObject.SetActive(false);

    }

    public void RequestRewardBasedVideo()
    {
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-6815533330672884/7861820838";
#elif UNITY_IPHONE
            string adUnitId = "ca-app-pub-6815533330672884/7861820838";
#else
            string adUnitId = "Free Cash Pool";
#endif     

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded video ad with the request.
        this.rewardBasedVideo.LoadAd(request, adUnitId);

    }
    public void HandleRewardBasedVideoLoaded(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoLoaded event received");
    }

    public void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        MonoBehaviour.print(
            "HandleRewardBasedVideoFailedToLoad event received with message: "
                             + args.Message);
    }

    public void HandleRewardBasedVideoOpened(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoOpened event received");
    }

    public void HandleRewardBasedVideoStarted(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoStarted event received");
    }

    public void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoClosed event received");
        this.RequestRewardBasedVideo();
    }
    bool adValidation_=false;
    public void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        if (adValidation_)
        {
            return;
        }
        adValidation_ = true;
        Debug.Log("HandleRewardBasedVideoRewarded    :::  " + StaticStrings.rewardForVideoAd);
        GameManager.Instance.playfabManager.addCoinsRequestWatchVideo(StaticStrings.rewardForVideoAd);

        string type = args.Type;
        double amount = args.Amount;

        MonoBehaviour.print(
            "HandleRewardBasedVideoRewarded event received for "
                        + amount.ToString() + " " + type);
    }

    public void HandleRewardBasedVideoLeftApplication(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoLeftApplication event received");
    }

    public static string ad_ = string.Empty;
    //#if UNITY_ANDROID || UNITY_IOS
    //    private void HandleShowResult()
    //    {
    //        Debug.Log("===============result=============" );
    //        //switch (result)
    //      //  {
    //        //  case ShowResult.Finished:
    //             //  {
    //                    Debug.Log("The ad was successfully shown.");
    //                    //        Debug.Log(StaticStrings.rewardForVideoAd);

    //                    //        ad_ = "AD";
    //                    //        GameManager.Instance.playfabManager.addCoinsRequest(StaticStrings.rewardForVideoAd);
    //                    //        StartCoroutine(Wait());
    //                    //        //
    //                    //        // YOUR CODE TO REWARD THE GAMER
    //                    //        // Give coins etc.
    //                    GameManager.Instance.playfabManager.addCoinsRequestWatchVideo(StaticStrings.rewardForVideoAd);
    //                 //   break;
    //              // }
    //        //    case ShowResult.Skipped:
    //        //        Debug.Log("The ad was skipped before reaching the end.");
    //        //        break;
    //        //    case ShowResult.Failed:.

    //        //        Debug.LogError("The ad failed to be shown.");
    //        //        break;

    //                ad_ = "AD";
    //             //   StartCoroutine(Wait());
    //    }

#if UNITY_ANDROID || UNITY_IOS
    private void HandleShowResult()
    {
        Debug.Log("The ad was successfully shown.=======");



    }
#endif

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(3.5f);
        GameManager.Instance.playfabManager.UpdatedFreeCreditAd(GameManager.playerTotalUpdatedAmt);

    }
    public void TutorialStart()
    {
        playStart.SetActive(true);
        tutorialStart.SetActive(true);
    }
    
}
