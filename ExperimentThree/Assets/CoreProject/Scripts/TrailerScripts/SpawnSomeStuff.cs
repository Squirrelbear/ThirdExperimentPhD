using UnityEngine;
using System.Collections;

public class SpawnSomeStuff : MonoBehaviour {

    public AISpawner spawner;
    public bool firstUpdate = true;

    public TowerBehaviour[] towers;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	    if(firstUpdate)
        {
            spawner.spawnWave("0.2:0;0.2:1;0.3:2;0.2:0;0.2:1;0.3:2;0.2:0;0.2:1;0.3:2;1:3;0.2:0;0.2:1;0.3:2;0.2:0;0.2:1;0.3:2;0.2:0;0.2:1;0.3:2;1:3;0.2:0;0.2:1;0.3:2;0.2:0;0.2:1;0.3:2;"
                              + "0.2:0;0.2:1;0.3:2;0.2:0;0.2:1;0.3:2;0.2:0;0.2:1;0.3:2;1:3;0.2:0;0.2:1;0.3:2;0.2:0;0.2:1;0.3:2;0.2:0;0.2:1;0.3:2;1:3;0.2:0;0.2:1;0.3:2;0.2:0;0.2:1;0.3:2;-1;-1");
            towers = GameObject.Find("DummyTowers").GetComponentsInChildren<TowerBehaviour>();
            firstUpdate = false;
        }

        spawner.update(Time.deltaTime);
        foreach(TowerBehaviour tower in towers)
        {
            tower.update(Time.deltaTime);
        }
	}
}
