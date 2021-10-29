using UnityEngine;
using System.Collections;

public class MatchSnapBehaviour : MonoBehaviour {

    public MatchObjectBehaviour matchObj = null;
    public MatchObjectSolBehaviour matchSolution = null;

    public bool lastMatchValidResult = false;
    public GameObject successStatusObject;

	// Use this for initialization
	void Start () {
        successStatusObject.GetComponent<MeshRenderer>().enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
        
	}

    public void setMatchObj(MatchObjectBehaviour matchObj)
    {
        /*if(this.matchObj != null && this.matchObj != matchObj)
        {
            print("Error point detected!!");
        }*/
        this.matchObj = matchObj;
    }

    public MatchObjectBehaviour getMatchObj()
    {
        return matchObj;
    }

    public bool matchValid()
    {
        bool result;

        if (matchSolution == null && matchObj == null)  // correctly left empty
        {
            result = true;
        }
        else if(matchSolution == null || matchObj == null) // incorrectly either in a wrong spot or missing a match
        {
            result = false;
        }
        else
        {
            // solution and match exist so compare them.
            result = matchObj.curObj == matchSolution.curObj && matchObj.curSize == matchSolution.curSize && matchObj.curTexture == matchSolution.curTexture;
        }

        if(result != lastMatchValidResult)
        {
            if (result && matchSolution != null)
            {
                successStatusObject.GetComponent<MeshRenderer>().enabled = true;

                successStatusObject.transform.position = getHoverPosition() ;
            }
            else
            {
                successStatusObject.GetComponent<MeshRenderer>().enabled = false;
            }
        }
        lastMatchValidResult = result;

        return result;
    }

    public Vector3 getHoverPosition()
    {
        Vector3 pos = successStatusObject.transform.position;
        int sizeAmount = (matchSolution != null && matchSolution.curSize > matchObj.curSize) ? matchSolution.curSize : matchObj.curSize;

        pos.y = transform.position.y + 6 + sizeAmount * 2f;
        return pos;
    }
}
