using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text;
using System;

public class RedeemPoints : MonoBehaviour {

	public Text textPointsNeeded;
	public Dropdown chooseReward;
	public Button buttonChooseReward;
	public Dropdown rewardPointsToRedeem;
	public List<Dropdown.OptionData> data;
	public string RewardValue;
	public string RewardDetails;
    public Text textAvailablePoints;
    public GameObject RewardResultObject;
    public Text TextRewardResult;

	// Use this for initialization
	void Start () {

		data = new List<Dropdown.OptionData>();

		data.Add (new Dropdown.OptionData("Amazon"));
		data.Add (new Dropdown.OptionData("Flipkart"));
		data.Add (new Dropdown.OptionData("Dominos"));
		data.Add (new Dropdown.OptionData("PizzaHut"));
		//data.Add (new Dropdown.OptionData("Oxbow Gaming Real Cash Voucher"));
		data.Add (new Dropdown.OptionData("Samsung Memory Card"));
		data.Add (new Dropdown.OptionData("Sandisk Pen Drive"));
        data.Add(new Dropdown.OptionData("Oxbow Gaming Specials"));
        data.Add(new Dropdown.OptionData("Powerbank"));
        data.Add(new Dropdown.OptionData("PayTM"));
        data.Add(new Dropdown.OptionData("Speakers"));
        data.Add(new Dropdown.OptionData("Computer Accessories"));
        data.Add(new Dropdown.OptionData("Smartphone & Tablet"));
        data.Add(new Dropdown.OptionData("Earphones & headphones"));
        // data.Add (new Dropdown.OptionData("Samsung Memory Card"));

        chooseReward.ClearOptions ();
		chooseReward.AddOptions (data);

		RewardChange (0);

		UpdatePointsNeeded ();

		chooseReward.onValueChanged.AddListener (RewardChange);
		rewardPointsToRedeem.onValueChanged.AddListener (RewardOptionChanged);
        textAvailablePoints.text = Convert.ToDouble(GameManager.Instance.reward_points) + "";

        buttonChooseReward.onClick.AddListener (RewardClick);
	}

	// Update is called once per frame
	void Update () {

	}
    void UpdatePointsNeeded()
    {
        string rewardSelection = chooseReward.captionText.text;
        string rewardOption = rewardPointsToRedeem.captionText.text;

        Dictionary<string, string> hashMap = new Dictionary<string, string>();

        hashMap.Add("Amazon-300", "4500");
        hashMap.Add("Amazon-500", "7500");
        hashMap.Add("Amazon-2000", "30000");
        hashMap.Add("Amazon-3000", "45000");

        hashMap.Add("Flipkart-300", "4500");
        hashMap.Add("Flipkart-500", "7500");
        hashMap.Add("Flipkart-1000", "15000");
        hashMap.Add("Flipkart-2000", "30000");

        hashMap.Add("Dominos-300", "4800");
        hashMap.Add("Dominos-500", "8000");

        hashMap.Add("PizzaHut-300", "4800");
        hashMap.Add("PizzaHut-500", "8000");

        hashMap.Add("PayTM-100", "1800");
        hashMap.Add("PayTM-300", "5400");
        hashMap.Add("PayTM-500", "7000");
        hashMap.Add("PayTM-1000", "12000");

        hashMap.Add("Oxbow Gaming Real Cash Voucher-100", "1500");
        hashMap.Add("Oxbow Gaming Real Cash Voucher-300", "4500");
        hashMap.Add("Oxbow Gaming Real Cash Voucher-500", "7000");
        hashMap.Add("Oxbow Gaming Real Cash Voucher-1000", "12000");
        hashMap.Add("Oxbow Gaming Real Cash Voucher-2000", "20000");
        hashMap.Add("Oxbow Gaming Real Cash Voucher-3000", "30000");

        hashMap.Add("Samsung Memory Card-32GB", "10500");
        hashMap.Add("Samsung Memory Card-64GB", "22400");
        hashMap.Add("Samsung Memory Card-128GB", "52800");

        hashMap.Add("Sandisk Pen Drive-32GB", "11400");
        hashMap.Add("Sandisk Pen Drive-64GB", "25500");
        hashMap.Add("Sandisk Pen Drive-128GB", "46000");

        hashMap.Add("Powerbank-Lenovo Power Bank", "30000");
        hashMap.Add("Powerbank-Portronics Power Wallet", "33500");
        hashMap.Add("Powerbank-PORTRONICS TORK", "32000");

        hashMap.Add("Speakers-Portrnics Sound cake portable", "32000");
        hashMap.Add("Speakers-BOSE SOUNDLINK COLOR BT", "175000");
        hashMap.Add("Speakers-Jbl flip4 waterproof speaker", "125000");
        hashMap.Add("Speakers-Logitech Z120 Stereo", "14400");
        hashMap.Add("Speakers-Portronics sublime", "40000");

        hashMap.Add("Computer Accessories-Logitech Wireless Mouse", "11000");
        hashMap.Add("Computer Accessories-Logitech Wireless Keyboard", "14000");
        hashMap.Add("Computer Accessories-Toshiba 1TB external hard disk", "64000");

        hashMap.Add("Smartphone & Tablet-Lenovo Tab4 10 Tablet", "265000");
        hashMap.Add("Smartphone & Tablet-Apple ipad(6th Gen) Tablet", "450000");
        hashMap.Add("Smartphone & Tablet-Galaxy J6", "225000");
        hashMap.Add("Smartphone & Tablet-Galaxy A6", "315000");
        hashMap.Add("Smartphone & Tablet-Nokia 3", "145000");
        hashMap.Add("Smartphone & Tablet-Nokia 2", "100000");
        hashMap.Add("Smartphone & Tablet-Champkia bluetooth watch", "20000");

        hashMap.Add("Earphones & headphones-Samsung level U Bluetooth", "50000");
        hashMap.Add("Earphones & headphones-BOSE SOUNDSPORT IN-EAR", "160000");
        hashMap.Add("Earphones & headphones-Motorola Pulse Max", "21000");
        hashMap.Add("Earphones & headphones-Boss Sounsport Wirelles", "200000");

        hashMap.Add("Oxbow Gaming Specials-Oxbowgaming t-shirt", "10000");
        hashMap.Add("Oxbow Gaming Specials-Oxbowgaming travel bag", "13000");
        hashMap.Add("Oxbow Gaming Specials-Oxbowgaming Insta Laptop bag", "25000");
        hashMap.Add("Oxbow Gaming Specials-Oxbowgaming Pen", "750");
        hashMap.Add("Oxbow Gaming Specials-Oxbowgaming Cap", "500");
        hashMap.Add("Oxbow Gaming Specials-Oxbowgaming Bottle", "2000");
        hashMap.Add("Oxbow Gaming Specials-Oxbowgaming Mug", "1000");

        string rewardValue;
        hashMap.TryGetValue(rewardSelection + "-" + rewardOption, out rewardValue);
        Debug.Log(rewardSelection + "-" + rewardOption + " = " + rewardValue);

        textPointsNeeded.text = rewardValue;

        RewardDetails = rewardSelection + "-" + rewardOption;
        RewardValue = rewardValue;
    }

    //void UpdatePointsNeeded() {
    //	string rewardSelection = chooseReward.captionText.text;
    //	string rewardOption = rewardPointsToRedeem.captionText.text;

    //	Dictionary<string, string> hashMap = new Dictionary<string, string>();

    //	hashMap.Add ("Amazon-300", "4500");
    //	hashMap.Add ("Amazon-500", "7500");
    //	hashMap.Add ("Amazon-2000", "30000");
    //	hashMap.Add ("Amazon-3000", "45000");

    //	hashMap.Add ("Flipkart-300", "4500");
    //	hashMap.Add ("Flipkart-500", "7500");
    //	hashMap.Add ("Flipkart-1000", "15000");
    //	hashMap.Add ("Flipkart-2000", "30000");

    //	hashMap.Add ("Dominos-300", "4800");
    //	hashMap.Add ("Dominos-500", "8000");

    //	hashMap.Add ("PizzaHut-300", "4800");
    //	hashMap.Add ("PizzaHut-500", "8000");

    //	hashMap.Add ("Oxbow Gaming Real Cash Voucher-100", "1500");
    //	hashMap.Add ("Oxbow Gaming Real Cash Voucher-300", "4500");
    //	hashMap.Add ("Oxbow Gaming Real Cash Voucher-500", "7000");
    //	hashMap.Add ("Oxbow Gaming Real Cash Voucher-1000", "12000");
    //	hashMap.Add ("Oxbow Gaming Real Cash Voucher-2000", "20000");
    //	hashMap.Add ("Oxbow Gaming Real Cash Voucher-3000", "30000");

    //	hashMap.Add ("Samsung Memory Card-32GB", "10500");
    //	hashMap.Add ("Samsung Memory Card-64GB", "22400");
    //	hashMap.Add ("Samsung Memory Card-128GB", "52800");

    //	hashMap.Add ("Sandisk Pen Drive-32GB", "11400");
    //	hashMap.Add ("Sandisk Pen Drive-64GB", "25500");
    //	hashMap.Add ("Sandisk Pen Drive-128GB", "46000");

    //	string rewardValue;
    //	hashMap.TryGetValue (rewardSelection + "-" + rewardOption, out rewardValue);
    //	Debug.Log (rewardSelection + "-" + rewardOption + " = " + rewardValue);

    //	textPointsNeeded.text = rewardValue;

    //	RewardDetails = rewardSelection + "-" + rewardOption;
    //	RewardValue = rewardValue;
    //}

    void RewardOptionChanged(int change) {
		UpdatePointsNeeded ();
	}

    //void RewardChange(int change) {
    //	List<Dropdown.OptionData> dataPointsOptions = new List<Dropdown.OptionData>();

    //	Debug.Log ("Reward selection changed to: " + data[change].text);

    //	switch(data[change].text) {
    //		case "Amazon":
    //			{
    //				dataPointsOptions.Add (new Dropdown.OptionData("300"));
    //				dataPointsOptions.Add (new Dropdown.OptionData("500"));
    //				dataPointsOptions.Add (new Dropdown.OptionData("2000"));
    //				dataPointsOptions.Add (new Dropdown.OptionData("3000"));
    //			}
    //			break;

    //		case "Flipkart":
    //			{
    //				dataPointsOptions.Add (new Dropdown.OptionData("300"));
    //				dataPointsOptions.Add (new Dropdown.OptionData("500"));
    //				dataPointsOptions.Add (new Dropdown.OptionData("1000"));
    //				dataPointsOptions.Add (new Dropdown.OptionData("2000"));
    //			}
    //			break;

    //		case "Dominos":
    //			{
    //				dataPointsOptions.Add (new Dropdown.OptionData("300"));
    //				dataPointsOptions.Add (new Dropdown.OptionData("500"));
    //			}
    //			break;

    //		case "PizzaHut":
    //			{
    //				dataPointsOptions.Add (new Dropdown.OptionData("300"));
    //				dataPointsOptions.Add (new Dropdown.OptionData("500"));
    //			}
    //			break;

    //		case "Oxbow Gaming Real Cash Voucher":
    //			{
    //				dataPointsOptions.Add (new Dropdown.OptionData("100"));
    //				dataPointsOptions.Add (new Dropdown.OptionData("300"));
    //				dataPointsOptions.Add (new Dropdown.OptionData("500"));
    //				dataPointsOptions.Add (new Dropdown.OptionData("1000"));
    //				dataPointsOptions.Add (new Dropdown.OptionData("2000"));
    //				dataPointsOptions.Add (new Dropdown.OptionData("3000"));
    //			}
    //			break;

    //		case "Samsung Memory Card":
    //			{
    //				dataPointsOptions.Add (new Dropdown.OptionData("32GB"));
    //				dataPointsOptions.Add (new Dropdown.OptionData("64GB"));
    //				dataPointsOptions.Add (new Dropdown.OptionData("128GB"));
    //			}
    //			break;

    //		case "Sandisk Pen Drive":
    //			{
    //				dataPointsOptions.Add (new Dropdown.OptionData("32GB"));
    //				dataPointsOptions.Add (new Dropdown.OptionData("64GB"));
    //				dataPointsOptions.Add (new Dropdown.OptionData("128GB"));
    //			}
    //			break;
    //	}

    //	rewardPointsToRedeem.ClearOptions();
    //	rewardPointsToRedeem.AddOptions(dataPointsOptions);

    //	UpdatePointsNeeded ();
    //}

    void RewardChange(int change)
    {
        List<Dropdown.OptionData> dataPointsOptions = new List<Dropdown.OptionData>();

        Debug.Log("Reward selection changed to: " + data[change].text);

        switch (data[change].text)
        {
            case "Amazon":
                {
                    dataPointsOptions.Add(new Dropdown.OptionData("300"));
                    dataPointsOptions.Add(new Dropdown.OptionData("500"));
                    dataPointsOptions.Add(new Dropdown.OptionData("2000"));
                    dataPointsOptions.Add(new Dropdown.OptionData("3000"));
                }
                break;

            case "Flipkart":
                {
                    dataPointsOptions.Add(new Dropdown.OptionData("300"));
                    dataPointsOptions.Add(new Dropdown.OptionData("500"));
                    dataPointsOptions.Add(new Dropdown.OptionData("1000"));
                    dataPointsOptions.Add(new Dropdown.OptionData("2000"));
                }
                break;

            case "Dominos":
                {
                    dataPointsOptions.Add(new Dropdown.OptionData("300"));
                    dataPointsOptions.Add(new Dropdown.OptionData("500"));
                }
                break;

            case "PizzaHut":
                {
                    dataPointsOptions.Add(new Dropdown.OptionData("300"));
                    dataPointsOptions.Add(new Dropdown.OptionData("500"));
                }
                break;

            case "Oxbow Gaming Real Cash Voucher":
                {
                    dataPointsOptions.Add(new Dropdown.OptionData("100"));
                    dataPointsOptions.Add(new Dropdown.OptionData("300"));
                    dataPointsOptions.Add(new Dropdown.OptionData("500"));
                    dataPointsOptions.Add(new Dropdown.OptionData("1000"));
                    dataPointsOptions.Add(new Dropdown.OptionData("2000"));
                    dataPointsOptions.Add(new Dropdown.OptionData("3000"));
                }
                break;

            case "Samsung Memory Card":
                {
                    dataPointsOptions.Add(new Dropdown.OptionData("32GB"));
                    dataPointsOptions.Add(new Dropdown.OptionData("64GB"));
                    dataPointsOptions.Add(new Dropdown.OptionData("128GB"));
                }
                break;

            case "Sandisk Pen Drive":
                {
                    dataPointsOptions.Add(new Dropdown.OptionData("32GB"));
                    dataPointsOptions.Add(new Dropdown.OptionData("64GB"));
                    dataPointsOptions.Add(new Dropdown.OptionData("128GB"));
                }
                break;

            case "Earphones & headphones":
                {
                    dataPointsOptions.Add(new Dropdown.OptionData("Samsung level U Bluetooth"));
                    dataPointsOptions.Add(new Dropdown.OptionData("BOSE SOUNDSPORT IN-EAR"));
                    dataPointsOptions.Add(new Dropdown.OptionData("Motorola Pulse Max"));
                    dataPointsOptions.Add(new Dropdown.OptionData("Boss Sounsport Wirelles"));
                }
                break;

            case "Oxbow Gaming Specials":
                {
                    dataPointsOptions.Add(new Dropdown.OptionData("Oxbowgaming t-shirt"));
                    dataPointsOptions.Add(new Dropdown.OptionData("Oxbowgaming travel bag"));
                    dataPointsOptions.Add(new Dropdown.OptionData("Oxbowgaming Insta Laptop bag"));
                    dataPointsOptions.Add(new Dropdown.OptionData("Oxbowgaming Pen"));
                    dataPointsOptions.Add(new Dropdown.OptionData("Oxbowgaming Cap"));
                    dataPointsOptions.Add(new Dropdown.OptionData("Oxbowgaming Bottle"));
                    dataPointsOptions.Add(new Dropdown.OptionData("Oxbowgaming Mug"));
                }
                break;

            case "Smartphone & Tablet":
                {
                    dataPointsOptions.Add(new Dropdown.OptionData("Lenovo Tab4 10 Tablet"));
                    dataPointsOptions.Add(new Dropdown.OptionData("Apple ipad(6th Gen) Tablet"));
                    dataPointsOptions.Add(new Dropdown.OptionData("Galaxy J6"));
                    dataPointsOptions.Add(new Dropdown.OptionData("Galaxy A6"));
                    dataPointsOptions.Add(new Dropdown.OptionData("Nokia 3"));
                    dataPointsOptions.Add(new Dropdown.OptionData("Nokia 2"));
                    dataPointsOptions.Add(new Dropdown.OptionData("Champkia bluetooth watch"));
                }
                break;

            case "Speakers":
                {

                    dataPointsOptions.Add(new Dropdown.OptionData("Portrnics Sound cake portable"));
                    dataPointsOptions.Add(new Dropdown.OptionData("BOSE SOUNDLINK COLOR BT"));
                    dataPointsOptions.Add(new Dropdown.OptionData("Jbl flip4 waterproof speaker"));
                    dataPointsOptions.Add(new Dropdown.OptionData("Logitech Z120 Stereo"));
                    dataPointsOptions.Add(new Dropdown.OptionData("Portronics sublime"));
                }
                break;

            case "Powerbank":
                {

                    dataPointsOptions.Add(new Dropdown.OptionData("Lenovo Power Bank"));
                    dataPointsOptions.Add(new Dropdown.OptionData("Portronics Power Wallet"));
                    dataPointsOptions.Add(new Dropdown.OptionData("PORTRONICS TORK"));
                }
                break;

            case "Computer Accessories":
                {

                    dataPointsOptions.Add(new Dropdown.OptionData("Logitech Wireless Mouse"));
                    dataPointsOptions.Add(new Dropdown.OptionData("Logitech Wireless Keyboard"));
                    dataPointsOptions.Add(new Dropdown.OptionData("Toshiba 1TB external hard disk"));
                }
                break;

            case "PayTM":
                {
                    dataPointsOptions.Add(new Dropdown.OptionData("100"));
                    dataPointsOptions.Add(new Dropdown.OptionData("300"));
                    dataPointsOptions.Add(new Dropdown.OptionData("500"));
                    dataPointsOptions.Add(new Dropdown.OptionData("1000"));
                }
                break;
        }

        rewardPointsToRedeem.ClearOptions();
        rewardPointsToRedeem.AddOptions(dataPointsOptions);

        UpdatePointsNeeded();
    }
    class CastleDownloadHandler: DownloadHandlerScript {
		public delegate void Finished();
		public event Finished onFinished;

		protected override void CompleteContent ()
		{
			UnityEngine.Debug.Log("CompleteContent()");
			base.CompleteContent ();
			if (onFinished!= null) {
				onFinished();
			}
		}
	}

	protected static byte[] GetBytes(string str) {
		byte[] bytes = Encoding.UTF8.GetBytes(str);
		return bytes;
	}

    void RedeemTransaction()
    {
       // buttonChooseReward.interactable = true;

        WWW www;
        Hashtable postHeader = new Hashtable();
        postHeader.Add("Content-Type", "application/json");

        var form = new WWWForm();
        string callData = "{\"jwt\": \"" + GameManager.Instance.loginKeyValue
                  + "\",\"rewardDetails\":\"" + RewardDetails + "\", \"rewardValue\": \"" + RewardValue + "\"}";

        var formData = GetBytes(callData);
        www = new WWW(redeem_url, formData, postHeader);

        StartCoroutine(WaitForRequest(www));
    }

    IEnumerator WaitForRequest(WWW www)
    {
        yield return www;

        // check for errors
        if (www.error == null)
        {
            Debug.LogError("WWW Ok!: " + www.text);
            Redeem_Result rPoints = JsonUtility.FromJson<Redeem_Result>(www.text);
            GameManager.Instance.reward_points = rPoints.updatedRewardPoints;
            RewardResultObject.SetActive(true);
            TextRewardResult.text = rPoints.message;
            textAvailablePoints.text = Convert.ToDouble(GameManager.Instance.reward_points) + "";
        }
        else
        {
            Debug.LogError("WWW Error: " + www.error);
            RewardResultObject.SetActive(true);
            TextRewardResult.text = www.error;
        }
    }

    string redeem_url = "https://www.oxbowgaming.com/my-api/api/redeem_points.php";
    
    int availablePoints = Convert.ToInt32(Math.Floor(Convert.ToDouble(GameManager.Instance.reward_points)));

	void RewardClick() {
        //buttonChooseReward.interactable = false;
        if (availablePoints > int.Parse(RewardValue))
        {
            RedeemTransaction();
            //Debug.Log(result.MoveNext());
            
        }
        else
        {
            Debug.Log("You don't have sufficient points to redeem");
            // InitMenuScript.instance.redeemValidation.SetActive(true);
            InitMenuScript.instance.redeemValidation.SetActive(true);
           InitMenuScript.instance.redeemValidationText.text = "You don't have sufficient points to redeem";
          //  Vector3 endPos = new Vector3(InitMenuScript.instance.redeemValidation.transform.position.x, InitMenuScript.instance.redeemValidation.transform.position.y + 50f, InitMenuScript.instance.redeemValidation.transform.position.z);
           // StartCoroutine(MoveOverSpeed(InitMenuScript.instance.redeemValidation, endPos, 50f));
            Invoke("RedeemValidationShow", 6f);
            return;
        }
    }

    public IEnumerator MoveOverSpeed(GameObject objectToMove, Vector3 end, float speed)
    {
        Vector3 finalPos = objectToMove.transform.position;
        // speed should be 1 unit per second
        while (objectToMove.transform.position != end)
        {
            objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, end, speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        objectToMove.SetActive(false);
        //buttonChooseReward.interactable = true;

        objectToMove.transform.position = finalPos;
    }
    public void RedeemValidationShow()
    {
        InitMenuScript.instance.redeemValidation.SetActive(false);
        InitMenuScript.instance.redeemValidationText.text = string.Empty;
    }
}


[System.Serializable]
public class Redeem_Result
{
    public string message;
    public string updatedRewardPoints;
    //{"message":"Redemption successful.","updatedRewardPoints":18500}
}
