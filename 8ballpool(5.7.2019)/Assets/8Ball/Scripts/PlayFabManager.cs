using UnityEngine;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine.SceneManagement;
//using Facebook.Unity;
using System.Collections.Generic;
using ExitGames.Client.Photon.Chat;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using AssemblyCSharp;
using System.Globalization;
using GooglePlayGames;
using UnityEngine.Networking;

public class PlayFabManager : Photon.PunBehaviour, IChatClientListener
{

    public string PlayFabId;
    public string PlayFabTitleID;
    public string PhotonAppID;
    public string PhotonChatID;
    public bool multiGame = true;
    public bool roomOwner = false;
    private FacebookManager fbManager;
    public GameObject fbButton;
    private FacebookFriendsMenu facebookFriendsMenu;
    public ChatClient chatClient;
    private bool alreadyGotFriends = false;
    public GameObject menuCanvas;
    public GameObject MatchPlayersCanvas;
    public GameObject splashCanvas;
    public bool opponentReady = false;
    public bool imReady = false;
    public GameObject playerAvatar;
    public GameObject playerName;
    public GameObject backButtonMatchPlayers;

    public GameObject loginEmail;
    public InputField loginPassword;
    public GameObject loginInvalidEmailorPassword;
    public GameObject loginCanvas;
    public GameObject loader_;


    public GameObject regiterEmail;
    public InputField registerPassword;
    public GameObject registerNickname;
    public GameObject registerInvalidInput;
    public GameObject registerCanvas;

    public GameObject resetPasswordEmail;
    public GameObject resetPasswordInformationText;

    public bool isInLobby = false;
    public bool isInMaster = false;
    public static string  storePlayerAmount="0";
    void OnEnable()
    {
        Application.runInBackground = true;
        if (PlayerPrefs.HasKey("email_account"))
        {
            loader_.SetActive(true);
            LoginWithEmailAccount();
        }

       
    }
    void OnDisable()
    {
         storePlayerAmount = "0";
    }
void Awake()
    {


        PlayFabTitleID = StaticStrings.PlayFabTitleID;
        PhotonAppID = StaticStrings.PhotonAppID;
        PhotonChatID = StaticStrings.PhotonChatID;
        PlayFabSettings.TitleId = PlayFabTitleID;
        PhotonNetwork.OnEventCall += this.OnEvent;
        DontDestroyOnLoad(transform.gameObject);
        //  GameManager.Instance.playfabManager = this;
        //         if (GameManager.Instance.logged) {
        //           showMenu();
        //     }
    }



    /// <summary>
    /// This function is called when the MonoBehaviour will be destroyed.
    /// </summary>
    void OnDestroy()
    {
        PhotonNetwork.OnEventCall -= this.OnEvent;
    }

    public void destroy()
    {
        if (this.gameObject != null)
            DestroyImmediate(this.gameObject);
    }

    // Use this for initialization
    void Start()
    {
        Debug.Log("Timeout 4");
        PhotonNetwork.BackgroundTimeout = 0.0f;
        //PhotonNetwork.SwitchToProtocol(ConnectionProtocol.Tcp);
        PlayFabTitleID = StaticStrings.PlayFabTitleID;
        PhotonAppID = StaticStrings.PhotonAppID;
        PhotonChatID = StaticStrings.PhotonChatID;

        GameManager.Instance.playfabManager = this;

        fbManager = GameObject.Find("FacebookManager").GetComponent<FacebookManager>();
        facebookFriendsMenu = GameManager.Instance.facebookFriendsMenu;//fbButton.GetComponent <FacebookFriendsMenu> ();


        //		if (multiGame)
        //			Login ();
        //		else
        //			SceneManager.LoadScene ("GameScene");
    }

    public void GoogleLogin()
    {
        if (!Social.localUser.authenticated)
        {
            Social.localUser.Authenticate((bool success) =>
            {
                // Handle success or failure
                OnConnectionResponse(success);
            });
        }
    }

    private void OnConnectionResponse(bool authenticated)
    {
        if (authenticated)
        {
            Debug.LogError("Google Signed In");
            var serverAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();
            Debug.Log("Server Auth Code: " + serverAuthCode);

            PlayFabClientAPI.LoginWithGoogleAccount(new LoginWithGoogleAccountRequest()
            {
                TitleId = PlayFabSettings.TitleId,
                AccessToken = serverAuthCode,
                //AccessToken = authToken,
                //ServerAuthCode = serverAuthCode,
                CreateAccount = true
            }, (result) =>
            {
                Debug.Log("Login with Google!");
                if (result.NewlyCreated)
                {
                    Debug.Log("Acount Newly Created");
                }
                Debug.LogError("Signed In as " + result.PlayFabId);
                WebLogin(((PlayGamesLocalUser)Social.localUser).Email, serverAuthCode);

            }, OnPlayFabError);
        }
        else
        {
            Debug.LogError("Something wrong happened while Connecting to Google Play Services.");
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (chatClient != null) { chatClient.Service(); }
        // if(isa)

    }
    bool pauseApp = false;
    private void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            Debug.Log("conneccting============= : " + pause);
            connectToChat();
        }
    }




    // handle events:
    private void OnEvent(byte eventcode, object content, int senderid)
    {
        if (eventcode == 199)
        {
            string dd = (string)content;
            string[] dd1 = dd.Split('-');
            GameManager.Instance.opponentCueIndex = Int32.Parse(dd1[0]);
            GameManager.Instance.opponentCueTime = Int32.Parse(dd1[1]);
            opponentReady = true;
            StartCoroutine(waitAndStartGame());
        }
        else if (eventcode == 198)
        {
            Debug.Log("Received 198");
            GameManager.Instance.initPositions = (Vector3[])content;

            if (GameManager.Instance.initPositions == null) Debug.Log("null pos");
            else Debug.Log("not null pos");
            GameManager.Instance.receivedInitPositions = true;
        }
        else if (eventcode == 141 && GameManager.Instance.MatchPlayersCanvas.activeSelf)
        {
            GameManager.Instance.controlAvatars.waitingOpponentTime = StaticStrings.photonDisconnectTimeout;
            GameManager.Instance.controlAvatars.opponentActive = false;
            GameManager.Instance.controlAvatars.messageBubbleText.GetComponent<Text>().text = StaticStrings.waitingForOpponent + " " + StaticStrings.photonDisconnectTimeout;
            GameManager.Instance.controlAvatars.messageBubble.GetComponent<Animator>().Play("ShowBubble");

            StartCoroutine(GameManager.Instance.controlAvatars.updateMessageBubbleText());
        }
        else if (eventcode == 142 && GameManager.Instance.MatchPlayersCanvas.activeSelf)
        {
            GameManager.Instance.controlAvatars.CancelInvoke("showLongTimeMessage");
            GameManager.Instance.controlAvatars.opponentActive = true;
            GameManager.Instance.controlAvatars.messageBubble.GetComponent<Animator>().Play("HideBubble");
        }


    }

    private IEnumerator waitAndStartGame()
    {
        while (!opponentReady || !imReady || (!GameManager.Instance.roomOwner && !GameManager.Instance.receivedInitPositions))
        {
            yield return 0;
        }

        startGameScene();
        //Invoke ("startGameScene", 2);

        opponentReady = false;
        imReady = false;
    }

    public void startGameScene()
    {

        SceneManager.LoadScene("GameScene");
    }

    public void resetPassword()
    {
        resetPasswordInformationText.SetActive(false);

        SendAccountRecoveryEmailRequest request = new SendAccountRecoveryEmailRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            Email = resetPasswordEmail.GetComponent<Text>().text
        };

        PlayFabClientAPI.SendAccountRecoveryEmail(request, (result) =>
        {
            resetPasswordInformationText.SetActive(true);
            resetPasswordInformationText.GetComponent<Text>().text = "Email sent to your address. Check your inbox";


        }, (error) =>
        {
            resetPasswordInformationText.SetActive(true);
            resetPasswordInformationText.GetComponent<Text>().text = "Account with specified email doesn't exist";
        });
    }

    public void setInitNewAccountData()
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("Cues", "'0'");
        data.Add("Chats", "");
        data.Add("UsedCue", "'0';'0';'0';'0'");
        GameManager.Instance.ownedCues = "'0'";
        GameManager.Instance.ownedChats = "";
        UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
        {
            Data = data,
            Permission = UserDataPermission.Public
        };

        PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) => {
            Debug.Log("Initial data added");
        }, (error1) => {
            Debug.Log("Initial data add error " + error1.ErrorMessage);
        }, null);
    }

    public void setUsedCue(int index, int power, int aim, int time)
    {
        GameManager.Instance.cueIndex = index;
        GameManager.Instance.cuePower = power;
        GameManager.Instance.cueAim = aim;
        GameManager.Instance.cueTime = time;

        if (GameManager.Instance.cueController != null)
        {
            GameManager.Instance.cueController.changeCueImage(index);
        }

        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("UsedCue", "'" + index + "';" + "'" + power + "';" + "'" + aim + "';" + "'" + time + "'");
        UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
        {
            Data = data,
            Permission = UserDataPermission.Public
        };

        PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) => {
            Debug.Log("Cue changed playfab");
        }, (error1) => {
            Debug.Log("Cue changed playfab error " + error1.ErrorMessage);
        }, null);
    }

    public void updateBoughtCues(int index)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("Cues", GameManager.Instance.ownedCues + ";'" + index + "'");
        GameManager.Instance.ownedCues = GameManager.Instance.ownedCues + ";'" + index + "'";
        UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
        {
            Data = data,
            Permission = UserDataPermission.Public
        };

        PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) => {
            Debug.Log("Bought cue added");
            UpdateAmount();

        }, (error1) => {
            Debug.Log("Bought cue error " + error1.ErrorMessage);
        }, null);
    }
    public static string amountUpdate_;

    public void updateBoughtChats(int index)
    {
        Debug.Log("amountUpdate_\\\\\\ : " + amountUpdate_);
        Debug.Log("index.ToString()\\\\\\ : " + index.ToString());


        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("Chats", GameManager.Instance.ownedChats + ";'" + index + "'");

        GameManager.Instance.ownedChats = GameManager.Instance.ownedChats + ";'" + index + "'";
        UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
        {
            Data = data,
            Permission = UserDataPermission.Public
        };

        PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) => {
            Debug.Log("Bought chat added");
            UpdateAmount();
        }, (error1) => {
            Debug.Log("Bought chat error " + error1.ErrorMessage);
        }, null);
    }
    //public string amountUpdateUrl = "https://www.oxbowgaming.com/my-api/api/update_score.php";
    //public void UpdateAmount()
    //{
    //    WWW www;
    //    System.Collections.Hashtable postHeader = new System.Collections.Hashtable();
    //    postHeader.Add("Content-Type", "application/json");

    //    var form = new WWWForm();
    //    Debug.Log("======ameManager.amountUpdate========" + GameManager.amountUpdate);
    //    string callData = "{\"creditToPlay\": \"" + GameManager.amountUpdate + "\", \"jwt\": \"" + GameManager.playerToken + "\"}";

    //    var formData = System.Text.Encoding.UTF8.GetBytes(callData);

    //    www = new WWW(amountUpdateUrl, formData);
    //    StartCoroutine(UpdateAmountResponse(www));
    //}

    public void updateScoreOnOxBoxServer(string amount)
    {
        StartCoroutine(UpdateScore(amount));
    }

    IEnumerator UpdateScore(string amount)
    {
        string url = "https://www.oxbowgaming.com/ok/UpdateScore.php";
        var form = new WWWForm();

        form.AddField("username", PlayerPrefs.GetString("email_account"));
        form.AddField("amount", amount);

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();
        Debug.Log("Apis response : " + www.downloadHandler.text);
        if (www.error == null)
        {

        }
        Debug.LogError("Added" + PlayerPrefs.GetString("username") + amount);
    }

    public void UpdateAmount()
    {
        WWW www;
        var form = new WWWForm();
        Debug.Log("======ameManager.amountUpdate========" + GameManager.amountUpdate);
        string callData = "{\"updatedFreeCredit\": \"" + GameManager.Instance.coinsCount.ToString() + "\", \"jwt\": \"" + GameManager.playerToken + "\"}";
        var formData = System.Text.Encoding.UTF8.GetBytes(callData);
        www = new WWW(updatedFreeCreditUrl, formData);
        StartCoroutine(UpdateAmountResponse(www));
    }

    public string updatedFreeCreditUrl = "https://www.oxbowgaming.com/my-api/api/update_free_credit.php";
    public void UpdatedFreeCreditAd(string addAmt)
    {
        WWW www;
        var form = new WWWForm();
        Debug.Log("======ameManager.amountUpdate========" + addAmt);
        string callData = "{\"updatedFreeCredit\": \"" + addAmt + "\", \"jwt\": \"" + GameManager.playerToken + "\"}";
        var formData = System.Text.Encoding.UTF8.GetBytes(callData);
        www = new WWW(updatedFreeCreditUrl, formData);
        StartCoroutine(AdAmountResponse(www));
    }
    IEnumerator UpdateAmountResponse(WWW www)
    {
        InitMenuScript.ad_ = string.Empty;
        yield return www;
        if (www.error == null)
        {
            Debug.Log("WWW Ok!: " + www.text);
        }
    }
    IEnumerator AdAmountResponse(WWW www)
    {
        yield return www;
        if (www.error == null)
        {
            Debug.Log("WWW Ok!: " + www.text);
        }
    }
    public void addCoinsRequest(int count)
    {
        Debug.Log("======ameManager.count========" + count.ToString());
        if (!GameManager.Instance.offlineMode)
        {
            GameManager.Instance.coinsCount += count;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("Coins", "" + GameManager.Instance.coinsCount);
            UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
            {
                Data = data,
                Permission = UserDataPermission.Public
            };
            PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) => {
                if (GameManager.Instance.coinsTextMenu != null)
                {
                    updateCoinsTextMenu();
                    //GameManager.Instance.coinsTextMenu.GetComponent<Text>().text = GameManager.Instance.coinsCount + "";
                }
                if (GameManager.Instance.coinsTextShop != null)
                {
                    updateCoinsTextShop();
                    //GameManager.Instance.coinsTextMenu.GetComponent<Text>().text = GameManager.Instance.coinsCount + "";
                }
                Debug.Log("Coins updated successfull ");
            }, (error1) => {
                Debug.Log("Coins updated error " + error1.ErrorMessage);
            }, null);
        }
    }

    public void addCoinsRequestWatchVideo(int count)
    {
        Debug.Log("======ameManager.count========" + count.ToString());
        Debug.Log("======GameManager.Instance.coinsCount.count========" + GameManager.Instance.coinsCount.ToString());
        if (!GameManager.Instance.offlineMode)
        {
            GameManager.Instance.coinsCount += count;
            Debug.Log("======totalcount========" + GameManager.Instance.coinsCount.ToString());
            UpdatedFreeCreditAd(GameManager.Instance.coinsCount.ToString());
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("Coins", "" + GameManager.Instance.coinsCount);
            UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
            {
                Data = data,
                Permission = UserDataPermission.Public
            };
            PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) => {
                if (GameManager.Instance.coinsTextMenu != null)
                {
                    updateCoinsTextMenu();
                    //GameManager.Instance.coinsTextMenu.GetComponent<Text>().text = GameManager.Instance.coinsCount + "";
                }
                if (GameManager.Instance.coinsTextShop != null)
                {
                    updateCoinsTextShop();
                    //GameManager.Instance.coinsTextMenu.GetComponent<Text>().text = GameManager.Instance.coinsCount + "";
                }
                Debug.Log("Coins updated successfull "+ GameManager.Instance.coinsCount);
            }, (error1) => {
                Debug.Log("Coins updated error " + error1.ErrorMessage);
            }, null);
        }
    }

    public void updateCoinsTextMenu()
    {
        if (GameManager.Instance.coinsCount != 0)
        {
            GameManager.Instance.coinsTextMenu.GetComponent<Text>().text = GameManager.Instance.coinsCount.ToString("0,0", CultureInfo.InvariantCulture).Replace(',', ' ');
            Debug.Log("  GameManager.Instance.coinsTextMenu.GetComponent<Text>().text " + GameManager.Instance.coinsTextMenu.GetComponent<Text>().text);
        }
        else
        {
            GameManager.Instance.coinsTextMenu.GetComponent<Text>().text = "0";
        }
        if (!string.Equals(storePlayerAmount, GameManager.Instance.coinsCount.ToString()) && !string.Equals(storePlayerAmount,"0"))
        {
            Debug.Log("GameManager.Instance.coinsCount : " + GameManager.Instance.coinsCount + "   storePlayerAmount :" + storePlayerAmount);
           UpdatedFreeCreditAd(GameManager.Instance.coinsCount.ToString());
          //  UpdatedFreeCreditAd("1000");

        }
        storePlayerAmount = GameManager.Instance.coinsCount.ToString();
    }

    public void updateCoinsTextShop()
    {
        if (GameManager.Instance.coinsCount != 0)
        {
            GameManager.Instance.coinsTextShop.GetComponent<Text>().text = GameManager.Instance.coinsCount.ToString("0,0", CultureInfo.InvariantCulture).Replace(',', ' ');
        }
        else
        {
            GameManager.Instance.coinsTextShop.GetComponent<Text>().text = "0";
        }
    }

    public void getPlayerDataRequest()
    {
        GetUserDataRequest getdatarequest = new GetUserDataRequest()
        {
            PlayFabId = GameManager.Instance.playfabManager.PlayFabId,
        };

        PlayFabClientAPI.GetUserData(getdatarequest, (result) =>
        {

            Dictionary<string, UserDataRecord> data = result.Data;

            if (data.ContainsKey("Coins"))
            {
                Debug.Log("Got coins from playfab");
                GameManager.Instance.coinsCount = Int32.Parse(data["Coins"].Value);
                // if (GameManager.Instance.coinsTextMenu != null)
                // {
                //     GameManager.Instance.coinsTextMenu.GetComponent<Text>().text = GameManager.Instance.coinsCount + "";
                // }
            }
            else
            {
                GameManager.Instance.coinsCount = 0;
            }

            if (data.ContainsKey("UsedCue"))
            {
                string[] d = data["UsedCue"].Value.Split(';');
                GameManager.Instance.cueIndex = Int32.Parse(d[0].Replace("'", ""));
                GameManager.Instance.cuePower = Int32.Parse(d[1].Replace("'", ""));
                GameManager.Instance.cueAim = Int32.Parse(d[2].Replace("'", ""));
                GameManager.Instance.cueTime = Int32.Parse(d[3].Replace("'", ""));

                Debug.Log("Using cue: " + GameManager.Instance.cueIndex);
            }

            if (data.ContainsKey("Chats"))
            {
                if (data["Chats"].Value != null)
                    GameManager.Instance.ownedChats = data["Chats"].Value;
            }

            if (data.ContainsKey("Cues"))
            {

                GameManager.Instance.ownedCues = data["Cues"].Value;
                Debug.Log("Owned Cues: " + GameManager.Instance.ownedCues);
            }

            //SceneManager.LoadScene("Menu");
            StartCoroutine(loadSceneMenu());

        }, (error) =>
        {
            Debug.Log("Data updated error " + error.ErrorMessage);
        }, null);
    }


    private IEnumerator loadSceneMenu()
    {
        yield return new WaitForSeconds(0.1f);

        if (isInMaster && isInLobby)
        {
            SceneManager.LoadScene("Menu");
            if (loader_.gameObject != null)
            {
            loader_.SetActive(false);
            }

        }
        else
        {
            StartCoroutine(loadSceneMenu());
        }

    }

    string registerURL = "https://www.oxbowgaming.com/my-api/api/register.php";
    string rEmail = "";
    string rPassword = "";
    string rNickname = "";

    public void RegisterNewAccountWithID()
    {
        rEmail = regiterEmail.GetComponent<Text>().text;
        rPassword = registerPassword.text;
        rNickname = registerNickname.GetComponent<Text>().text;

        registerInvalidInput.SetActive(false);

        //if (Regex.IsMatch(rEmail, @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
        //    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$") && rPassword.Length >= 6 && rNickname.Length > 0)
        if (rEmail.Length > 0 && rPassword.Length > 0)
        {
            WWW www;
            System.Collections.Hashtable postHeader = new System.Collections.Hashtable();
            postHeader.Add("Content-Type", "application/json");

            var form = new WWWForm();

            string callData = "{\"username\": \"" + rNickname + "\", \"email\": \"" + rEmail + "\", \"password\": \"" + rPassword + "\"}";

            var formData = System.Text.Encoding.UTF8.GetBytes(callData);

            www = new WWW(registerURL, formData, postHeader);
            StartCoroutine(WaitForRegister(www));
        }
        else
        {
            registerInvalidInput.SetActive(true);
            registerInvalidInput.GetComponent<Text>().text = "Invalid input specified";
        }
    }

    IEnumerator WaitForRegister(WWW www)
    {
        yield return www;

        // check for errors
        if (www.error == null)
        {
            Debug.Log("WWW Ok!: " + www.text);
            ServerData registerData = JsonUtility.FromJson<ServerData>(www.text);
            Debug.LogError("name : " + registerData.username + " , free money : " + registerData.free_money);
            GameManager.Instance.loginKeyValue = registerData.jwt;
            GameManager.playerToken = registerData.jwt;
            GameManager.Instance.reward_points = registerData.reward_points;
            RegisterProfileOnPrefab(registerData.username, registerData.free_money);
        }
        else
        {
            Debug.Log("WWW Ok!: " + www.text);
            ServerData registerData = JsonUtility.FromJson<ServerData>(www.text);
            // GameManager.Instance.facebookManager.registerValidation.SetActive(true);
            //GameManager.Instance.facebookManager.registerValidationText.text = registerData.message;
            //Invoke("RegisterValidation", 6f);
            registerInvalidInput.SetActive(true);
            registerInvalidInput.GetComponent<Text>().text = registerData.message;
            Debug.Log("WWW Error: " + www.error);
            loginInvalidEmailorPassword.SetActive(true);
            Invoke("HideErrorText", 6f);
            //Debug.Log(error.ErrorMessage);
        }
    }
    public void HideErrorText()
    {
        registerInvalidInput.SetActive(false);
        registerInvalidInput.GetComponent<Text>().text = string.Empty;
    }

    public void RegisterValidation()
    {
        GameManager.Instance.facebookManager.registerValidation.SetActive(false);
        GameManager.Instance.facebookManager.registerValidationText.text = string.Empty;
    }

    void RegisterProfileOnPrefab(string name, string money)
    {
        RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            Email = rEmail,
            Password = rPassword,
            RequireBothUsernameAndEmail = false
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, (result) =>
        {

            PlayFabId = result.PlayFabId;
            Debug.Log("Got PlayFabID: " + PlayFabId);

            registerCanvas.SetActive(false);
            PlayerPrefs.SetString("email_account", rEmail);
            PlayerPrefs.SetString("password", rPassword);
            PlayerPrefs.SetString("LoggedType", "EmailAccount");
            PlayerPrefs.Save();
            GameManager.Instance.nameMy = name;

            //Dictionary<string, string> data = new Dictionary<string, string>();
            //UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
            //{
            //    Data = data,
            //    Permission = UserDataPermission.Public
            //};

            UpdateUserTitleDisplayNameRequest displayNameRequest = new UpdateUserTitleDisplayNameRequest()
            {
                DisplayName = GameManager.Instance.playfabManager.PlayFabId
            };

            PlayFabClientAPI.UpdateUserTitleDisplayName(displayNameRequest, (response) =>
            {
                Debug.Log("Title Display name updated successfully");
            }, (error) =>
            {
                Debug.Log("Title Display name updated error: " + error.Error);

            }, null);

            int bal = Convert.ToInt32(Math.Floor(Convert.ToDouble(money)));
            Debug.LogError("money : " + bal);

            Dictionary<string, string> data = new Dictionary<string, string>();

            data.Add("LoggedType", "EmailAccount");
            data.Add("PlayerName", GameManager.Instance.nameMy);
            data.Add("Coins", bal + "");

            UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
            {
                Data = data,
                Permission = UserDataPermission.Public
            };
            //GameManager.Instance.myPlayerData.UpdateUserData(data);

            PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) =>
            {
                Debug.Log("Data updated successfull ");
            }, (error1) =>
            {
                Debug.Log("Data updated error " + error1.ErrorMessage);
            }, null);

            fbManager.showLoadingCanvas();
            GetPhotonToken();

            Invoke("ChangeSceneToMenu", 3);
        },
        (error) =>
        {
            registerInvalidInput.SetActive(true);
            registerInvalidInput.GetComponent<Text>().text = error.ErrorMessage;
            Debug.Log("Error registering new account with email: " + error.ErrorMessage + "\n" + error.ErrorDetails);
        });
    }

    void ChangeSceneToMenu()
    {
        SceneManager.LoadScene("Menu");
    }


    public void LoginWithFacebook()
    {
        LoginWithFacebookRequest request = new LoginWithFacebookRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            CreateAccount = true,
            //   AccessToken = Facebook.Unity.AccessToken.CurrentAccessToken.TokenString //fb
        };

        PlayFabClientAPI.LoginWithFacebook(request, (result) =>
        {
            PlayFabId = result.PlayFabId;
            Debug.Log("Got PlayFabID: " + PlayFabId);

            if (result.NewlyCreated)
            {
                Debug.Log("(new account)");
                setInitNewAccountData();
                addCoinsRequest(StaticStrings.initCoinsCount);
            }
            else
            {
                Debug.Log("(existing account)");
            }


            UpdateUserTitleDisplayNameRequest displayNameRequest = new UpdateUserTitleDisplayNameRequest()
            {
                //DisplayName = GameManager.Instance.nameMy,
                DisplayName = GameManager.Instance.playfabManager.PlayFabId
            };

            PlayFabClientAPI.UpdateUserTitleDisplayName(displayNameRequest, (response) =>
            {
                Debug.Log("Title Display name updated successfully");
            }, (error) =>
            {
                Debug.Log("Title Display name updated error: " + error.Error);

            }, null);


            Dictionary<string, string> data = new Dictionary<string, string>();

            data.Add("LoggedType", "Facebook");
            //  data.Add("FacebookID", Facebook.Unity.AccessToken.CurrentAccessToken.UserId);  //fb
            if (result.NewlyCreated)
                data.Add("PlayerName", GameManager.Instance.nameMy);
            else
            {
                GetUserDataRequest getdatarequest = new GetUserDataRequest()
                {
                    PlayFabId = result.PlayFabId,

                };

                PlayFabClientAPI.GetUserData(getdatarequest, (result2) =>
                {

                    Dictionary<string, UserDataRecord> data2 = result2.Data;


                    GameManager.Instance.nameMy = data2["PlayerName"].Value;
                }, (error) =>
                {
                    Debug.Log("Data updated error " + error.ErrorMessage);
                }, null);
            }
            data.Add("PlayerAvatarUrl", GameManager.Instance.avatarMyUrl);

            UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
            {
                Data = data,
                Permission = UserDataPermission.Public
            };

            PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) =>
            {
                Debug.Log("Data updated successfull ");
            }, (error1) =>
            {
                Debug.Log("Data updated error " + error1.ErrorMessage);
            }, null);




            GetPhotonToken();




            //GetFriends ();
        },
            (error) =>
            {
                Debug.Log("Error logging in player with custom ID: " + error.ErrorMessage + "\n" + error.ErrorDetails);
                GameManager.Instance.connectionLost.showDialog();
                //Debug.Log(error.ErrorMessage);
            });
    }
    private void OnApplicationQuit()
    {
        Debug.Log("Clear");
        //PlayerPrefs.DeleteKey("LoggedType");
    }
    private string androidUnique()
    {
        AndroidJavaClass androidUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityPlayerActivity = androidUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject unityPlayerResolver = unityPlayerActivity.Call<AndroidJavaObject>("getContentResolver");
        AndroidJavaClass androidSettingsSecure = new AndroidJavaClass("android.provider.Settings$Secure");
        return androidSettingsSecure.CallStatic<string>("getString", unityPlayerResolver, "android_id");
    }

    string loginURL = "https://www.oxbowgaming.com/my-api/api/login.php";
    string email = "";
    string password = "";

    public void LoginWithEmailAccount()
    {
        loginInvalidEmailorPassword.SetActive(false);

        if (PlayerPrefs.HasKey("email_account"))
        {
            email = PlayerPrefs.GetString("email_account");
            password = PlayerPrefs.GetString("password");
            AutoLogin.storeEmail = email;
            AutoLogin.storePassword = password;

        }
        else
        {
            email = loginEmail.GetComponent<Text>().text;
            password = loginPassword.text;
        }

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            //egisterInvalidInput.SetActive(true);
            //registerInvalidInput.GetComponent<Text>().text = "Please Enter UeserName Password";
            Debug.Log("email" + email);

            GameManager.Instance.facebookManager.registerValidationText.text = "Please Enter UeserName Password";
            GameManager.Instance.facebookManager.registerValidation.SetActive(true);
            Invoke("RegisterValidation", 6f);
            return;
        }
        WebLogin(email, password);
    }


    public void AutoLoginPlayer()
    {

        AutoLogin.storeEmail = PlayerPrefs.GetString("email");
        AutoLogin.storePassword = PlayerPrefs.GetString("pass");
        Debug.Log("username ===:" + AutoLogin.storeEmail);
        Debug.Log("pass== : " + AutoLogin.storePassword);
        //AutoLogin. storeEmail = PlayerPrefs.GetString("email");
        //storePassword = PlayerPrefs.GetString("pass");


    }

    public void WebLogin(string wEmail, string wPassword)
    {
        //PlayerPrefs.DeleteAll();
        //PlayerPrefs.SetString("email", wEmail);
        //PlayerPrefs.SetString("pass", password);
        //AutoLoginPlayer();
        //Debug.Log("username :" + AutoLogin.storeEmail);
        //Debug.Log("pass : " + AutoLogin.storePassword);

        loader_.SetActive(true);

        WWW www;
        System.Collections.Hashtable postHeader = new System.Collections.Hashtable();
        postHeader.Add("Content-Type", "application/json");

        var form = new WWWForm();
        string callData = "{\"email\": \"" + wEmail + "\", \"password\": \"" + wPassword + "\"}";
        //string callData = "{\"email\": \"mepunit@gmail.com\", \"password\": \"P@ssw0rd!\"}";

        var formData = System.Text.Encoding.UTF8.GetBytes(callData);
        www = new WWW(loginURL, formData, postHeader);
        StartCoroutine(WaitForRequest(www));
    }
    string getEmail = string.Empty;
    IEnumerator WaitForRequest(WWW www)
    {
        yield return www;

        // check for errors
        if (www.error == null)
        {
            Debug.Log("WWW Ok!: " + www.text);
            ServerData loginData = JsonUtility.FromJson<ServerData>(www.text);
            Debug.LogError("name : " + loginData.username);
            GameManager.Instance.loginKeyValue = loginData.jwt;
            GameManager.playerToken = loginData.jwt;

            getEmail = loginData.email;
            GameManager.Instance.reward_points = loginData.reward_points;
            SetLoginParameters(loginData.username, loginData.free_money, loginData.reward_points);


        }
        else
        {
            loader_.SetActive(false);

            Debug.Log("WWW Error: " + www.error);
            Debug.Log("WWW Ok!: " + www.text);
            // loginInvalidEmailorPassword.SetActive(true);
            ServerData registerData = JsonUtility.FromJson<ServerData>(www.text);
            if (registerData.message == "Login failed.")
            {
                Debug.Log("registerData.message " + registerData.message);

                GameManager.Instance.facebookManager.registerValidation.SetActive(true);
                GameManager.Instance.facebookManager.registerValidationText.text = " Please enter a valid email address";
                Invoke("RegisterValidation", 6f);
            }
            else
            {
                Debug.Log("registerData.message " + registerData.message);

                GameManager.Instance.facebookManager.registerValidation.SetActive(true);
                GameManager.Instance.facebookManager.registerValidationText.text = registerData.message;
                Invoke("RegisterValidation", 6f);
            }

            Debug.Log("WWW Error: " + www.error);
            loginInvalidEmailorPassword.SetActive(true);
            //Debug.Log(error.ErrorMessage);

        }
    }

    string loginKeyValue;

    private void SetLoginParameters(string name, string money, string redeem_points)
    {
        LoginWithEmailAddressRequest request = new LoginWithEmailAddressRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            Email = getEmail,
            Password = password
        };

        //PlayFabClientAPI.RegisterPlayFabUser(request, (result) =>
        PlayFabClientAPI.LoginWithEmailAddress(request, (result) =>
        {
            PlayFabId = result.PlayFabId;
            Debug.Log("Got PlayFabID: " + PlayFabId);
            PlayerPrefs.SetString("email_account", email);
            PlayerPrefs.SetString("password", password);
            PlayerPrefs.SetString("LoggedType", "EmailAccount");
            PlayerPrefs.SetString("token", GameManager.playerToken);
            PlayerPrefs.Save();
            AutoLogin.storeEmail = PlayerPrefs.GetString("email_account");
            AutoLogin.storePassword = PlayerPrefs.GetString("password");
            Debug.Log("username ===:" + AutoLogin.storeEmail);
            Debug.Log("pass== : " + AutoLogin.storePassword);
            Debug.Log("password== : " + PlayerPrefs.GetString("password"));

            if (result.NewlyCreated)
            {
                Debug.Log("(new account)");
                setInitNewAccountData();
                //addCoinsRequest(StaticStrings.initCoinsCount);
            }
            else
            {
                //CheckIfFirstTitleLogin(PlayFabId, false);
                Debug.Log("(existing account)");
            }

            UpdateUserTitleDisplayNameRequest displayNameRequest = new UpdateUserTitleDisplayNameRequest()
            {
                //DisplayName = name,
                DisplayName = GameManager.Instance.playfabManager.PlayFabId
            };

            PlayFabClientAPI.UpdateUserTitleDisplayName(displayNameRequest, (response) =>
            {
                Debug.Log("Title Display name updated successfully");
            }, (error) =>
            {
                Debug.Log("Title Display name updated error: " + error.Error);

            }, null);

            int bal = Convert.ToInt32(Math.Floor(Convert.ToDouble(money)));
            Debug.LogError("money : " + bal);
            Debug.LogError("redeem_points : " + redeem_points);
            GameManager.Instance.nameMy = name;
            Dictionary<string, string> data = new Dictionary<string, string>();

            data.Add("LoggedType", "EmailAccount");
            data.Add("PlayerName", GameManager.Instance.nameMy);
            data.Add("Coins", bal + "");
            // data.Add(MyPlayerData.LOGIN_KEY, GameManager.Instance.loginKeyValue);
            // GameManager.Instance.myPlayerData.UpdateUserData(data);

            UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
            {
                Data = data,
                Permission = UserDataPermission.Public
            };
            // GameManager.Instance.myPlayerData.UpdateUserData(data);

            PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) =>
            {
                //  loader_.SetActive(false);
                Debug.Log("Data updated successfull ");
            }, (error1) =>
            {
                // loader_.SetActive(false);
                Debug.Log("Data updated error " + error1.ErrorMessage);
            }, null);

            //fbManager.showLoadingCanvas();
            GetPhotonToken();            
                Invoke("ChangeSceneToMenu", 3);            
            
        }, (error) => {
            //loginInvalidEmailorPassword.SetActive(true);
            Debug.LogError("Error logging in player with custom ID: " + error.ErrorMessage);
            //Debug.Log(error.ErrorMessage);
            LoginErrorRegister(name, money);
            //GameManager.Instance.connectionLost.showDialog();
        });

    }


    private void LoginErrorRegister(string name, string money)
    {
        RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest()
        {

            TitleId = PlayFabSettings.TitleId,
            Email = getEmail,
            Password = password,
            RequireBothUsernameAndEmail = false
        };
        Debug.Log("================= PlayFabSettings.TitleId=====================" + PlayFabSettings.TitleId);
        Debug.Log("================= email.TitleId=====================" + email);
        Debug.Log("================= password.TitleId=====================" + password);
        Debug.Log("================= RequireBothUsernameAndEmail.TitleId=====================");

        PlayFabClientAPI.RegisterPlayFabUser(request, (result) =>
        {
            PlayFabId = result.PlayFabId;
            Debug.LogError("Got PlayFabID: " + PlayFabId);
            PlayerPrefs.SetString("email_account", email);
            PlayerPrefs.SetString("password", password);
            PlayerPrefs.SetString("LoggedType", "EmailAccount");
            PlayerPrefs.Save();

            setInitNewAccountData();

            UpdateUserTitleDisplayNameRequest displayNameRequest = new UpdateUserTitleDisplayNameRequest()
            {
                DisplayName = GameManager.Instance.playfabManager.PlayFabId
            };

            PlayFabClientAPI.UpdateUserTitleDisplayName(displayNameRequest, (response) =>
            {
                Debug.LogError("Title Display name updated successfully");
            }, (error) =>
            {
                Debug.LogError("Title Display name updated error: " + error.Error);

            }, null);

            int bal = Convert.ToInt32(Math.Floor(Convert.ToDouble(money)));
            Debug.LogError("money : " + bal);

            GameManager.Instance.nameMy = name;

            Dictionary<string, string> data = new Dictionary<string, string>();

            data.Add("LoggedType", "EmailAccount");
            data.Add("PlayerName", GameManager.Instance.nameMy);
            data.Add("Coins", bal + "");

            UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
            {
                Data = data,
                Permission = UserDataPermission.Public
            };
            // GameManager.Instance.myPlayerData.UpdateUserData(data);




            PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) =>
            {
                Debug.LogError("Data updated successfull ");
            }, (error1) =>
            {
                Debug.LogError("Data updated error " + error1.ErrorMessage);
            }, null);

            //fbManager.showLoadingCanvas();
            GetPhotonToken();
            Invoke("ChangeSceneToMenu", 3);
        },
        (error) =>
        {
            registerInvalidInput.SetActive(true);
            registerInvalidInput.GetComponent<Text>().text = error.ErrorMessage;
            Debug.LogError("Error registering new account with email: " + error.ErrorMessage + "\n" + error.ErrorDetails);
        });
    }



    private void RegisterPlayerOnPlayFab(string name, string money)
    {
        RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest()
        {

            TitleId = PlayFabSettings.TitleId,
            Email = email,
            Password = password,
            RequireBothUsernameAndEmail = false
        };
        Debug.Log("================= PlayFabSettings.TitleId=====================" + PlayFabSettings.TitleId);
        Debug.Log("================= email.TitleId=====================" + email);
        Debug.Log("================= password.TitleId=====================" + password);
        Debug.Log("================= RequireBothUsernameAndEmail.TitleId=====================");

        PlayFabClientAPI.RegisterPlayFabUser(request, (result) =>
        {
            PlayFabId = result.PlayFabId;
            Debug.LogError("Got PlayFabID: " + PlayFabId);
            PlayerPrefs.SetString("email_account", email);
            PlayerPrefs.SetString("password", password);
            PlayerPrefs.SetString("LoggedType", "EmailAccount");
            PlayerPrefs.Save();

            setInitNewAccountData();

            UpdateUserTitleDisplayNameRequest displayNameRequest = new UpdateUserTitleDisplayNameRequest()
            {
                DisplayName = GameManager.Instance.playfabManager.PlayFabId
            };

            PlayFabClientAPI.UpdateUserTitleDisplayName(displayNameRequest, (response) =>
            {
                Debug.LogError("Title Display name updated successfully");
            }, (error) =>
            {
                Debug.LogError("Title Display name updated error: " + error.Error);

            }, null);

            int bal = Convert.ToInt32(Math.Floor(Convert.ToDouble(money)));
            Debug.LogError("money : " + bal);

            GameManager.Instance.nameMy = name;

            Dictionary<string, string> data = new Dictionary<string, string>();

            data.Add("LoggedType", "EmailAccount");
            data.Add("PlayerName", GameManager.Instance.nameMy);
            data.Add("Coins", bal + "");

            UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
            {
                Data = data,
                Permission = UserDataPermission.Public
            };
            // GameManager.Instance.myPlayerData.UpdateUserData(data);




            PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) =>
            {
                Debug.LogError("Data updated successfull ");
            }, (error1) =>
            {
                Debug.LogError("Data updated error " + error1.ErrorMessage);
            }, null);

            //fbManager.showLoadingCanvas();
            GetPhotonToken();
            Invoke("ChangeSceneToMenu", 3);
        },
        (error) =>
        {
            registerInvalidInput.SetActive(true);
            registerInvalidInput.GetComponent<Text>().text = error.ErrorMessage;
            Debug.LogError("Error registering new account with email: " + error.ErrorMessage + "\n" + error.ErrorDetails);
        });
    }

    public void Login()
    {
        string customId = "";
        if (PlayerPrefs.HasKey("unique_identifier"))
        {
            customId = PlayerPrefs.GetString("unique_identifier");
        }
        else
        {
            customId = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString("unique_identifier", customId);
        }




        Debug.Log("UNIQUE IDENTIFIER: " + customId);

        LoginWithCustomIDRequest request = new LoginWithCustomIDRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            CreateAccount = true,
            CustomId = customId //SystemInfo.deviceUniqueIdentifier
        };



        PlayFabClientAPI.LoginWithCustomID(request, (result) =>
        {
            PlayFabId = result.PlayFabId;
            Debug.Log("Got PlayFabID: " + PlayFabId);

            Dictionary<string, string> data = new Dictionary<string, string>();

            if (result.NewlyCreated)
            {
                Debug.Log("(new account)");
                setInitNewAccountData();
                //addCoinsRequest(StaticStrings.initCoinsCount);
            }
            else
            {
                Debug.Log("(existing account)");
            }



            string name = result.PlayFabId;
            if (PlayerPrefs.HasKey("GuestPlayerName"))
            {
                name = PlayerPrefs.GetString("GuestPlayerName");
            }
            else
            {
                name = "Guest";
                for (int i = 0; i < 6; i++)
                {
                    name += UnityEngine.Random.Range(0, 9);
                }
                PlayerPrefs.SetString("GuestPlayerName", name);
                PlayerPrefs.Save();
            }


            data.Add("LoggedType", "Guest");
            data.Add("PlayerName", name);


            UpdateUserTitleDisplayNameRequest displayNameRequest = new UpdateUserTitleDisplayNameRequest()
            {
                //DisplayName = name,
                DisplayName = GameManager.Instance.playfabManager.PlayFabId
            };

            PlayFabClientAPI.UpdateUserTitleDisplayName(displayNameRequest, (response) =>
            {
                Debug.Log("Title Display name updated successfully");
            }, (error) =>
            {
                Debug.Log("Title Display name updated error: " + error.Error);

            }, null);


            UpdateUserDataRequest userDataRequest = new UpdateUserDataRequest()
            {
                Data = data,
                Permission = UserDataPermission.Public
            };

            PlayFabClientAPI.UpdateUserData(userDataRequest, (result1) =>
            {
                Debug.Log("Data updated successfull ");
            }, (error1) =>
            {
                Debug.Log("Data updated error " + error1.ErrorMessage);
            }, null);

            GameManager.Instance.nameMy = name;

            PlayerPrefs.SetString("LoggedType", "Guest");
            PlayerPrefs.Save();

            fbManager.showLoadingCanvas();


            GetPhotonToken();

        },
            (error) =>
            {
                Debug.Log("Error logging in player with custom ID:");
                Debug.Log(error.ErrorMessage);
                GameManager.Instance.connectionLost.showDialog();
            });
    }


    // List<string> playfabFriendsName = new List<string>();
    // public void GetPlayfabFriends()
    // {
    //     if (alreadyGotFriends)
    //     {
    //         Debug.Log("show firneds FFFF");
    //         if (PlayerPrefs.GetString("LoggedType").Equals("Facebook"))
    //         {
    //             fbManager.getFacebookInvitableFriends();
    //         }
    //         else
    //         {

    //             facebookFriendsMenu.showFriends(null, null, null);
    //         }
    //     }
    //     else
    //     {
    //         Debug.Log("IND");
    //         GetFriendsListRequest request = new GetFriendsListRequest();
    //         request.IncludeFacebookFriends = true;
    //         PlayFabClientAPI.GetFriendsList(request, (result) =>
    //         {

    //             Debug.Log("Friends list Playfab: " + result.Friends.Count);
    //             var friends = result.Friends;

    //             List<string> playfabFriends = new List<string>();
    //             playfabFriendsName = new List<string>();
    //             List<string> playfabFriendsFacebookId = new List<string>();


    //             chatClient.RemoveFriends(GameManager.Instance.friendsIDForStatus.ToArray());

    //             List<string> friendsToStatus = new List<string>();


    //             if(friends.Count > 0) {
    //                 for(int i=0; i<friends.Count; i++)
    //                 //foreach (var friend in friends)
    //                 {

    //                     var friend = friends[i];


    //                     playfabFriends.Add(friend.FriendPlayFabId);

    //                     Debug.Log("Title: " + friend.TitleDisplayName);
    //                     GetUserDataRequest getdatarequest = new GetUserDataRequest()
    //                     {
    //                         PlayFabId = friend.TitleDisplayName,
    //                     };

    //                     int ii = i;

    //                     PlayFabClientAPI.GetUserData(getdatarequest, (result2) =>
    //                     {

    //                         Dictionary<string, UserDataRecord> data2 = result2.Data;

    //                         playfabFriendsName.Add(data2["PlayerName"].Value);
    //                         Debug.Log("Added " + data2["PlayerName"].Value);


    //                         if(ii == friends.Count - 1) {
    //                             GameManager.Instance.friendsIDForStatus = friendsToStatus;

    //                             chatClient.AddFriends(friendsToStatus.ToArray());



    //                             GameManager.Instance.facebookFriendsMenu.addPlayFabFriends(playfabFriends, playfabFriendsName, playfabFriendsFacebookId);
    //                             //facebookFriendsMenu.addPlayFabFriends (playfabFriends, playfabFriendsName, playfabFriendsFacebookId);

    //                             if (PlayerPrefs.GetString("LoggedType").Equals("Facebook"))
    //                             {
    //                                 fbManager.getFacebookInvitableFriends();
    //                             }
    //                             else
    //                             {
    //                                 GameManager.Instance.facebookFriendsMenu.showFriends(null, null, null);
    //                                 //facebookFriendsMenu.showFriends (null, null, null);
    //                             }
    //                             //alreadyGotFriends = true;
    //                         }
    //                         //GameManager.Instance.nameMy = data2["PlayerName"].Value;

    //                     }, (error) =>
    //                     {
    //                         playfabFriendsName.Add("Unknown");
    //                         if(ii == friends.Count - 1) {
    //                             GameManager.Instance.friendsIDForStatus = friendsToStatus;

    //                             chatClient.AddFriends(friendsToStatus.ToArray());



    //                             GameManager.Instance.facebookFriendsMenu.addPlayFabFriends(playfabFriends, playfabFriendsName, playfabFriendsFacebookId);
    //                             //facebookFriendsMenu.addPlayFabFriends (playfabFriends, playfabFriendsName, playfabFriendsFacebookId);

    //                             if (PlayerPrefs.GetString("LoggedType").Equals("Facebook"))
    //                             {
    //                                 fbManager.getFacebookInvitableFriends();
    //                             }
    //                             else
    //                             {
    //                                 GameManager.Instance.facebookFriendsMenu.showFriends(null, null, null);
    //                                 //facebookFriendsMenu.showFriends (null, null, null);
    //                             }
    //                             //alreadyGotFriends = true;
    //                         }
    //                         Debug.Log("Data updated error " + error.ErrorMessage);
    //                     }, null);


    //                     friendsToStatus.Add(friend.FriendPlayFabId);
    //                 }
    //             } else {
    //                 GameManager.Instance.friendsIDForStatus = friendsToStatus;

    //                         chatClient.AddFriends(friendsToStatus.ToArray());



    //                         GameManager.Instance.facebookFriendsMenu.addPlayFabFriends(playfabFriends, playfabFriendsName, playfabFriendsFacebookId);
    //                         //facebookFriendsMenu.addPlayFabFriends (playfabFriends, playfabFriendsName, playfabFriendsFacebookId);

    //                         if (PlayerPrefs.GetString("LoggedType").Equals("Facebook"))
    //                         {
    //                             fbManager.getFacebookInvitableFriends();
    //                         }
    //                         else
    //                         {
    //                             GameManager.Instance.facebookFriendsMenu.showFriends(null, null, null);
    //                             //facebookFriendsMenu.showFriends (null, null, null);
    //                         }
    //                         //alreadyGotFriends = true;
    //             }

    //         }, OnPlayFabError);
    //     }


    // }

    public void GetPlayfabFriends()
    {
        if (alreadyGotFriends)
        {
            Debug.Log("show firneds FFFF");
            if (PlayerPrefs.GetString("LoggedType").Equals("Facebook"))
            {
                fbManager.getFacebookInvitableFriends();
            }
            else
            {

                facebookFriendsMenu.showFriends(null, null, null);
            }
        }
        else
        {
            Debug.Log("IND");
            GetFriendsListRequest request = new GetFriendsListRequest();
            request.IncludeFacebookFriends = true;
            PlayFabClientAPI.GetFriendsList(request, (result) =>
            {

                Debug.Log("Friends list Playfab: " + result.Friends.Count);
                var friends = result.Friends;

                List<string> playfabFriends = new List<string>();
                List<string> playfabFriendsName = new List<string>();
                List<string> playfabFriendsFacebookId = new List<string>();


                chatClient.RemoveFriends(GameManager.Instance.friendsIDForStatus.ToArray());

                List<string> friendsToStatus = new List<string>();


                int index = 0;
                foreach (var friend in friends)
                {


                    playfabFriends.Add(friend.FriendPlayFabId);

                    Debug.Log("Title: " + friend.TitleDisplayName);
                    GetUserDataRequest getdatarequest = new GetUserDataRequest()
                    {
                        PlayFabId = friend.TitleDisplayName,
                    };


                    int ind2 = index;

                    PlayFabClientAPI.GetUserData(getdatarequest, (result2) =>
                    {

                        Dictionary<string, UserDataRecord> data2 = result2.Data;
                        playfabFriendsName[ind2] = data2["PlayerName"].Value;
                        Debug.Log("Added " + data2["PlayerName"].Value);
                        GameManager.Instance.facebookFriendsMenu.updateName(ind2, data2["PlayerName"].Value, friend.TitleDisplayName);

                    }, (error) =>
                    {

                        Debug.Log("Data updated error " + error.ErrorMessage);
                    }, null);

                    playfabFriendsName.Add("");

                    friendsToStatus.Add(friend.FriendPlayFabId);

                    index++;
                }

                GameManager.Instance.friendsIDForStatus = friendsToStatus;

                chatClient.AddFriends(friendsToStatus.ToArray());



                GameManager.Instance.facebookFriendsMenu.addPlayFabFriends(playfabFriends, playfabFriendsName, playfabFriendsFacebookId);
                //facebookFriendsMenu.addPlayFabFriends (playfabFriends, playfabFriendsName, playfabFriendsFacebookId);

                if (PlayerPrefs.GetString("LoggedType").Equals("Facebook"))
                {
                    fbManager.getFacebookInvitableFriends();
                }
                else
                {
                    GameManager.Instance.facebookFriendsMenu.showFriends(null, null, null);
                    //facebookFriendsMenu.showFriends (null, null, null);
                }
                //alreadyGotFriends = true;


            }, OnPlayFabError);
        }


    }

    // Generic PlayFab callback for errors.
    void OnPlayFabError(PlayFabError error)
    {
        Debug.LogError("Playfab Error: " + error.ErrorMessage);
    }

    // #######################  PHOTON  ##########################

    void GetPhotonToken()
    {
        GetPhotonAuthenticationTokenRequest request = new GetPhotonAuthenticationTokenRequest();
        request.PhotonApplicationId = PhotonAppID.Trim();//GameConstants.PhotonAppId.Trim();
                                                         // get an authentication ticket to pass on to Photon
        PlayFabClientAPI.GetPhotonAuthenticationToken(request, OnPhotonAuthenticationSuccess, OnPlayFabError);
    }


    public string authToken;
    // callback on successful GetPhotonAuthenticationToken request 
    void OnPhotonAuthenticationSuccess(GetPhotonAuthenticationTokenResult result)
    {
        string photonToken = result.PhotonCustomAuthenticationToken;
        Debug.Log(string.Format("Yay, logged in session token: {0}", photonToken));
        PhotonNetwork.AuthValues = new AuthenticationValues();
        PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.Custom;
        PhotonNetwork.AuthValues.AddAuthParameter("username", this.PlayFabId);
        PhotonNetwork.AuthValues.AddAuthParameter("Token", result.PhotonCustomAuthenticationToken);
        PhotonNetwork.AuthValues.UserId = this.PlayFabId;
        PhotonNetwork.ConnectUsingSettings("1.0");
        PhotonNetwork.playerName = this.PlayFabId;

        //		PhotonNetwork.JoinLobby ();


        authToken = result.PhotonCustomAuthenticationToken;
        // chatClient = new ChatClient(this);
        // GameManager.Instance.chatClient = chatClient;
        // // Set your favourite region. "EU", "US", and "ASIA" are currently supported.
        // ExitGames.Client.Photon.Chat.AuthenticationValues authValues = new ExitGames.Client.Photon.Chat.AuthenticationValues();
        // authValues.UserId = this.PlayFabId;
        // authValues.AuthType = ExitGames.Client.Photon.Chat.CustomAuthenticationType.Custom;
        // authValues.AddAuthParameter("username", this.PlayFabId);
        // authValues.AddAuthParameter("Token", result.PhotonCustomAuthenticationToken);
        // chatClient.Connect(this.PhotonChatID, "1.0", authValues);
        getPlayerDataRequest();
        connectToChat();

    }

    public void connectToChat()
    {
        chatClient = new ChatClient(this);
        GameManager.Instance.chatClient = chatClient;
        // Set your favourite region. "EU", "US", and "ASIA" are currently supported.
        ExitGames.Client.Photon.Chat.AuthenticationValues authValues = new ExitGames.Client.Photon.Chat.AuthenticationValues();
        authValues.UserId = this.PlayFabId;
        authValues.AuthType = ExitGames.Client.Photon.Chat.CustomAuthenticationType.Custom;
        authValues.AddAuthParameter("username", this.PlayFabId);
        authValues.AddAuthParameter("Token", authToken);
        chatClient.Connect(this.PhotonChatID, "1.0", authValues);
    }

    public void OnConnected()
    {
        Debug.Log("Photon Chat connected!!!");
        chatClient.Subscribe(new string[] { "invitationsChannel" });
    }

    public void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        GameManager.Instance.opponentDisconnected = true;



        if (GameManager.Instance.controlAvatars != null)
        {
            if (GameManager.Instance.readyToAnimateCoins)
            {
                GameManager.Instance.controlAvatars.playerDisconnected();

            }
            else
            {
                GameManager.Instance.controlAvatars.playerRejected = true;
            }

            GameManager.Instance.controlAvatars.hideMessageBubble();

            //GameManager.Instance.controlAvatars.playerRejected = true;
        }

        if (GameManager.Instance.cueController != null)
        {
            GameManager.Instance.cueController.HideAllControllers();
            Debug.Log("Player disconnected. You won");
            GameManager.Instance.playerDisconnected = true;
            PhotonNetwork.LeaveRoom();
            GameManager.Instance.iWon = true;
            GameManager.Instance.gameControllerScript.showMessage(GameManager.Instance.nameOpponent + " disconnected from room");
            GameManager.Instance.stopTimer = true;
            GameManager.Instance.cueController.youWonMessage.SetActive(true);
            GameManager.Instance.audioSources[3].Play();
            GameManager.Instance.cueController.youWonMessage.GetComponent<Animator>().Play("YouWinMessageAnimation");
        }
    }

    public void showMenu()
    {

        menuCanvas.gameObject.SetActive(true);

        playerName.GetComponent<Text>().text = GameManager.Instance.nameMy;

        if (GameManager.Instance.avatarMy != null)
            playerAvatar.GetComponent<Image>().sprite = GameManager.Instance.avatarMy;

        splashCanvas.SetActive(false);
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        Debug.Log("Subscribed to a new channel - set online status!");

        //splashCanvas.SetActive (false);

        chatClient.SetOnlineStatus(ChatUserStatus.Online);


        // getPlayerDataRequest();
        //SceneManager.LoadScene("Menu");

        //menuCanvas.gameObject.SetActive (true);

        //playerName.GetComponent <Text> ().text = GameManager.Instance.nameMy;

        //if(GameManager.Instance.avatarMy != null)
        //	playerAvatar.GetComponent <Image> ().sprite = GameManager.Instance.avatarMy;


    }


    public void challengeFriend(string id, string message)
    {
        chatClient.SendPrivateMessage(id, "INVITE_SEND;" + id + this.PlayFabId + ";" + GameManager.Instance.nameMy + ";" + message);

        //chatClient.PublishMessage( "invitationsChannel", "So Long, and Thanks for All the Fish!" );
        Debug.Log("Send invitation to: " + id);

        //		RoomOptions roomOptions = new RoomOptions() { isVisible = false, maxPlayers = 2 };
        //		PhotonNetwork.JoinOrCreateRoom(id+this.PlayFabId, roomOptions, TypedLobby.Default);

    }




    string roomname;
    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        if (!sender.Equals(this.PlayFabId))
        {
            if (message.ToString().Contains("INVITE_SEND"))
            {
                string roomName = message.ToString().Split(';')[1];
                int payout = Int32.Parse(message.ToString().Split(';')[3]);
                GameManager.Instance.tableNumber = Int32.Parse(message.ToString().Split(';')[4]);
                Debug.Log("INVITE_SEND " + message + "  " + sender + " room: " + roomName);
                GameManager.Instance.payoutCoins = payout;
                GameManager.Instance.invitationDialog.GetComponent<PhotonChatListener>().showInvitationDialog(0, message.ToString().Split(';')[2], sender, roomName);

                //			GameManager.Instance.invitationDialog.GetComponent<Animator> ().Play ("InvitationDialogShow");
                //			GameObject.Find ("InvitationDialog").GetComponent<Animator> ().Play ("InvitationDialogShow");

            }
            else if (message.ToString().Contains("INVITE_ACCEPT"))
            {
                string roomName = message.ToString().Split(';')[1];
                Debug.Log("INVITE_ACCEPT " + message + "  " + sender + " room: " + roomName);
                GameManager.Instance.invitationDialog.GetComponent<PhotonChatListener>().showInvitationDialog(2, message.ToString().Split(';')[2], sender, roomName);
                //			GameManager.Instance.invitationDialog.GetComponent<Animator> ().Play ("InvitationDialogShow");
                //			GameObject.Find ("InvitationDialog").GetComponent<Animator> ().Play ("InvitationDialogShow");

            }
            else if (message.ToString().Contains("INVITE_REJECT"))
            {
                string roomName = message.ToString().Split(';')[1];
                Debug.Log("INVITE_REJECT " + message + "  " + sender + " room: " + roomName);
                GameManager.Instance.invitationDialog.GetComponent<PhotonChatListener>().showInvitationDialog(1, message.ToString().Split(';')[2], sender, roomName);
                //			GameManager.Instance.invitationDialog.GetComponent<Animator> ().Play ("InvitationDialogShow");
                //			GameObject.Find ("InvitationDialog").GetComponent<Animator> ().Play ("InvitationDialogShow");

            }
            else if (message.ToString().Contains("INVITE_START"))
            {
                string roomName = message.ToString().Split(';')[1];
                //				PhotonNetwork.JoinRoom (roomName);

                Debug.Log("INVITE_START " + message + "  " + sender + " room: " + roomName);
                //				GameManager.Instance.invitationDialog.GetComponent <PhotonChatListener> ().showInvitationDialog (1, message.ToString ().Split (';') [2], sender, roomName);
                //			GameManager.Instance.invitationDialog.GetComponent<Animator> ().Play ("InvitationDialogShow");
                //			GameObject.Find ("InvitationDialog").GetComponent<Animator> ().Play ("InvitationDialogShow");

            }
            else if (message.ToString().Contains("INVITE_STOP"))
            {
                string roomName = message.ToString().Split(';')[1];
                Debug.Log("INVITE_STOP " + message + "  " + sender + " room: " + roomName);


                GameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().playerRejected = true;

                GetUserDataRequest getdatarequest = new GetUserDataRequest()
                {
                    PlayFabId = sender,

                };

                PlayFabClientAPI.GetUserData(getdatarequest, (result) => {

                    Dictionary<string, UserDataRecord> data = result.Data;


                    if (data.ContainsKey("LoggedType"))
                    {
                        if (data["LoggedType"].Value.Equals("Facebook"))
                        {
                            //callApiToGetOpponentData (data ["FacebookID"].Value);
                            getOpponentData(data);
                        }
                        else
                        {
                            Debug.Log("DUPADUPA");
                            if (data.ContainsKey("PlayerName"))
                            {
                                GameManager.Instance.nameOpponent = data["PlayerName"].Value;
                                GameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                                backButtonMatchPlayers.SetActive(false);
                            }
                            else
                            {
                                GameManager.Instance.nameOpponent = "Guest453678";
                                GameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                                GameManager.Instance.controlAvatars.playerRejected = true;
                            }


                            //SceneManager.LoadScene ("GameScene");
                        }
                    }
                    else
                    {
                        GameManager.Instance.nameOpponent = "Guest453678";
                        GameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                        GameManager.Instance.controlAvatars.playerRejected = true;
                    }

                }, (error) =>
                {
                    Debug.Log("Data updated error " + error.ErrorMessage);
                }, null);



                //				GameManager.Instance.invitationDialog.GetComponent <PhotonChatListener> ().showInvitationDialog (1, message.ToString ().Split (';') [2], sender, roomName);
                //			GameManager.Instance.invitationDialog.GetComponent<Animator> ().Play ("InvitationDialogShow");
                //			GameObject.Find ("InvitationDialog").GetComponent<Animator> ().Play ("InvitationDialogShow");

            }
        }
        //		Debug.Log ("INVITE RECEIVED " + message + "  " + sender);
        //
        //		roomname = message.ToString ();
        //
        //		Invoke ("join", 3.0f);

    }

    public void join()
    {
        PhotonNetwork.JoinRoom(roomname);
    }

    public void DebugReturn(DebugLevel level, string message)
    {

    }

    public void OnChatStateChange(ChatState state)
    {

    }


    public override void OnDisconnectedFromPhoton()
    {
        Debug.Log("Disconnected from photon");
        //GameManager.Instance.connectionLost.showDialog();
        switchUser();
    }

    public void DisconnecteFromPhoton()
    {
        PhotonNetwork.Disconnect();
    }
    public void ForgotPassword()
    {
        Application.OpenURL("https://www.oxbowgaming.com/forgot_pass.php");
    }
    public void switchUser()
    {
        //if(GameManager.Instance.playfabManager != null)
        GameManager.Instance.playfabManager.destroy();
        //if(GameManager.Instance.facebookManager != null)    
        GameManager.Instance.facebookManager.destroy();
        //if(GameManager.Instance.connectionLost != null)
        GameManager.Instance.connectionLost.destroy();
        //if(GameManager.Instance.adsScript != null)
        // GameManager.Instance.adsScript.destroy(); adm
        GameManager.Instance.avatarMy = null;
        GameManager.Instance.logged = false;

        PlayerPrefs.DeleteAll();
        GameManager.Instance.resetAllData();
        GameManager.Instance.coinsCount = 0;
        SceneManager.LoadScene("LoginSplash");
    }

    public void OnDisconnected()
    {
        //  Debug.Log("Chat disconnected called!!!!!!!!!!! Reconnect");
        connectToChat();

        //GameManager.Instance.connectionLost.showDialog();
    }



    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {

    }

    public void OnUnsubscribed(string[] channels)
    {

    }


    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        Debug.Log("STATUS UPDATE CHAT!");
        Debug.Log("Status change for: " + user + " to: " + status);

        bool foundFriend = false;
        for (int i = 0; i < GameManager.Instance.friendsStatuses.Count; i++)
        {
            string[] friend = GameManager.Instance.friendsStatuses[i];
            if (friend[0].Equals(user))
            {
                GameManager.Instance.friendsStatuses[i][1] = "" + status;
                foundFriend = true;
                break;
            }
        }

        if (!foundFriend)
        {
            GameManager.Instance.friendsStatuses.Add(new string[] { user, "" + status });
        }

        if (GameManager.Instance.facebookFriendsMenu != null)
            GameManager.Instance.facebookFriendsMenu.updateFriendStatus(status, user);
    }



    public override void OnJoinedLobby()
    {
        //getCoinsRequest();
        Debug.Log("OnJoinedLobby");

        isInLobby = true;
        //		PhotonNetwork.JoinRandomRoom();
    }

    public override void OnConnectedToMaster()
    {
        isInMaster = true;
        // when AutoJoinLobby is off, this method gets called when PUN finished the connection (instead of OnJoinedLobby())
        //PhotonNetwork.JoinRandomRoom();
        //		RoomOptions roomOptions = new RoomOptions() { isVisible = false, maxPlayers = 2 };
        //		PhotonNetwork.JoinOrCreateRoom("debugRoom", roomOptions, TypedLobby.Default);

        PhotonNetwork.JoinLobby();

    }

    public void JoinRoomAndStartGame()
    {
        //		RoomOptions roomOptions = new RoomOptions () { isVisible = false, maxPlayers = 2 };
        //		//PhotonNetwork.Joi
        //		PhotonNetwork.JoinOrCreateRoom("debugRoom", roomOptions, TypedLobby.Default);

        ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "tbl", GameManager.Instance.tableNumber }, { "isAvailable", true } };
        PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, 0);
    }

    public void OnPhotonRandomJoinFailed()
    {

        //RoomOptions roomOptions = new RoomOptions () { isVisible = true, maxPlayers = 2 };
        // PhotonNetwork.CreateRoom (null, roomOptions, TypedLobby.Default);
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.CustomRoomPropertiesForLobby = new String[] { "tbl", "isAvailable" };
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "tbl", GameManager.Instance.tableNumber }, { "isAvailable", true } };
        roomOptions.MaxPlayers = 2;
        roomOptions.IsVisible = true;
        PhotonNetwork.CreateRoom(null, roomOptions, TypedLobby.Default);


    }




    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
        Debug.Log("Owner room: " + roomOwner);

        GameManager.Instance.avatarOpponent = null;

        if (!roomOwner)
        {

            GameManager.Instance.backButtonMatchPlayers.SetActive(false);

            GetUserDataRequest getdatarequest = new GetUserDataRequest()
            {
                PlayFabId = PhotonNetwork.otherPlayers[0].name,

            };

            PlayFabClientAPI.GetUserData(getdatarequest, (result) =>
            {

                Dictionary<string, UserDataRecord> data = result.Data;


                if (data.ContainsKey("LoggedType"))
                {
                    if (data["LoggedType"].Value.Equals("Facebook"))
                    {
                        //callApiToGetOpponentData (data ["FacebookID"].Value);
                        getOpponentData(data);
                    }
                    else
                    {
                        Debug.Log("DUPADUPA");
                        if (data.ContainsKey("PlayerName"))
                        {
                            GameManager.Instance.nameOpponent = data["PlayerName"].Value;
                            GameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                        }
                        else
                        {
                            GameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                            GameManager.Instance.controlAvatars.playerRejected = true;
                            GameManager.Instance.nameOpponent = "Guest568253";
                        }


                        //SceneManager.LoadScene ("GameScene");
                    }
                }
                else
                {
                    GameManager.Instance.nameOpponent = "Guest453678";
                    GameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                    GameManager.Instance.controlAvatars.playerRejected = true;
                }

            }, (error) =>
            {
                Debug.Log("Data updated error " + error.ErrorMessage);
            }, null);


        }



    }


    public override void OnCreatedRoom()
    {        
        roomOwner = true;
        GameManager.Instance.roomOwner = true;
        Debug.Log("OnCreatedRoom");

    }

    public override void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom called");
        if (!GameManager.Instance.otherPlayerJoined)
        {
            Debug.Log("AI_calling");
            roomOwner = true;
            GameManager.Instance.roomOwner = true;
        }
        else
        {
            roomOwner = false;
            GameManager.Instance.roomOwner = false;
            GameManager.Instance.resetAllData();
        }       

    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {

        GameManager.Instance.controlAvatars.hideLongTimeMessage();
        //SceneManager.LoadScene ("GameScene");
        PhotonNetwork.room.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "isAvailable", false } });
        Debug.Log("New player joined");

        GameManager.Instance.otherPlayerJoined = true;
        GameManager.Instance.backButtonMatchPlayers.SetActive(false);
        //backButtonMatchPlayers.SetActive (false);
        GetUserDataRequest getdatarequest = new GetUserDataRequest()
        {
            PlayFabId = PhotonNetwork.otherPlayers[0].name,

        };

        PlayFabClientAPI.GetUserData(getdatarequest, (result) =>
        {
            Debug.Log("Data updated successfull ");
            Dictionary<string, UserDataRecord> data = result.Data;



            if (data.ContainsKey("LoggedType"))
            {
                if (data["LoggedType"].Value.Equals("Facebook"))
                {

                    getOpponentData(data);
                    //callApiToGetOpponentData (data["FacebookID"].Value);

                    //				data.Add("PlayerName", GameManager.Instance.nameMy);
                    //				data.Add("PlayerAvatarUrl"

                }
                else
                {
                    if (data.ContainsKey("PlayerName"))
                    {
                        GameManager.Instance.nameOpponent = data["PlayerName"].Value;
                        GameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                    }
                    else
                    {
                        GameManager.Instance.nameOpponent = "Guest675824";
                        GameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                        GameManager.Instance.controlAvatars.playerRejected = true;
                    }




                    //SceneManager.LoadScene ("GameScene");

                }
            }
            else
            {
                GameManager.Instance.nameOpponent = "Guest453678";
                GameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
                GameManager.Instance.controlAvatars.playerRejected = true;
            }

        }, (error) =>
        {
            Debug.Log("Data updated error " + error.ErrorMessage);
        }, null);
    }

    private void getOpponentData(Dictionary<string, UserDataRecord> data)
    {
        if (data.ContainsKey("PlayerName"))
            GameManager.Instance.nameOpponent = data["PlayerName"].Value;
        else
            GameManager.Instance.nameOpponent = "Guest857643";
        if (data.ContainsKey("PlayerAvatarUrl"))
        {
            StartCoroutine(loadImageOpponent(data["PlayerAvatarUrl"].Value));
        }
        else
        {
            GameManager.Instance.avatarOpponent = null;
            GameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
            GameManager.Instance.backButtonMatchPlayers.SetActive(false);
        }

    }

    public IEnumerator loadImageOpponent(string url)
    {
        // Load avatar image

        Debug.Log("Opponent image url: " + url);
        // Start a download of the given URL
        WWW www = new WWW(url);

        // Wait for download to complete
        yield return www;


        GameManager.Instance.avatarOpponent = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0.5f, 0.5f), 32);

        GameManager.Instance.MatchPlayersCanvas.GetComponent<ControlAvatars>().foundPlayer = true;
        GameManager.Instance.backButtonMatchPlayers.SetActive(false);

        //		SceneManager.LoadScene ("GameScene");
    }

    //	private void callApiToGetOpponentData(string id)
    //	{
    //
    //
    //		FB.API(id + "?fields=first_name", Facebook.Unity.HttpMethod.GET, delegate(IGraphResult result) {
    //			GameManager.Instance.nameOpponent = result.ResultDictionary ["first_name"].ToString ();
    //
    //			FB.API("/" + id + "/picture?type=square&height=92&width=92", Facebook.Unity.HttpMethod.GET, delegate(IGraphResult result2) {
    //				if (result2.Texture != null) {
    //					// use texture
    //					GameManager.Instance.avatarOpponent = Sprite.Create(result2.Texture, new Rect(0, 0, result2.Texture.width, result2.Texture.height), new Vector2(0.5f, 0.5f), 32);
    //					SceneManager.LoadScene ("GameScene");
    //				}
    //			});
    //		});
    //	}









    //	public void OnGUI()
    //	{
    //		GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
    //	}





}

[System.Serializable]
public class ServerData
{
    public string message;
    public string jwt;
    public string email;
    public string free_money;
    public string wallet;
    public string username;
    public string reward_points;
}