using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoLogin : MonoBehaviour {
    public static AutoLogin Instance;
    public static string storeEmail ;
    public static string storePassword;
    // Use this for initialization
    public void Awake()
    {
        Instance = this;
    }
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
