using UnityEngine;
using System.Collections;

public class TickBoxBehaviour : MonoBehaviour {
    private Camera cam;

	// Use this for initialization
	void Start () {
        cam = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
        if (cam == null)
            return;

        transform.rotation = Quaternion.LookRotation(cam.transform.position - transform.position) * Quaternion.Euler(90, 0, 0);
	}
}
