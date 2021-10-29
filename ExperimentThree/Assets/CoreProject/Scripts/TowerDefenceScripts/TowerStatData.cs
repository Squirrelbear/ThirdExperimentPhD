using UnityEngine;
using System.Collections;

public class TowerStatData : MonoBehaviour {

    // Note that it is: damage,range,fire delay

    public float[,] towerBaseStats = new float[,] { { 20, 20, 0.5f }, { 0.6f, 15, 1 }, { 20, 20, 0.2f }, { 35, 20, 0.8f } };
    public static float[,] towerModStats = new float[,] { { 20, 40, 0.5f }, { 0.5f, 25, 1 }, { 20, 30, 0.2f }, { 30, 30, 0.8f } };

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
