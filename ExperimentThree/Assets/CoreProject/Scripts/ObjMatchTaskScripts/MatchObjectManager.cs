using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MatchObjectManager : MonoBehaviour {

    public List<MatchObjectSolBehaviour> solutionObjects;
    public List<MatchObjectBehaviour> matchObjects;

    public GameObject snapNodesParent;
    public List<MatchSnapBehaviour> snapNodes;

    public GameObject solutionPrefab;
    public GameObject userObjectPrefab;

    private CameraBehaviour camRef;

    public MatchObjectBehaviour selectedMatchObj = null;
    public DeselectButtonBehaviour deselectButton;

    //public TowerSnapBehaviour[] towerSnapPoints;
    public bool isObjSnapped = false;
    public bool allowMoveObj = false;
    public MatchSnapBehaviour tempSnapCache = null;

    private bool firstUpdate = true;
    private bool secondUpdate = true;

    public bool circularOnSelect = false;

    public int[] minimumOperationCount; // spawnCylinder, spawnCube, spawnSphere, setSmall, setMed, setLarge, setBlue, setRed, setGreen
    public int[] operationCount; // spawnCylinder, spawnCube, spawnSphere, setSmall, setMed, setLarge, setBlue, setRed, setGreen, setCylinder, setCube, setSphere
    public int[] menuOperationCount; // Deselect, Back, Open Shape Menu, Open Size Menu, Open Colour Menu, Move Object

    private ExperimentState.GameState lastState = ExperimentState.GameState.MainMenu;
    public GameObject introObject;

	// Use this for initialization
	void Start () {
        snapNodes = new List<MatchSnapBehaviour>();
        foreach (Transform c in snapNodesParent.transform)
        {
            snapNodes.Add(c.GetComponent<MatchSnapBehaviour>());
        }
        solutionObjects = new List<MatchObjectSolBehaviour>();
        matchObjects = new List<MatchObjectBehaviour>();
        camRef = Camera.main.GetComponent<CameraBehaviour>();
	}
	
	// Update is called once per frame
	public void update (float deltaTime) {
        if(firstUpdate && camRef.experimentState.gameState == ExperimentState.GameState.ObjectManipPerph_Test)
        {
            spawnSolutionNodes(5);
            menuOperationCount = new int[] { 0,0,0,0,0,0 };
            firstUpdate = false;
        }
        else if (secondUpdate && camRef.experimentState.gameState == ExperimentState.GameState.ObjectManipCirc_Test)
        {
            spawnSolutionNodes(5);
            menuOperationCount = new int[] { 0, 0, 0, 0, 0, 0 };
            secondUpdate = false;
            circularOnSelect = true;
        }

        if (camRef.experimentState.gameState == ExperimentState.GameState.ObjectManipPerph_Test
            || camRef.experimentState.gameState == ExperimentState.GameState.ObjectManipCirc_Test)
        {
            //debugKeyboardInput();

            if (circularOnSelect)
            {
                updateCircularMenuInput();
            }
            updateMenuInput();
            updateObjectInteraction(deltaTime);
            
            bool winCondition = testWinCondition();
            if (winCondition)
            {
                //print("Success! Victory! ETC!.");
                saveAllTaskData();
                resetAllNodes();
                camRef.experimentState.nextState();
            }
        }

        if (camRef.experimentState.gameState != lastState && camRef.experimentState.gameState == ExperimentState.GameState.ObjectManipPerph_Intro3)
        {
            introObject.SetActive(true);
            //print("revealing");
        }
        else if (camRef.experimentState.gameState != lastState && camRef.experimentState.gameState == ExperimentState.GameState.ObjectManipPerph_Test)
        {
            introObject.SetActive(false);
        }
        lastState = camRef.experimentState.gameState;
	}

    public void debugKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            resetAllNodes();
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            spawnSolutionNodes(5);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            spawnMatchObject(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            spawnMatchObject(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            spawnMatchObject(2);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            moveSelectedObject();
        }
    }

    public bool testWinCondition()
    {
        bool result = true;
        foreach(MatchSnapBehaviour s in snapNodes)
        {
            if (!s.matchValid())
            {
                result = false;
            }
            else if (s.getMatchObj() != null && s.getMatchObj() == selectedMatchObj)
            {
                deSelect();
            }
        }
        return result;
    }

    public void updateObjectInteraction(float deltaTime)
    {
        if (attemptSelection(deltaTime))
        {
            // do nothing
        }
        else if ((camRef.clickDebugMode || camRef.cursorMode != CameraBehaviour.CursorMode.LookAt) && isObjSnapped && camRef.clickOccured && !camRef.clickUsed)
        {
            // Debug only deselection. Places tower back down on a snap with a click
            deSelect();
        }
        else if (attemptDeSelect(deltaTime))
        {
            // do nothing
        }
        else
        {
            updateSelected();
        }

        if (!circularOnSelect)
        {
            deselectButton.update(deltaTime);
        }
    }

    public void spawnMatchObject(int objType)
    {
        // Check for invalid object types, if there is already a selected object that is currently moving
        // And if there are too many objects already created.
        if (objType < 0 || objType >= 3 || (selectedMatchObj != null && allowMoveObj)
            || matchObjects.Count >= solutionObjects.Count)
        {
            return;
        }
        GameObject obj = (GameObject)Instantiate(userObjectPrefab, new Vector3(0, 1.59f, 0), Quaternion.identity);
        MatchObjectBehaviour script = obj.GetComponent<MatchObjectBehaviour>();
        matchObjects.Add(script);
        script.setAllProperties(objType, 0, 0);
        //levelManager.addTower(tower);
        selectObj(obj, false);
        moveSelectedObject();
    }

    public void moveSelectedObject()
    {
        if (selectedMatchObj != null)
        {
            allowMoveObj = true;
            camRef.peripheryBehaviour.leftMenuDef = "";
            camRef.peripheryBehaviour.rightMenuDef = "";
            foreach (MatchSnapBehaviour snapPoint in snapNodes)
            {
                if (snapPoint.getMatchObj() == selectedMatchObj)
                {
                    snapPoint.setMatchObj(null);
                }
            }
        }
    }

    public void updateMenuInput()
    {
        int action = camRef.peripheryBehaviour.menuBehaviour.menuAction;
        if (action >= 9001 && action < 9004)
        {
            action = action % 10 - 1;
            spawnMatchObject(action);
            operationCount[action]++;
        }
        else if((action >= 9011 && action < 9014)
                || (action >= 9021 && action < 9024)
                || (action >= 9031 && action < 9034))
        {
            action -= 9011;
            modifySelectedObject(action);
            camRef.peripheryBehaviour.leftMenuDef = ExperimentState.OBJECTMODEDITMENU;
            camRef.peripheryBehaviour.rightMenuDef = ExperimentState.OBJECTMODEDITMENU;
        }
        else if(action == 9040)
        {
            deSelect();
            menuOperationCount[0]++;
        }
        else if(action == 9010)
        {
            // spawn menu type selection
            camRef.peripheryBehaviour.menuBehaviour.showMenu(ExperimentState.OBJECTMODTYPEMENU);
            menuOperationCount[2]++;
        }
        else if(action == 9020)
        {
            // spawn menu size selection
            camRef.peripheryBehaviour.menuBehaviour.showMenu(ExperimentState.OBJECTMODSIZEMENU);
            menuOperationCount[3]++;
        }
        else if(action == 9030)
        {
            // spawn menu colour selection
            camRef.peripheryBehaviour.menuBehaviour.showMenu(ExperimentState.OBJECTMODCOLOURMENU);
            menuOperationCount[4]++;
        }
        else if(action == 9050)
        {
            moveSelectedObject();
            menuOperationCount[5]++;
        }
        else if(action == 9000)
        {
            // return to original menu
            camRef.peripheryBehaviour.menuBehaviour.showMenu(ExperimentState.OBJECTMODEDITMENU);
            menuOperationCount[1]++;
        }
    }

    public void updateCircularMenuInput()
    {
        if(camRef.circularMenu.menuActive)
        {
            MatchSnapBehaviour curSnap = null;
            foreach (MatchSnapBehaviour snap in snapNodes)
            {
                if (snap.getMatchObj() == selectedMatchObj)
                {
                    curSnap = snap;
                    break;
                }
            }

            if (curSnap != null)
            {
                camRef.circularMenu.transform.position = curSnap.getHoverPosition();
            }
        }

        int action = camRef.circularMenu.menuAction;
        if (action >= 9001 && action < 9004)
        {
            if(selectedMatchObj != null)
            {
                deSelect();
            }
            action = action % 10 - 1;
            spawnMatchObject(action);
            operationCount[action]++;
        }
        else if ((action >= 9011 && action < 9014)
                || (action >= 9021 && action < 9024)
                || (action >= 9031 && action < 9034))
        {
            action -= 9011;
            modifySelectedObject(action);
            camRef.circularMenu.showMenu(ExperimentState.OBJECTMODEDITMENU);
            //camRef.peripheryBehaviour.leftMenuDef = ExperimentState.OBJECTMODEDITMENU;
            //camRef.peripheryBehaviour.rightMenuDef = ExperimentState.OBJECTMODEDITMENU;
        }
        else if (action == 9040 || action == -5) // -5 is the default for a button named "ButtonClose"
        {
            deSelect();
            menuOperationCount[0]++;
        }
        else if (action == 9010)
        {
            // spawn menu type selection
            camRef.circularMenu.showMenu(ExperimentState.OBJECTMODTYPEMENU);
            menuOperationCount[2]++;
        }
        else if (action == 9020)
        {
            // spawn menu size selection
            camRef.circularMenu.showMenu(ExperimentState.OBJECTMODSIZEMENU);
            menuOperationCount[3]++;
        }
        else if (action == 9030)
        {
            // spawn menu colour selection
            camRef.circularMenu.showMenu(ExperimentState.OBJECTMODCOLOURMENU);
            menuOperationCount[4]++;
        }
        else if (action == 9050)
        {
            moveSelectedObject();
            menuOperationCount[5]++;
        }
        else if (action == 9000)
        {
            // return to original menu
            camRef.circularMenu.showMenu(ExperimentState.OBJECTMODEDITMENU);
            menuOperationCount[1]++;
        }
    }

    public void modifySelectedObject(int editCode)
    {
        // Eg, 01 = curObj/Cube
        int a = editCode / 10;
        int b = editCode - (a * 10);
        
        if(selectedMatchObj != null)
        {
            selectedMatchObj.setObjectProperty(a, b);

            if (a == 0)
            {
                operationCount[9 + b]++;
            }
            else if(a == 1)
            {
                operationCount[3 + b]++;
            }
            else if (a == 2)
            {
                operationCount[6 + b]++;
            }
        }
    }

    public void resetAllNodes()
    {
        deselectButton.hide();
        foreach(MatchObjectSolBehaviour s in solutionObjects)
        {
            Destroy(s.gameObject);
        }
        solutionObjects.Clear();

        foreach (MatchObjectBehaviour s in matchObjects)
        {
            Destroy(s.gameObject);
        }
        matchObjects.Clear();

        foreach(MatchSnapBehaviour s in snapNodes)
        {
            s.matchSolution = null;
            s.setMatchObj(null);
            s.successStatusObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public void spawnSolutionNodes(int count)
    {
        resetAllNodes();

        minimumOperationCount = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        operationCount = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        for (int i = 0; i < count; i++)
        {
            MatchSnapBehaviour snapPoint = snapNodes[Random.Range(0, snapNodes.Count)];
            Transform target = snapPoint.transform;
            bool targetUnique = true;
            int iterationCounter = 0;
            do
            {
                targetUnique = true;
                foreach(MatchObjectSolBehaviour t in solutionObjects)
                {
                    float d = Vector3.Distance(t.transform.position, target.position);
                    if (d < 15)
                    {
                        targetUnique = false;
                        snapPoint = snapNodes[Random.Range(0, snapNodes.Count)];
                        target = snapPoint.transform;
                        break;
                    }
                }
                iterationCounter++;
                if(iterationCounter >= 40)
                {
                    ErrorLog.logData("Critical error: No valid location found to place next match solution object! Resetting and retrying.", true);
                    resetAllNodes();
                    spawnSolutionNodes(count);
                    return;
                }
            } while (!targetUnique);

            int var1 = Random.Range(0, 3); // shape
            int var3 = solutionObjects.Count; // color
            if (var3 > 2)
            {
                var3 = Random.Range(0, 3);
            }
            int var2 = Random.Range(0, 3); // size
            if (var2 == 0 && Random.Range(0, 100) > 70)
            {
                var2 = Random.Range(1, 3);
            }

            GameObject newObj = (GameObject)Instantiate(solutionPrefab, target.position, target.rotation);
            MatchObjectSolBehaviour newObjBehaviour = newObj.GetComponent<MatchObjectSolBehaviour>();
            newObjBehaviour.setAllProperties(var1, var2, var3);
            solutionObjects.Add(newObjBehaviour);
            snapPoint.matchSolution = newObjBehaviour;

            minimumOperationCount[var1]++; // spawn correct shape
            if (var2 != 0)
            {
                minimumOperationCount[3+var2]++; // only increase if a size swap would be required.
            }
            if(var3 != 0)
            {
                minimumOperationCount[6 + var3]++; // only increase if a colour swap would be required.
            }
        }
    }

    private bool attemptSelection(float deltaTime)
    {
        if (camRef.selectedObject != null && selectedMatchObj != null && allowMoveObj)
        {
            return false;
        }

        if (camRef.clickOccured && (camRef.clickDebugMode || camRef.cursorMode != CameraBehaviour.CursorMode.LookAt))
        {
            //print("Searching for object");
            GameObject tempObj = findMatchObjInRay(camRef.getUpdatedRay());
            if (tempObj != null)
            {
                //print("Object found");
                selectObj(tempObj, true);
                isObjSnapped = false;
                camRef.clickUsed = true;
            }
        }
        else
        {
            GameObject tempObj = findMatchObjInRay(camRef.getUpdatedRay());
            if (tempObj != null)
            {
                if (tempObj == camRef.selectedObject)
                {
                    // object is already selected don't start again
                }
                else if (camRef.selectingObject == null || camRef.selectingObject != tempObj)
                {
                    camRef.selectionProgress = 0;
                    camRef.selectingObject = tempObj;
                }
                else
                {
                    camRef.selectionProgress += deltaTime;
                    if (camRef.selectionProgress >= camRef.timeToSelect)
                    {
                        selectObj(tempObj, true);
                        camRef.selectionProgress = 0;
                        camRef.selectingObject = null;
                        isObjSnapped = false;
                    }
                    else
                    {
                        camRef.cursor.updateCursor(camRef.selectionProgress / camRef.timeToSelect);
                    }
                }
            }
            else
            {
                camRef.selectionProgress = 0;
                camRef.selectingObject = null;
            }
        }

        return true;
    }

    public bool attemptDeSelect(float deltaTime)
    {
        if (selectedMatchObj != null && allowMoveObj)
        {
            if (isObjSnapped)
            {
                if (camRef.deselectionProgress >= camRef.timeToSelect)
                {
                    deSelect();
                    return true;
                }
                else
                {
                    camRef.deselectionProgress += deltaTime;
                    camRef.cursor.updateCursor(camRef.deselectionProgress / camRef.timeToSelect);
                    return false;
                }
            }
            else
            {
                camRef.deselectionProgress = 0;
            }
        }
        camRef.deselectionProgress = 0;
        return false;
    }

    private void updateSelected()
    {
        if (selectedMatchObj != null && allowMoveObj)
        {
            RaycastHit hit;
            Ray ray = camRef.getUpdatedRay();
            if (Physics.Raycast(ray, out hit, 9999.0f, 1 << 8))
            {
                //print(hit.transform.name);
                // get the hit point:
                Vector3 newPoint = ray.GetPoint(hit.distance);
                //newPoint.y += 1.59f;

                float distanceSnap = 99999;
                Vector3 snapLocation = newPoint;
                isObjSnapped = false;
                foreach (MatchSnapBehaviour snapPoint in snapNodes)
                {
                    if (snapPoint.getMatchObj() != null) { continue; }

                    float distanceTemp = Vector3.Distance(snapPoint.transform.position, newPoint);
                    if (distanceTemp < distanceSnap && distanceTemp < 5)
                    {
                        snapLocation = snapPoint.transform.position;
                        snapLocation.y += 1.59f;
                        distanceSnap = distanceTemp;
                        isObjSnapped = true;
                        tempSnapCache = snapPoint;
                        break;
                    }
                }

                camRef.selectedObject.transform.position = snapLocation;
            }
        }
    }

    public void fixCircularMenuPos()
    {
        MatchSnapBehaviour curSnap = null;
        foreach (MatchSnapBehaviour snap in snapNodes)
        {
            if (snap.getMatchObj() == selectedMatchObj)
            {
                curSnap = snap;
                break;
            }
        }

        if (curSnap != null)
        {
            camRef.circularMenu.transform.position = curSnap.getHoverPosition();
        }
    }

    public void selectObj(GameObject obj, bool enableEdit)
    {
        // before allowing selection test if the object is already part of a complete solution
        foreach (MatchSnapBehaviour snapPoint in snapNodes)
        {
            if (snapPoint.getMatchObj() != null && snapPoint.getMatchObj().gameObject == obj && snapPoint.matchValid())
            {
                print("Object selection failed: " + obj);
                return;
            }
        }

        camRef.selectedObject = obj;

        if (obj != null)
        {
            selectedMatchObj = obj.GetComponent<MatchObjectBehaviour>();
            if (enableEdit)
            {
                if (circularOnSelect)
                {
                    camRef.circularMenu.showMenu(ExperimentState.OBJECTMODEDITMENU);
                    fixCircularMenuPos();
                    camRef.circularMenu.transform.rotation = Quaternion.LookRotation(camRef.transform.position - camRef.circularMenu.transform.position) * Quaternion.Euler(0, 180, 0);
                    //print("set location of menu.");
                }
                else
                {
                    camRef.peripheryBehaviour.leftMenuDef = ExperimentState.OBJECTMODEDITMENU;
                    camRef.peripheryBehaviour.rightMenuDef = ExperimentState.OBJECTMODEDITMENU;
                }
            }
            
            /*if (selectedTower != null)
            {
                foreach(TowerSnapBehaviour snapPoint in towerSnapPoints)
                {
                    snapPoint.gameObject.GetComponent<MeshRenderer>().enabled = true;
                }
                //selectedTower.setTowerState(TowerBehaviour.TowerState.TowerPlacement);
            }*/
        }
    }

    public void deSelect()
    {
        //tempSnapCache = selectedMatchObj.gameObject;

        if (allowMoveObj)
        {
            // only do this when an object is being moved. The tempSnapCache variable is not updated at other times.
            tempSnapCache.setMatchObj(selectedMatchObj);
        }
        camRef.selectedObject = null;
        selectedMatchObj = null;
        allowMoveObj = false;
        isObjSnapped = false;
        camRef.peripheryBehaviour.leftMenuDef = ExperimentState.OBJECTMODCREATEMENU;
        camRef.peripheryBehaviour.rightMenuDef = ExperimentState.OBJECTMODCREATEMENU;

        if(circularOnSelect)
        {
            camRef.circularMenu.menuActive = false;
        }

        /*foreach (TowerSnapBehaviour snapPoint in towerSnapPoints)
        {
            snapPoint.gameObject.GetComponent<MeshRenderer>().enabled = false;
        }*/

        //levelManager.updateMenuRightText();
    }

    private GameObject findObjectByTagInRay(Ray ray, string tag)
    {
        RaycastHit[] hit;
        hit = Physics.RaycastAll(ray);
        if (hit.Length == 0)
            return null;

        for (int i = 0; i < hit.Length; i++)
        {
            if (hit[i].transform.tag.Equals(tag))
            {
                return hit[i].transform.gameObject;
            }
        }
        return null;
    }

    private GameObject findMatchObjInRay(Ray ray)
    {
        return findObjectByTagInRay(ray, "MatchObject");
    }

    private void saveAllTaskData()
    {
        //minimumOperationCount; // spawnCylinder, spawnCube, spawnSphere, setSmall, setMed, setLarge, setBlue, setRed, setGreen
        //operationCount; // spawnCylinder, spawnCube, spawnSphere, setSmall, setMed, setLarge, setBlue, setRed, setGreen, setCylinder, setCube, setSphere
        //menuOperationCount; // Deselect, Back, Open Shape Menu, Open Size Menu, Open Colour Menu, Move Object

        ExtraDataRecorder eDR = ExtraDataRecorder.getSingleton();
        string collectionName = ((circularOnSelect) ? "Circular" : "Periphery") + " Match Puzzle Metrics";
        ExtraDataRecorder.ExtraDataCollection eDC = eDR.getDataCollection(collectionName);

        eDC.logData("variable, spawnCylinder, spawnCube, spawnSphere, setSmall, setMed, setLarge, setBlue, setRed, setGreen, setCylinder, setCube, setSphere");
        string mOCStr = "minimumOperationCount,";
        foreach (int i in minimumOperationCount) mOCStr += (i + ",");
        mOCStr += "0,0,0";
        eDC.logData(mOCStr);

        string oCStr = "operationCount,";
        foreach (int i in operationCount) oCStr += (i + ",");
        eDC.logData(oCStr);

        string diffStr = "difference,";
        for (int i = 0; i < minimumOperationCount.Length; i++)
        {
            diffStr += ((operationCount[i] - minimumOperationCount[i]) + ",");
        }
        diffStr += operationCount[9] + "," + operationCount[10] + "," + operationCount[11];
        eDC.logData(diffStr);

        eDC.logData("MenuOperationCount,Deselect,Back,Shape Menu, Size Menu, Colour Menu, Move Obj");
        string menOC = ",";
        foreach (int i in menuOperationCount) menOC += (i + ",");
        eDC.logData(menOC);
    }
}
