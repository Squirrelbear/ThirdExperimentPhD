using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TwoStepBehaviour : MonoBehaviour {

    public string tempMenuDef;
    public Button showMenuBtn;

    public bool menuActive = false;
    public MenuBehaviour menuToReveal;
    private Camera cam;

    public float menuActiveUptime = 0;
    public float timeSinceLastInteract = 0;
    public float timeToHideMenu = 8;

    public ExtraDataRecorder.ExtraDataCollection log;

	// Use this for initialization
	void Start () {
        cam = Camera.main;
        log = ExtraDataRecorder.getSingleton().getDataCollection("TwoStepMenuBehaviourLog");
        log.logData("Timestamp,GameStateSuper,GameState,Menu,MenuUptime,MenuHideReason", false);
	}
	
	// Update is called once per frame
	public void update (float deltaTime) 
    {
        if (!menuActive)
        {
            gameObject.GetComponent<Canvas>().enabled = false;
            return;
        }
        else if (cam != null)
        {
            transform.rotation = Quaternion.LookRotation(cam.transform.position - transform.position) * Quaternion.Euler(0, 180, 0);
        }

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
        menuActiveUptime += deltaTime;

        // check any hits to see if any of them are blocking UI elements
        foreach (RaycastResult r in hits)
        {
            if (r.gameObject.name == showMenuBtn.name)
            {
                targetFound = true;

                menuToReveal.showMenu(tempMenuDef);
                menuActive = false;
                //print("log interaction event with two step");
                log.logData(menuToReveal.experimentState.gameSuperState + "," + menuToReveal.experimentState.gameState + "," + tempMenuDef + "," + menuActiveUptime + ",ButtonEvent", true);

                break;
            }
        }

        if (targetFound)
        {
            timeSinceLastInteract = 0;
        }
        else
        {
            timeSinceLastInteract += deltaTime;
            if (timeSinceLastInteract >= timeToHideMenu)
            {
                timeSinceLastInteract = 0;
                menuActive = false;
                log.logData(menuToReveal.experimentState.gameSuperState + "," + menuToReveal.experimentState.gameState + "," + tempMenuDef + "," + menuActiveUptime + ",HideFromInactivity", true);
            }
        }
	}

    public void show(string menuDef)
    {
        //print("Revealing menu");
        tempMenuDef = menuDef;

        gameObject.GetComponent<Canvas>().enabled = true;
        menuActive = true;
        // in case the menu was already active reset the hide timer
        timeSinceLastInteract = 0;
        menuActiveUptime = 0;
    }
}
