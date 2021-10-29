using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TowerStatusDialog : MonoBehaviour {

    public enum TowerStatusDialogType { Selection, Selected };

    public TowerStatusDialogType dialogType;
    public bool tooltipVisible = false;
    public GameObject titleObject;
    public GameObject titleDurability;
    public GameObject titleSelecting;
    public GameObject panel;

    // cached references
    public Text tooltipTitleText;
    public Text tooltipDuraText;
    public Text tooltipSelectingText;
    public Camera cam;
    public CameraBehaviour camRef;
    public TowerBehaviour cachedTowerRef;
    public Image imageScript;

    public Button btnDeselect, btnRepair, btnMove;
    public Text txtRepair, txtMove;
    public Button[] buttonList;
    public LevelManager levelManager;

    public GameObject selectionTarget;
    public float timeToSelect = 1.5f;
    public float selectionTimeProgress = 0;
    public GameObject cursorObject;
    public Reticle cursor;

	// Use this for initialization
	void Start () {
        if (titleObject != null)
        {
            tooltipTitleText = titleObject.GetComponent<Text>();
        }
        if (titleDurability != null)
        {
            tooltipDuraText = titleDurability.GetComponent<Text>();
        }
        if (titleSelecting != null)
        {
            tooltipSelectingText = titleSelecting.GetComponent<Text>();
        }
        cam = Camera.main;
        camRef = cam.GetComponent<CameraBehaviour>();
        imageScript = panel.GetComponent<Image>();
        hide();
	}
	
	// Update is called once per frame
	public void update (float deltaTime) {
        if (dialogType == TowerStatusDialogType.Selected)
        {
            if (camRef.selectedTower == null || camRef.selectedTower.towerState == TowerBehaviour.TowerState.TowerPlacement)
            {
                hide();
                return;
            }
            else
            {
                show();
            }

            if (camRef.selectedTower != cachedTowerRef)
            {
                cachedTowerRef = camRef.selectedTower;
                updateTowerTitle();
            }
            updateButtons(deltaTime);
        }
        else if (dialogType == TowerStatusDialogType.Selection)
        {
            if (camRef.selectingObject == null 
                || camRef.selectingObject == camRef.selectedObject 
                || camRef.selectingObject.GetComponent<TowerBehaviour>() == null)
            {
                hide();
                return;
            }
            else
            {
                show();
            }

            cachedTowerRef = camRef.selectingObject.GetComponent<TowerBehaviour>();
            tooltipSelectingText.text = (int)((camRef.selectionProgress / camRef.timeToSelect) * 100) + "%";
        }

        if (cam != null)
        {
            transform.rotation = Quaternion.LookRotation(cam.transform.position - transform.position) * Quaternion.Euler(0, 180, 0);
        }

        if (cachedTowerRef != null)
        {
            transform.position = cachedTowerRef.transform.position + new Vector3(0, 5, 0);
            float durability = cachedTowerRef.durability;
            tooltipDuraText.text = (int)(durability*100) + "%";
            if (durability == 0)
            {
                imageScript.color = (dialogType == TowerStatusDialogType.Selected) ? Color.yellow : Color.red;
            }
            else
            {
                imageScript.color = (dialogType == TowerStatusDialogType.Selected) ? Color.green : Color.white;
            }
        }
	}

    private void updateTowerTitle()
    {
        if(cachedTowerRef != null)
        {
            switch(cachedTowerRef.towerType)
            {
                case TowerBehaviour.TowerType.Basic:
                    tooltipTitleText.text = "Basic Tower";
                    break;
                case TowerBehaviour.TowerType.Frost:
                    tooltipTitleText.text = "Frost Tower";
                    break;
                case TowerBehaviour.TowerType.Swarm:
                    tooltipTitleText.text = "Swarm Tower";
                    break;
                case TowerBehaviour.TowerType.Explosive:
                    tooltipTitleText.text = "Explosive Tower";
                    break;
            }
        }
    }

    private void updateButtons(float deltaTime)
    {
        if(cachedTowerRef != null)
        {
            int repairCost = levelManager.getTowerRepairCost(cachedTowerRef.towerType, cachedTowerRef.durability);
            if(repairCost == 0 || repairCost > levelManager.currency)
            {
                btnRepair.GetComponent<CanvasRenderer>().SetAlpha(0);
                txtRepair.GetComponent<CanvasRenderer>().SetAlpha(0);
                btnRepair.interactable = false;
            }
            else
            {
                btnRepair.GetComponent<CanvasRenderer>().SetAlpha(100);
                txtRepair.GetComponent<CanvasRenderer>().SetAlpha(100);
                txtRepair.text = "Repair\n" + repairCost;
                btnRepair.interactable = true;
            }

            bool showMove = (int)(levelManager.levelState) % 2 == 1;
            float moveAlpha = (showMove) ? 100 : 0;
            btnMove.GetComponent<CanvasRenderer>().SetAlpha(moveAlpha);
            txtMove.GetComponent<CanvasRenderer>().SetAlpha(moveAlpha);
            btnMove.interactable = showMove;

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
                foreach(Button button in buttonList)
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
                            if ((camRef.cursorMode == CameraBehaviour.CursorMode.LookAt && selectionTimeProgress >= timeToSelect)
                                || (camRef.cursorMode != CameraBehaviour.CursorMode.LookAt && camRef.clickOccured))//Time.timeSinceLevelLoad >= selectionTimeProgress)
                            {
                                //print("WARNING NEED TO ADD TRACKING HERE!!");
                                if (button.name == btnDeselect.name)
                                {
                                    ErrorLog.logData("Deselecting tower.", false);
                                    camRef.forceDeselectImmediately();
                                    levelManager.loggingStats[7]++;
                                }
                                else if(button.name == btnMove.name)
                                {
                                    ErrorLog.logData("Moving tower.", false);
                                    camRef.moveSelectedTower();
                                    levelManager.loggingStats[6]++;
                                }
                                else if(button.name == btnRepair.name)
                                {
                                    ErrorLog.logData("Repairing tower.", false);
                                    levelManager.repairSelectedTower();
                                    levelManager.loggingStats[5]++;
                                }

                                
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
                }
                if (targetFound)
                {
                    break;
                }
            }

            if (!targetFound)
            {
                selectionTarget = null;
            }
        }
    }

    public void hide()
    {
        tooltipVisible = false;
        gameObject.GetComponent<Canvas>().enabled = false;
    }

    public void show()
    {
        tooltipVisible = true;
        gameObject.GetComponent<Canvas>().enabled = true;
    }
}
