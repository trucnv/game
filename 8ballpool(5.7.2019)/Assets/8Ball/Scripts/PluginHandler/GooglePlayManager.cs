using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GooglePlayManager : MonoBehaviour
{

    public static GooglePlayManager instance;

    // Use this for initialization
    void Awake()
    {
        instance = this;

        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
        .RequestEmail()
        // enables saving game progress.
        .EnableSavedGames()
        .Build();

        PlayGamesPlatform.InitializeInstance(config);
        // recommended for debugging:
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        //OnConnectionResponse(PlayGamesPlatform.Instance.localUser.authenticated);
    }


    //private void OnConnectionResponse(bool authenticated)
    //{
    //    if (authenticated)
    //    {

    //    }
    //    else
    //    {

    //    }
    //}
}
