using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AssemblyCSharp;

public class YouWinMessageChangeSprite : MonoBehaviour {

    public Sprite other;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void changeSprite() {
        GetComponent<Image>().sprite = other;
    }

    public void loadWinnerScene() {
        if (GameManager.Instance.offlineMode) {
            if (GameManager.Instance.Player_AI)
            {
                SceneManager.LoadScene("WinnerScene");
            }
            else {
                GameManager.Instance.playfabManager.roomOwner = false;
                GameManager.Instance.roomOwner = false;
                GameManager.Instance.resetAllData();
                SceneManager.LoadScene("Menu");
                Debug.Log("Timeout 7");
                PhotonNetwork.BackgroundTimeout = 0;
            }


            
          //  if (GameManager.Instance.offlineMode && StaticStrings.showAdWhenLeaveGame) adm
               // GameManager.Instance.adsScript.ShowAd(); adm

        } else {
            SceneManager.LoadScene("WinnerScene");
        }

    }
}
