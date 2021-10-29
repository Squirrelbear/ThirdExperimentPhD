using UnityEngine;
using System.Collections;

public class MatchObjectBehaviour : MonoBehaviour {

    public GameObject[] matchObj;
    //private float[][] scaleBase = new float[][] { new float[]{ 4, 1.5f, 4 }, 
    //                                              new float[]{ 3.5f, 3.5f, 3.5f }, 
    //                                              new float[]{ 4, 4, 4 } };
    private float[] yIndex = new float[] { 28.5f, 30, 31.2f };

    public int curObj = 0;
    public int curSize = 0;
    public int curTexture = 0;

    private Material[] mats;
    private Color[] colours = new Color[] { Color.blue, Color.red, Color.green };
    private bool firstUpdate = true;

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
        if (firstUpdate)
        {
            //print("Doing update...");
            setObjectProperty(0, curObj);
            setObjectProperty(1, curSize);
            setObjectProperty(2, curTexture);
            firstUpdate = false;
        }

        /*if (Input.GetKeyDown(KeyCode.P))
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
        }*/
	}

    public void setAllProperties(int type, int size, int colour)
    {
        //print("Setting properties: " + type + ", " + size + ", " + colour);
        curObj = type;
        curSize = size;
        curTexture = colour;
        firstUpdate = true;
        //print("Doing property...");
    }

    public void setObjectProperty(int propertyID, int propertyValue)
    {
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
                mats[i].color = colours[curTexture];
            }
        }
        else
        {
            ErrorLog.logData("Error: Invalid setObjectProperty call. PropertyID is out of range. With data: " + propertyID + "," + propertyValue);
        }
    }
}
