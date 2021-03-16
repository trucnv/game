using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using LitJson;

public class CoinManagementScript : MonoBehaviour {
	[SerializeField]private Text[] EnterFees;
	[SerializeField]private Text[] WinningPrice;
	[SerializeField]private int[] EnterFeesCount;

	private float cashPercentage;
	string url = "https://oxbowgaming.com/ok/api.php";
	string url1 = "https://min-api.cryptocompare.com/data/price?fsym=BTC&tsyms=USD";

	IEnumerator After1Sec(){
		WWW apitemp = new WWW (url1);
		yield return apitemp;
		if(apitemp.error==null){
			ProcessTemp (apitemp.text);
		}
		float temp = float.Parse (val) + 1000f;
		//Debug.Log (val + " after add " + temp);
	}
		
	void callFuntion(){
		StartCoroutine(After1Sec());
	}
	void Update()
	{
		//Debug.Log (GameManager.Instance.CommissionPercentage);
	}

	// Sample JSON for the following script has attached.
	IEnumerator Start()
	{
		InvokeRepeating("callFuntion",1f,1f);
		
		//StartCoroutine (After1Sec (), 1);
		
		/*
		WWW www = new WWW(url);
		print(www.text);
		var theGameObject = new JsonReader(www.text) ;
		*/


		WWW www = new WWW(url);
		yield return www;
		if (www.error == null)
		{
			Processjson(www.text);
			for(int i=0;i<EnterFees.Length;i++){
				float tempPercentage = (100.0f - cashPercentage) * 0.01f;
				GameManager.Instance.CommissionPercentage = tempPercentage;
				Debug.Log (tempPercentage);
				WinningPrice[i].text =  ((int)(tempPercentage * (EnterFeesCount[i] * 2))+1).ToString();
			}
		}
		else
		{
			Debug.Log("ERROR: " + www.error);
		}
	}
	private void Processjson(string jsonString)
	{
		JsonData jsonvale = JsonMapper.ToObject(jsonString);
		Debug.Log (jsonvale["EBOOK_APP"][0]["pool_freecash"].ToString());
		cashPercentage = float.Parse (jsonvale["EBOOK_APP"][0]["pool_freecash"].ToString());
		Debug.Log (cashPercentage);
	}

	string val;
	private void ProcessTemp(string jsonVal)
	{
		JsonData jsonvale = JsonMapper.ToObject(jsonVal);
		val = jsonvale["USD"].ToString();
	}
}