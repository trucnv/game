using AssemblyCSharp;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialtoPlay : MonoBehaviour {
    public GameObject tutorialStart;
    public GameObject detail1;
    public GameObject detail2;
    public GameObject Canvas;
    public void TutorialStart()
    {
        
        tutorialStart.SetActive(false);
        //detail1.SetActive(false);
        //detail2.SetActive(true);
        StartCoroutine("Explain");
    }
    IEnumerator Explain()
    {
        yield return new WaitForSeconds(8.0f);

        detail1.SetActive(false);
        detail2.SetActive(true);
    }
    public void StartOnClick()
    {
        //addCoinsRequest(0);
        Canvas.SetActive(false);
        

    }
	// Use this for initialization
	void Start () {
        
       

    }
    
	
	// Update is called once per frame
	void Update () {
		
	}   
}
