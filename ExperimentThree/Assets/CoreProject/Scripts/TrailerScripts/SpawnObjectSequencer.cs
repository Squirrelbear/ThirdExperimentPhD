using UnityEngine;
using System.Collections;

public class SpawnObjectSequencer : MonoBehaviour {

    public GameObject[] objects;
    public float timeBetweenSpawns = 0.3f;
    public int index = 0;
    public float timer = -2;

    public static bool beginSpawn = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (beginSpawn)
        {
            if (index >= objects.Length)
                return;

            timer += Time.deltaTime;
            if (timer > timeBetweenSpawns)
            {
                timer = 0;
                objects[index].SetActive(true);
                index++;
            }
        }
	}
}
