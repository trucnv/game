using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StartVideo : MonoBehaviour {
    public GameObject VideoScreen;
    public VideoPlayer video;
    private VideoClip videoClip;
    //public AudioSource audioSource;
    private void Awake()
    {
        video.Prepare();
        StartCoroutine("VideoStart");
    }
    IEnumerator VideoStart()
    {
        while(video.isPrepared == false)
        {
            yield return new WaitForSeconds(0f);
        }
        video.transform.GetComponent<RawImage>().enabled = true;
        video.transform.GetComponent<RawImage>().texture = video.texture;
        video.Play();        
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
