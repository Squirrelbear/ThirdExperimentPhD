using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuBehaviour : MonoBehaviour {
    
    //int i = 0;

    public bool menuActive = false;
    public bool mouseDebugMode = true;

    public GameObject selectionTarget;
    public float timeToSelect = 1.5f;
    public float timeToHideMenu = 8f;
    public float selectionTimeProgress = 0;
    public float timeSinceLastInteract = 0;
    public int menuAction = 0;

    public GameObject titleObject;
    public GameObject[] buttonObjects;
        
    // cached references
    public Text menuTitle;
    public Button[] buttons;
    public Text[] buttonTexts;
    public int[] buttonResultCode;

    //string exampleMenuDef = "Some Title,First,1,Second,2,Third,3,Fourth,4";

    private Camera cam;
    private CameraBehaviour camRef;

    public GameObject cursorObject;
    public Reticle cursor;

    // Data logging
    public ExtraDataRecorder extraDataRecorder;
    public ExtraDataRecorder.ExtraDataCollection menuInteractionLog;
    public float menuActiveUptime = 0;
    public string curMenuDef = null;
    public ExperimentState experimentState;

    public GameObject otherMenu;

    public bool enableAutoHide = true;
    public bool logAsPeriphery = false;

    public bool twoStepMenuEnabled = false;
    public TwoStepBehaviour twoStepObject;

	// Use this for initialization
	void Start () {
        buttons = new Button[buttonObjects.Length];
        buttonTexts = new Text[buttonObjects.Length];
        buttonResultCode = new int[buttonObjects.Length];
        for (int i = 0; i < buttonObjects.Length; i++)
        {
            buttonTexts[i] = buttonObjects[i].GetComponentInChildren<Text>();
            buttons[i] = buttonObjects[i].GetComponent<Button>();
            
            if(buttonObjects[i].name == "ButtonClose")
            {
                buttonResultCode[i] = -5;
            }
            else
            {
                buttonResultCode[i] = -1;
            }
        }
        menuTitle = titleObject.GetComponent<Text>();

        cam = Camera.main;
        camRef = cam.GetComponent<CameraBehaviour>();
        cursor = cursorObject.GetComponent<Reticle>();

        extraDataRecorder = ExtraDataRecorder.getSingleton();
        if (logAsPeriphery)
        {
            menuInteractionLog = extraDataRecorder.getDataCollection("PeripheryMenuBehaviourLog");
        }
        else
        {
            menuInteractionLog = extraDataRecorder.getDataCollection("CircularMenuBehaviourLog");
        }
        menuInteractionLog.logData("Timestamp,GameStateSuper,GameState,Menu,MenuUptime,MenuHideReason,ButtonName,ButtonID,ButtonResultCode", false);
        experimentState = cam.GetComponent<ExperimentState>();
	}

    // Update is called once per frame
    public void update(float deltaTime)
    {
        if(twoStepObject != null)
        {
            twoStepObject.update(deltaTime);
        }

        menuAction = 0;
        if (!menuActive)
        {
            gameObject.GetComponent<Canvas>().enabled = false;
            return;
        }
        else if(cam != null)
        {
            transform.rotation = Quaternion.LookRotation(cam.transform.position - transform.position) * Quaternion.Euler(0, 180, 0);
        }

        // get pointer event data, then set current mouse position
        PointerEventData ped = new PointerEventData(EventSystem.current);
        mouseDebugMode = cam.GetComponent<CameraBehaviour>().clickDebugMode;
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
            for (int i = 0; i < buttonObjects.Length; i++)
            {
                if (r.gameObject.name == buttonObjects[i].name)
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
                        if(enableAutoHide && selectionTarget.name == "ButtonClose")
                        {
                            // apply double time to close button
                            selectionTimeProgress += deltaTime;
                        }
                        if ((camRef.cursorMode == CameraBehaviour.CursorMode.LookAt && selectionTimeProgress >= timeToSelect)
                            || (camRef.cursorMode != CameraBehaviour.CursorMode.LookAt && camRef.clickOccured))//Time.timeSinceLevelLoad >= selectionTimeProgress)
                        {
                            /*if (!r.gameObject.name.Equals("ButtonClose"))
                            {
                                menuAction = buttonResultCode[i];
                            }*/
                            menuAction = buttonResultCode[i];
                            menuActive = false;
                            menuInteractionLog.logData(experimentState.gameSuperState + "," + experimentState.gameState + "," + curMenuDef + "," + menuActiveUptime + ",ButtonEvent," + buttonTexts[i].text + "," + i + "," + menuAction, true);

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
            }

            if(targetFound) 
            {
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
            if (timeSinceLastInteract >= timeToHideMenu && enableAutoHide)
            {
                timeSinceLastInteract = 0;
                menuActive = false;
                menuInteractionLog.logData(experimentState.gameSuperState + "," + experimentState.gameState + "," + curMenuDef + "," + menuActiveUptime + ",HideFromInactivity,-1,-1,0", true);
            }
        }
    }

    public void applyMenu(string menuDef)
    {
        string[] splitData = menuDef.Split(',');
        
        menuTitle.text = splitData[0];
        for (int i = 0; i < buttonTexts.Length-1; i++)
        {
            buttonTexts[i].text = splitData[1 + i * 2];
            buttonResultCode[i] = int.Parse(splitData[2 + i * 2]);
        }
    }

    public void showMenu(string menuDef, bool overrideTwoStep = true)
    {
        if(otherMenu != null)
        {
            MenuBehaviour m = otherMenu.GetComponent<MenuBehaviour>();
            if(m.menuActive)
            {
                m.menuActive = false;
                m.menuInteractionLog.logData(experimentState.gameSuperState + "," + experimentState.gameState + "," + m.curMenuDef + "," + m.menuActiveUptime + ",HideFromRevealOther,-1,-1,0", true);
            }
            else if(m.twoStepObject != null && m.twoStepObject.menuActive)
            {
                m.twoStepObject.menuActive = false;
                m.twoStepObject.log.logData(experimentState.gameSuperState + "," + experimentState.gameState + "," + m.twoStepObject.tempMenuDef + "," + m.twoStepObject.menuActiveUptime + ",HideFromRevealOther", true);
            }
        }

        // menu was already open and is being overwritten
        if (menuActive)
        {
            menuInteractionLog.logData(experimentState.gameSuperState + "," + experimentState.gameState + "," + curMenuDef + "," + menuActiveUptime + ",ReplacedWhileActive,-1,-1,0", true);
        }

        if(twoStepMenuEnabled && !overrideTwoStep)
        {
            menuActive = false;
            //print("Before: " + twoStepObject.transform.position);
            twoStepObject.transform.position = transform.position;
            //print("After: " + twoStepObject.transform.position);
            twoStepObject.show(menuDef);
            return;
        }

        gameObject.GetComponent<Canvas>().enabled = true;
        applyMenu(menuDef);
        menuActive = true;
        selectionTarget = null;
        selectionTimeProgress = 0;
        // in case the menu was already active reset the hide timer
        timeSinceLastInteract = 0;
        
        // logging variables
        curMenuDef = menuDef.Replace(",",";");
        menuActiveUptime = 0;
    }
}
