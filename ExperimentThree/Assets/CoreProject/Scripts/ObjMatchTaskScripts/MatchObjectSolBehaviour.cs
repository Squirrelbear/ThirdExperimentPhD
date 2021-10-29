using UnityEngine;
using System.Collections;
using System;

public class MatchObjectSolBehaviour : MonoBehaviour {
    public GameObject[] matchObj;
    private float[] yIndex = new float[] { 28.5f, 30, 31.2f };

    public int curObj = 0;
    public int curSize = 0;
    public int curTexture = 0;

    private Material[] mats;
    private Color[] colours = new Color[] { Color.blue, Color.red, Color.green };

    private bool increasing = false;
    private bool firstUpdate = true;

    public MatchObjectBehaviour matchedObject = null;

	// Use this for initialization
	void Start () {
        mats = new Material[matchObj.Length];
	    for(int i = 0; i < matchObj.Length; i++)
        {
            MeshRenderer renderer = matchObj[i].GetComponentInChildren<MeshRenderer>();
            mats[i] = renderer.material;
            matchObj[i].SetActive(false);
        }

        /*setObjectProperty(0, 0);
        setObjectProperty(1, 0);
        setObjectProperty(2, 0);*/
	}
	
	// Update is called once per frame
	void Update () {
        if(firstUpdate)
        {
            setObjectProperty(0, curObj);
            setObjectProperty(1, curSize);
            setObjectProperty(2, curTexture);
            firstUpdate = false;
        }

        cycleAlpha();

        if (Input.GetKeyDown(KeyCode.P))
        {
            int newObj = (curObj + 1) % 3;
            setObjectProperty(0, newObj);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            int newSize = (curSize+1) % 3;
            setObjectProperty(1, newSize);
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            int newTex = (curTexture + 1) % 3;
            setObjectProperty(2, newTex);
        }
	}

    public void setAllProperties(int type, int size, int colour)
    {
        //print("Setting properties: " + type + ", " + size + ", " + colour);
        curObj = type;
        curSize = size;
        curTexture = colour;
        firstUpdate = true;
    }

    public void setObjectProperty(int propertyID, int propertyValue)
    {
        //print("Setting property: " + propertyID + "," + propertyValue);
        if(propertyValue < 0 || propertyValue > 2)
        {
            ErrorLog.logData("Error: Invalid setObjectProperty call. PropertyValue is out of range. With data: " + propertyID + "," + propertyValue);
            return;
        }

        if (propertyID == 0) // shape
        {
            if (curObj != propertyValue)
            {
                matchObj[curObj].SetActive(false);
            }
            curObj = propertyValue;
            matchObj[curObj].SetActive(true);
            //print("Revealing: " + curObj);
        }
        else if(propertyID == 1) // size
        {
            curSize = propertyValue;
            transform.localScale = new Vector3(curSize + 1, curSize + 1, curSize + 1);
            transform.position = new Vector3(transform.position.x, yIndex[curSize], transform.position.z);
        }
        else if(propertyID == 2) // colour
        {
            curTexture = propertyValue;
            for (int i = 0; i < matchObj.Length; i++)
            {
                //print(i);
                //try
                //{
                    mats[i].color = colours[curTexture];
                /*} 
                catch(Exception e)
                {
                    print(i);
                    print(e.Message);
                }*/
            }
        }
        else
        {
            ErrorLog.logData("Error: Invalid setObjectProperty call. PropertyID is out of range. With data: " + propertyID + "," + propertyValue);
        }
    }

    private void cycleAlpha()
    {
        Color newColor2 = mats[curObj].color;
        if (increasing)
        {
            newColor2.a += Time.deltaTime / 3;
        }
        else
        {
            newColor2.a -= Time.deltaTime / 3;
        }

        if (newColor2.a < 0.2)
        {
            newColor2.a = 0.2f;
            increasing = true;
        }
        else if (newColor2.a > 0.6)
        {
            newColor2.a = 0.6f;
            increasing = false;
        }
        mats[curObj].color = newColor2;
    }
}