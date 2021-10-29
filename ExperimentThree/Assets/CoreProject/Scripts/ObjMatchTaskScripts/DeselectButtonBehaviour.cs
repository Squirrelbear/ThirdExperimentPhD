using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DeselectButtonBehaviour : MonoBehaviour {

    public GameObject button;
    public MatchObjectManager matchObjManager;

    // cached references
    public Camera cam;
    public CameraBehaviour camRef;
    public MatchSnapBehaviour snapCache = null;

    public GameObject selectionTarget;
    public float timeToSelect = 1.5f;
    public float selectionTimeProgress = 0;
    public GameObject cursorObject;
    public Reticle cursor;

	// Use this for initialization
	void Start () {
        cam = Camera.main;
        camRef = cam.GetComponent<CameraBehaviour>();
        hide();
	}
	
	// Update is called once per frame
	public void update (float deltaTime) {
        if(matchObjManager.selectedMatchObj == null || matchObjManager.allowMoveObj )
        {
            snapCache = null;
            hide();
            //print("hiding" + matchObjManager.selectedMatchObj + " " + matchObjManager.allowMoveObj);
            return;
        }
        else
        {
            //print("showing");
            show();
        }

        if(snapCache == null || matchObjManager.selectedMatchObj != snapCache.getMatchObj())
        {
            foreach(MatchSnapBehaviour snap in matchObjManager.snapNodes)
            {
                if(snap.getMatchObj() == matchObjManager.selectedMatchObj)
                {
                    snapCache = snap;
                    break;
                }
            }
        }

        transform.rotation = Quaternion.LookRotation(cam.transform.position - transform.position) * Quaternion.Euler(0, 180, 0);
        transform.position = snapCache.getHoverPosition();
        //print(transform.position);

        // get pointer event data, then set current mouse position
        PointerEventData ped = new PointerEventData(EventSystem.current);
        bool mouseDebugMode = cam.GetComponent<CameraBehaviour>().clickDebugMode;
        if (mouseDebugMode)
        {
            ped.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }
        else
        {
            ped.position = new Vector2(Screen.width / 2, Screen.height / 2);
        }

        // create an empty list of raycast results
        List<RaycastResult> hits = new List<RaycastResult>();

        // ray cast into UI and check for hits
        EventSystem.current.RaycastAll(ped, hits);

        bool targetFound = false;

        // check any hits to see if any of them are blocking UI elements
        foreach (RaycastResult r in hits)
        {
            if (r.gameObject.name == button.name)
            {
                targetFound = true;

                if (selectionTarget != r.gameObject)
                {
                    selectionTarget = r.gameObject;
                    selectionTimeProgress = 0;// Time.timeSinceLevelLoad + timeToSelect;
                }
                else
                {
                    selectionTimeProgress += deltaTime;
                    if (selectionTimeProgress >= timeToSelect
                        || (camRef.cursorMode != CameraBehaviour.CursorMode.LookAt && camRef.clickOccured))//Time.timeSinceLevelLoad >= selectionTimeProgress)
                    {
                        ErrorLog.logData("Deselecting Match Object.", false);
                        matchObjManager.deSelect();
                        //menuInteractionLog.logData(experimentState.gameSuperState + "," + experimentState.gameState + "," + curMenuDef + "," + menuActiveUptime + ",ButtonEvent," + buttonTexts[i].text + "," + i + "," + menuAction, true);

                        //menuTitle.text = ""+buttonResultCode[i];//"Selection Complete";
                        //applyMenu(exampleMenuDef);
                    }
                    else
                    {
                        //float percentRemaining = 100 - (selectionEndTime - Time.timeSinceLevelLoad) / timeToSelect * 100;
                        //float percentRemaining = selectionTimeProgress / timeToSelect * 100;
                        //menuTitle.text = r.gameObject.name + " " + percentRemaining + "%";
                        cursor.updateCursor(selectionTimeProgress / timeToSelect);
                    }
                }

                break;
            }

            if(targetFound)
            {
                break;
            }
        }

        if (!targetFound)
        {
            selectionTarget = null;
        }
	}

    public void hide()
    {
        gameObject.GetComponent<Canvas>().enabled = false;
    }

    public void show()
    {
        gameObject.GetComponent<Canvas>().enabled = true;
    }
}
