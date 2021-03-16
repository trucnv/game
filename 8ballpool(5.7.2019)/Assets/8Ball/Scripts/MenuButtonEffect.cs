using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButtonEffect : MonoBehaviour {
    public GameObject[] tables;
    public ScrollRect mumbaiScrollView;
    int lastID = 0;
    bool isChanging = false;

    private bool mumbaiClickOn = false;

    public void MumbaiPointEnter()
    {
        //Debug.Log(GameManager.Instance.tableNumber);
        tables[Global.number].transform.localScale = tables[Global.number].transform.localScale * 1.1f;
        mumbaiClickOn = true;
    }
    public void MumbaiPointExit()
    {
        mumbaiClickOn = false;
        tables[Global.number].transform.localScale = tables[Global.number].transform.localScale;
    }
	
	// Update is called once per frame
	void Update () {
        if (mumbaiClickOn == false)
        {
            //tables[lastID].transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void OnScrollValueChanged()
    {
        //Debug.Log("---Changed Time : " + lastChangedTime.ToString());
        float curXPos = mumbaiScrollView.transform.Find("GridWithElements").GetComponent<RectTransform>().localPosition.x;
        int id = Mathf.Abs((int)((curXPos + 392) / 220));

        id += 1;

        if (curXPos > -392)
            id = 0;

        if (id < 0)
            id = 0;        

        if (id >= 7)
            id = 7;
        if (id != lastID)
            tables[lastID].transform.localScale = new Vector3(1,1,1);
        tables[id].transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        lastID = id;
    }
}
