using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExperimentState : MonoBehaviour {

    public static int GAMEVERSION = 30;

    public enum GameSuperState { MainMenu = 0, 
                            ObjectManipPerph = 100, 
                            ObjectManipCirc = 200, 
                            TowerDefenceHead = 300,
                            TowerDefenceControl = 400,
                            Complete = 500 };

    public PeripheryBehaviour peripheryBehaviour;
    public List<GameObject> stateObjects;

    private int[] maxStateOfEach = new int[]{4,5,3,8,3,0};

    public GameObject menuInformation;
    public GameObject menuQuestion;

    private InformationMenuBehaviour infoMenuBehaviour;
    private QuestionMenuBehaviour questionMenuBehaviour;
    public Transform menuPositionMainMenu;

    private bool firstUpdate = true;

    private int[] questionResponses = new int[4];

    public int skipToState = 0;

    public ExtraDataRecorder extraDataRecorder;
    public ExtraDataRecorder.ExtraDataCollection timeEventLog;
    public ExtraDataRecorder.ExtraDataCollection questionEventLog;
    public float timeSinceLastState = 0;

    public enum GameState
    {
        MainMenu = 0,
        MainMenu_Intro01 = 1,       // Welcome and thankyou; evaluation for this experiment should consider 
                                    //if periphery menu system is useful and not based on the quality of the game
        MainMenu_Intro02 = 2,       // Show stages of experiment
        MainMenu_Intro03 = 3,       // Explain after hitting continue that the user should look either left or right 
        MainMenu_WaitForBegin = 4,

        ObjectManipPerph = 100,
        ObjectManipPerph_Intro1 = 101,
        ObjectManipPerph_Intro2 = 102,
        ObjectManipPerph_Intro3 = 103,
        ObjectManipPerph_Test = 104,
        ObjectManipPerph_Question = 105,

        ObjectManipCirc = 200,
        ObjectManipCirc_Intro1 = 201,
        ObjectManipCirc_Test = 202,
        ObjectManipCirc_Question = 203,

        TowerDefenceHead = 300,
        TowerDefenceHead_Intro01 = 301,
        TowerDefenceHead_Intro02 = 302,
        TowerDefenceHead_Intro03 = 303,
        TowerDefenceHead_Intro04 = 304,
        TowerDefenceHead_Intro05 = 305,
        TowerDefenceHead_Intro06 = 306,
        TowerDefenceHead_Test = 307,
        TowerDefenceHead_Question1 = 308,

        TowerDefenceControl = 400,
        TowerDefenceControl_Intro = 401,
        TowerDefenceControl_Test = 402,
        TowerDefenceControl_Question1 = 403,

        Complete = 500
    };

    // Temp
    //private const string TODO = "ERRORERRORERRORERRORERROR NEED TO DEFINE THIS STILL ERRORERRORERRORERRORERROR";

    // Menu definitions
    public const string MAINMENUWAITFORBEGINMENU = "Main Menu,Select Any To Begin,-1,Select Any To Begin,-1,Select Any To Begin,-1,Select Any To Begin,-1";
    public const string TOWERDEFENCEMENULEFT = "Modify Tower,Move Tower,3000,Repair Tower [-%R%],3001,Destroy Tower [+%D%],3002,Deselect Tower,3003";
    public const string TOWERDEFENCEMENURIGHT = "Create Tower,%B%,1000,%F%,1001,%S%,1002,%E%,1003";

    public const string OBJECTMODCREATEMENU = "Create Object,Cylinder,9001,Cube,9002,Sphere,9003,Cancel,9004";
    public const string OBJECTMODEDITMENU = "Modify Object,Change Type,9010,Change Size,9020,Change Colour,9030,Move Object,9050";
    public const string OBJECTMODTYPEMENU = "Change Type,Cylinder,9011,Cube,9012,Sphere,9013,Back,9000";
    public const string OBJECTMODSIZEMENU = "Change Size,Small,9021,Medium,9022,Large,9023,Back,9000";
    public const string OBJECTMODCOLOURMENU = "Change Colour,Blue,9031,Red,9032,Green,9033,Back,9000";

    // Main menu dialogue
    private const string MAINMENUINTROONE = "Welcome to this short experiment. Thank you for taking the time to participate, your input is greatly appreciated. "
                                          + "While evaluating this experiment for any questions that are asked consider the hands free nature of the menu."
                                          + " The game itself is just a means to demonstrate a few simple examples where this menu could be applied.";
    private const string MAINMENUINTROTWO = "There will be four stages you will be asked to complete for this experiment.\n"
                                          + "1. Object Manipulation with Periphery Menu\n2. Object Manipulation with Circular Menu\n3. Tower Defence\n3. Tower Defence with Controller";
    private const string MAINMENUINTROTHREE = "If you have any questions or run into problems at any point please ask."
                                            + "\nAfter selecting Continue on this dialogue you should look quickly to the left or right to reveal the main menu. "
                                            + "Selecting any option there will begin the first task.";

    // Object Manipulation Task
    private const string OBJECTMANIPINTROONE = "For this first task you will be shown 5 objects in the area before you. These will be pulsing to make them obvious. "
                                              + "The goal is to create 5 matching objects. To match objects you need to match the position, shape, size, and colour. "
                                              + "Looking quickly to the left or right will spawn the menu you will use to spawn new objects or manipulate existing ones.";
    private const string OBJECTMANIPINTROTWO = "When you have no object selected the menu shown will be one to create a new object of a selectable type. If you select "
                                              + "an object by looking directly at it the menu shown from looking left or right will provide options to modify the other "
                                              + "properties of the object.";
    private const string OBJECTMANIPINTROTHREE = "Finally before you begin, it is important to note that if you wish to adjust your view of the world, there are nodes like "
                                              + "the yellow one shown to the right. If you look directly at these for a moment they will transition you to a persective from "
                                              + "the selected side. These will be present for all tasks including the tower defence.\n\nWhen you are ready to begin, select continue.";
    private const string OBJECTMANIPINTROFOUR = "The following task will be the same as the task you have just completed, however the periphery menu will not be used "
                                              + "for object manipulation after selecting objects. Instead you will use a simple circular type menu. This will function "
                                              + "in the same way it did when using the periphery version. You will still use the menu from looking left or right to create "
                                              + "objects.\n\nWhen you are ready to begin, select continue.";

    // Tower Defence dialogue
    private const string TOWERDEFENCEINTROONE = "These are the enemies you will be facing!\n"
                                              + "1. Basic Spider (front left): A basic medium speed unit.\n"
                                              + "2. Fast Spider (front middle): A fast moving unit.\n"
                                              + "3. Dangerous Spider (front right): A medium speed unit that will damage your towers.\n"
                                              + "4. Boss Spider (middle): A slow, but armoured queen.\n"
                                              + "5. Final Boss ( back): If you see this: run!";
    private const string TOWERDEFENCEINTROTWO = "To create a tower you can make the menu appear by looking to the left or right. The menu shown will appear as the "
                                              + "one in front of you. Selecting a tower will then allow you to place them into the world. "
                                              + "The cost to build a tower is shown in the brackets to the right. If you do not have enough, "
                                              + "than the button will show \"Insufficent Funds\" instead.";
    private const string TOWERDEFENCEINTROTHREE = "The towers you will be building during this game are (shown left to right):\n"
                                                + "1. Basic Tower: Long range, lower damage tower.\n"
                                                + "2. Frost Tower: Pulses a slowing aura making enemies easier to hit.\n"
                                                + "3. Swarm Tower: Fast firing, medium damage tower.\n"
                                                + "4. Explosive Tower: Fires shells that damage in a small area around the target enemy.";
    private const string TOWERDEFENCEINTROFOUR = "Towers that you have placed into the game can be modified in a few ways. You can hover over a tower to "
                                              + "see the durability. If you hover for long enough the tower will be selected. The dialog shown above will appear while selecting. "
                                              + "The bottom dialog below will appear once selection is complete. A damaged tower can be repaired with the left button, " 
                                              + "also showing the cost of repair. During downtime the move option can let you move a tower to somewhere else. ";
    private const string TOWERDEFENCEINTROFIVE = "The goal of a tower defence is to prevent the enemy units from reaching the base at the end. "
                                              + "Choose your towers carefully, and use them to ensure the enemy forces will not reach your base. "
                                              + "There will be four phases with time between for moving your towers if desired. \n"
                                              + "The dialog shown will indicate the current state of the level, showing your currency, base health, and wave status.";
    private const string TOWERDEFENCEINTROSIX = "When you continue from this screen you will have a period of time to place your initial towers before the first wave "
                                              + "begins to spawn. \n\nIf you have any additional questions about how to play you should ask the researcher now.\n\n"
                                              + "Once you hit continue the tower defence will begin.";
    private const string TOWERDEFENCEINTROCONTROLLER = "You will are now tasked with completing the tower defence once more. This time you will be provided with a controller. "
                                                     + "The controller will only allow you to provide input using the \"A\" button. When you do this it will perform selections. "
                                                     + "\n\nBefore you continue, make sure you have the controller in your hands. And if you have any additional questions, you can ask them now.";

    // Questions
    private const string OBJECTMANIPPERPHQUESTION = "On a scale of 1 to 5, where 1 is low and 5 is high, how effective was the periphery menu system for completing this untimed task?";
    private const string OBJECTMANIPCIRCQUESTION = "On a scale of 1 to 5, where 1 is low and 5 is high, how effective was the circular menu system for completing this untimed task?";
    private const string TOWERDEFENCEHEADQUESTION = "On a scale of 1 to 5, where 1 is low, and 5 is high, how effective was the periphery vision menu for completing this time sensitive task?";
    private const string TOWERDEFENCECONTROLQUESTION = "On a scale of 1 to 5, where 1 is low, and 5 is high, how effective was the periphery vision menu for completing this time sensitive task with the addition of the controller?";

    public struct GameStateMenuDef
    {
        public GameState gameState;
        public string menuDefLeft, menuDefRight, menuDefOtherTitle, menuDefOtherBody;
        public int showMenu;

        public GameStateMenuDef(GameState gameState, string menuDefLeft, string menuDefRight, string menuDefOtherTitle, string menuDefOtherBody, int showMenu)
        {
            this.gameState = gameState;
            this.menuDefLeft = menuDefLeft;
            this.menuDefRight = menuDefRight;
            this.menuDefOtherTitle = menuDefOtherTitle;
            this.menuDefOtherBody = menuDefOtherBody;
            this.showMenu = showMenu; // 0 = none, 1 = info screen in main menu, 2 = question in main menu, 3 = info screen in tower defence, 4 = question screen in tower defence
        }
    }

    public List<List<GameStateMenuDef>> stateStrings = new List<List<GameStateMenuDef>> {
        // MainMenu
        new List<GameStateMenuDef>{
            new GameStateMenuDef(GameState.MainMenu,"","","","",0),
            new GameStateMenuDef(GameState.MainMenu_Intro01,"","","Main Menu: Introduction 1/3",MAINMENUINTROONE,1),
            new GameStateMenuDef(GameState.MainMenu_Intro02,"","","Main Menu: Introduction 2/3",MAINMENUINTROTWO,1),
            new GameStateMenuDef(GameState.MainMenu_Intro03,"","","Main Menu: Introduction 3/3",MAINMENUINTROTHREE,1),
            new GameStateMenuDef(GameState.MainMenu_WaitForBegin,MAINMENUWAITFORBEGINMENU,MAINMENUWAITFORBEGINMENU,"","",0),
        },
        
        // Object Manipulation with Periphery Menu
        new List<GameStateMenuDef>{
            new GameStateMenuDef(GameState.ObjectManipPerph,"","","","",0),
            new GameStateMenuDef(GameState.ObjectManipPerph_Intro1,"","","Object Manipulation Test 1: Introduction 1/3",OBJECTMANIPINTROONE,1),
            new GameStateMenuDef(GameState.ObjectManipPerph_Intro2,"","","Object Manipulation Test 1: Introduction 2/3",OBJECTMANIPINTROTWO,1),
            new GameStateMenuDef(GameState.ObjectManipPerph_Intro3,"","","Object Manipulation Test 1: Introduction 3/3",OBJECTMANIPINTROTHREE,1),
            new GameStateMenuDef(GameState.ObjectManipPerph_Test,OBJECTMODCREATEMENU,OBJECTMODCREATEMENU,"","",0),
            new GameStateMenuDef(GameState.ObjectManipPerph_Question,"","","User Response Question",OBJECTMANIPPERPHQUESTION,2),
        },

        // Object Manipulation with Circular Menu
        new List<GameStateMenuDef>{
            new GameStateMenuDef(GameState.ObjectManipCirc,"","","","",0),
            new GameStateMenuDef(GameState.ObjectManipCirc_Intro1,"","","Object Manipulation Test 2: Introduction",OBJECTMANIPINTROFOUR,1),
            new GameStateMenuDef(GameState.ObjectManipCirc_Test,OBJECTMODCREATEMENU,OBJECTMODCREATEMENU,"","",0),
            new GameStateMenuDef(GameState.ObjectManipCirc_Question,"","","User Response Question",OBJECTMANIPCIRCQUESTION,2),
        },

        // Tower Defence
        new List<GameStateMenuDef>{
            new GameStateMenuDef(GameState.TowerDefenceHead,"","","","",0),
            new GameStateMenuDef(GameState.TowerDefenceHead_Intro01,"","","Tower Defence: Introduction 1/6",TOWERDEFENCEINTROONE,1),
            new GameStateMenuDef(GameState.TowerDefenceHead_Intro02,"","","Tower Defence: Introduction 2/6",TOWERDEFENCEINTROTWO,1),
            new GameStateMenuDef(GameState.TowerDefenceHead_Intro03,"","","Tower Defence: Introduction 3/6",TOWERDEFENCEINTROTHREE,1),
            new GameStateMenuDef(GameState.TowerDefenceHead_Intro04,"","","Tower Defence: Introduction 4/6",TOWERDEFENCEINTROFOUR,1),
            new GameStateMenuDef(GameState.TowerDefenceHead_Intro05,"","","Tower Defence: Introduction 5/6",TOWERDEFENCEINTROFIVE,1),
            new GameStateMenuDef(GameState.TowerDefenceHead_Intro06,"","","Tower Defence: Introduction 6/6",TOWERDEFENCEINTROSIX,1),
            new GameStateMenuDef(GameState.TowerDefenceHead_Test,TOWERDEFENCEMENURIGHT,TOWERDEFENCEMENURIGHT,"","",0),
            new GameStateMenuDef(GameState.TowerDefenceHead_Question1,"","","User Response Question",TOWERDEFENCEHEADQUESTION,2)
        },

        // Tower Defence with Controller
        new List<GameStateMenuDef>{
            new GameStateMenuDef(GameState.TowerDefenceControl,"","","","",0),
            new GameStateMenuDef(GameState.TowerDefenceControl_Intro,"","","Tower Defence with Controller Introduction",TOWERDEFENCEINTROCONTROLLER,1),
            new GameStateMenuDef(GameState.TowerDefenceControl_Test,TOWERDEFENCEMENURIGHT,TOWERDEFENCEMENURIGHT,"","",0),
            new GameStateMenuDef(GameState.TowerDefenceControl_Question1,"","","User Response Question",TOWERDEFENCECONTROLQUESTION,2)
        },

        // Complete
        new List<GameStateMenuDef>{
            new GameStateMenuDef(GameState.Complete,"","","Testing Complete","Thank you for participating!",1)
        },
    };


    public GameSuperState gameSuperState = GameSuperState.MainMenu;
    public GameState gameState = GameState.MainMenu;

    public ReplayEngine replayEngine;
    public CameraBehaviour camRef;

	// Use this for initialization
	void Start () {
        peripheryBehaviour = gameObject.GetComponent<PeripheryBehaviour>();
        replayEngine = gameObject.GetComponent<ReplayEngine>();
        camRef = gameObject.GetComponent<CameraBehaviour>();
        for(int i = 0; i < questionResponses.Length; i++)
        {
            questionResponses[i] = 0;
        }

        extraDataRecorder = ExtraDataRecorder.getSingleton();
        timeEventLog = extraDataRecorder.getDataCollection("Time Log");
        timeEventLog.logData("Timestamp,GameSuperState,GameState,timeSinceLastState", false);
        questionEventLog = extraDataRecorder.getDataCollection("User Response Questions");
        questionEventLog.logData("Timestamp,Question,Response", false);

        int setSkipTo = 0;
        bool success = Settings.parseIntSetting("SkipToStage", ref setSkipTo);
        if(success)
        {
            skipToState = setSkipTo;
        }

        int gameSettingVersion = GAMEVERSION;
        success = Settings.parseIntSetting("GameVersion", ref gameSettingVersion);
        if (success)
        {
            GAMEVERSION = gameSettingVersion;
        }
	}
	
	// Update is called once per frame
	public void update(float deltaTime)
    {
        // disable all states until they are needed.
        if (firstUpdate)
        {
            infoMenuBehaviour = menuInformation.GetComponent<InformationMenuBehaviour>();
            questionMenuBehaviour = menuQuestion.GetComponent<QuestionMenuBehaviour>();

            for (int i = 1; i < stateObjects.Count; i++)
            {
                stateObjects[i].SetActive(false);
            }
            firstUpdate = false;
        }
        timeSinceLastState += deltaTime;

        if (replayEngine.checkKey(KeyCode.F1))
        {
            applyAutoSave();
        }

        // Push into next state after one frame of beginning.
        if (((int)gameState) % 100 == 0)
        {
            nextState();
        }

        infoMenuBehaviour.update(deltaTime);
        questionMenuBehaviour.update(deltaTime);

        if (infoMenuBehaviour.menuAction == -1 || peripheryBehaviour.menuBehaviour.menuAction == -1 || questionMenuBehaviour.menuAction == -1)
        {
            testStoreQuestionResponse();
            nextState();
        }
        else if (replayEngine.checkKey(KeyCode.F12))//Input.GetKeyDown(KeyCode.F12))
        {
            //print("KeyCode F12");
            questionMenuBehaviour.menuResult = -2;
            testStoreQuestionResponse();
            nextState();
        }
        else if ((int)gameState < skipToState)
        {
            nextState();
        }
        else if((gameState == GameState.TowerDefenceControl_Test || gameState == GameState.TowerDefenceHead_Test) 
                && camRef.levelManager.levelState == LevelManager.LEVELSTATE.Complete)
        {
            nextState();
        }
	}

    public void nextState()
    {
        if (gameSuperState == GameSuperState.Complete)
        {
            timeSinceLastState = 0;
            return;
        }
        // Hide the menu to make it disappear between skips using the shortcut keys
        if (infoMenuBehaviour != null)
        {
            infoMenuBehaviour.menuActive = false;
        }
        if (questionMenuBehaviour != null)
        {
            questionMenuBehaviour.menuActive = false;
        }

        timeEventLog.logData(gameSuperState + "," + gameState + "," + timeSinceLastState,true);
        timeSinceLastState = 0;

        int curState = (int)gameState;
        int curStateSuper = (curState / 100); // truncate last two digits
        int gameStateIndex;
        if (curState % 100 + 1 > maxStateOfEach[curStateSuper])
        {
            stateObjects[curStateSuper].SetActive(false);
            curStateSuper += 1;
            gameStateIndex = 0;
            gameState = (GameState)(curStateSuper * 100);
            gameSuperState = (GameSuperState)(curStateSuper * 100);
            stateObjects[curStateSuper].SetActive(true);

            replayEngine.autoSave.setProperty("GameState", (int)gameState + "");
            AutoSave.saveAutoSave(replayEngine.autoSave);
        }
        else {
            gameState++;
            gameStateIndex = ((int)gameState) % 100;
        }

        peripheryBehaviour.leftMenuDef = stateStrings[curStateSuper][gameStateIndex].menuDefLeft;
        peripheryBehaviour.rightMenuDef = stateStrings[curStateSuper][gameStateIndex].menuDefRight;

        if (stateStrings[curStateSuper][gameStateIndex].showMenu == 1)
        {
            if (infoMenuBehaviour == null)
            {
                infoMenuBehaviour = menuInformation.GetComponent<InformationMenuBehaviour>();
            }

            infoMenuBehaviour.showMenu(stateStrings[curStateSuper][gameStateIndex].menuDefOtherTitle, stateStrings[curStateSuper][gameStateIndex].menuDefOtherBody);
            menuInformation.transform.position = menuPositionMainMenu.position;
        }
        else if (stateStrings[curStateSuper][gameStateIndex].showMenu == 2)
        {
            if (questionMenuBehaviour == null)
            {
                questionMenuBehaviour = menuQuestion.GetComponent<QuestionMenuBehaviour>();
            }

            questionMenuBehaviour.showMenu(stateStrings[curStateSuper][gameStateIndex].menuDefOtherTitle, stateStrings[curStateSuper][gameStateIndex].menuDefOtherBody);
            menuQuestion.transform.position = menuPositionMainMenu.position;
        }

        if(gameState == GameState.TowerDefenceHead_Test || gameState == GameState.TowerDefenceControl_Test)
        {
            camRef.setCameraToSnap(camRef.towerDefenceFirstSnap, camRef.towerDefenceCameraLookAt);
            // Trigger a proper periphery tower menu update
            camRef.levelManager.addCurrency(0);
            camRef.levelManager.nextState();
        }
        else if(gameState == GameState.TowerDefenceControl_Question1 || gameState == GameState.TowerDefenceHead_Question1)
        {
            camRef.setCameraToSnap(camRef.menusCameraSnapNode, camRef.otherModesCameraLookAt);
        }
        else if(gameState == GameState.ObjectManipCirc_Test || gameState == GameState.ObjectManipPerph_Test)
        {
            camRef.setCameraToSnap(camRef.firstCameraSnapNode, camRef.otherModesCameraLookAt);
        }
        else if(gameState == GameState.ObjectManipPerph_Question || gameState == GameState.ObjectManipCirc_Question)
        {
            camRef.setCameraToSnap(camRef.menusCameraSnapNode, camRef.otherModesCameraLookAt);
            camRef.matchObjectManager.resetAllNodes();
        }
        else if(gameState == GameState.TowerDefenceControl_Intro)
        {
            camRef.cursorMode = CameraBehaviour.CursorMode.XboxClick;
        }

        ErrorLog.logData("New State: " + gameState, true);
        //print("New State: " + gameState);
    }

    private void testStoreQuestionResponse()
    {
        if (questionMenuBehaviour.menuAction != -1)
        {
            return;
        }

        switch(gameState)
        {
            case GameState.ObjectManipPerph_Question:
                questionResponses[0] = questionMenuBehaviour.menuResult+1;
                ErrorLog.logData("GameState.ObjectManipPerph_Question: " + questionResponses[0], false);
                questionEventLog.logData(gameState + "," + questionResponses[0], true);
                break;
            case GameState.ObjectManipCirc_Question:
                questionResponses[1] = questionMenuBehaviour.menuResult+1;
                ErrorLog.logData("GameState.ObjectManipCirc_Question: " + questionResponses[1], false);
                questionEventLog.logData(gameState + "," + questionResponses[1], true);
                break;
            case GameState.TowerDefenceHead_Question1:
                questionResponses[2] = questionMenuBehaviour.menuResult+1;
                ErrorLog.logData("GameState.TowerDefenceHead_Question1: " + questionResponses[2], false);
                questionEventLog.logData(gameState + "," + questionResponses[2], true);
                break;
            case GameState.TowerDefenceControl_Question1:
                questionResponses[3] = questionMenuBehaviour.menuResult+1;
                ErrorLog.logData("GameState.TowerDefenceControl_Question1: " + questionResponses[3], false);
                questionEventLog.logData(gameState + "," + questionResponses[3], true);
                break;
        }
    }

    public void applyAutoSave()
    {
        replayEngine.useLastAutoSave();
        string s = replayEngine.autoSave.getPropertyValue((replayEngine.replayMode == ReplayEngine.ReplayMode.Replay) ? "SkipUsed":"GameState");
        ErrorLog.logData("Trying to parse data: " + s, true);
        //print("Trying to parse data: " + s);
        if (s.Length == 0)
        {
            // Can't apply because there is no jump state
            //print("Can't jump to state because no state was supplied by auto save.");
            ErrorLog.logData("Can't jump to state because no state was supplied by auto save.", true);
            return;
        }
        int i = int.Parse(s);
        i = i - i % 100;
        skipToState = i;

        if (skipToState > (int)gameState)
        {
            replayEngine.autoSave.setProperty("SkipUsed", skipToState + "");
            AutoSave.saveAutoSave(replayEngine.autoSave);
        }
    }
}
