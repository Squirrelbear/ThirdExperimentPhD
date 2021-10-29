using UnityEngine;
using System.Collections;

public class TrailerCamera : MonoBehaviour {
    //public float endZAmount;

    //public float endZ;
    public float panSpeed = 0.02f;
    public float rotSpeed = 0.01f;
    private float panProgress = 0;
    private float rotProgress = 0;

    public Transform start, end;
    public bool move = false;

    private Vector3 startRot, endRot;

    public GameObject[] sequenceElements;
    public float timeBetweenSequences = 2;
    public float timer = 0;
    public int curSequence = 0;

    // Use this for initialization
    void Start()
    {
        

    }

    // Update is called once per frame
    void Update()
    {
        if(!move)
        {
            timer += Time.deltaTime;
            if (timer > timeBetweenSequences)
            {
                timer = 0;
                beginNextSequence();
            }
        }
        else
        {
            updateSequence();
        }

       /* if (transform.position.z < endZ)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + panSpeed * Time.deltaTime);
        }*/
    }

    public void beginNextSequence()
    {
        if (curSequence >= sequenceElements.Length)
            return;

        
        start = sequenceElements[curSequence].transform;
        end = sequenceElements[curSequence + 1].transform;

        startRot = start.eulerAngles;
        endRot = end.eulerAngles;

        transform.eulerAngles = startRot;
        transform.position = start.position;
        move = true;
        panProgress = 0;
        rotProgress = 0;

        if(curSequence == 2)
        {
            SpawnObjectSequencer.beginSpawn = true;
        }

        curSequence += 2;
    }

    public void updateSequence()
    {
        if (move)
        {
            panProgress += panSpeed * Time.deltaTime;
            rotProgress += rotSpeed * Time.deltaTime;

            Vector3 currentAngle = new Vector3(
                Mathf.LerpAngle(startRot.x, endRot.x, rotProgress),
                Mathf.LerpAngle(startRot.y, endRot.y, rotProgress),
                Mathf.LerpAngle(startRot.z, endRot.z, rotProgress));

            transform.eulerAngles = currentAngle;

            Vector3 currentPos = Vector3.Lerp(start.position, end.position, panProgress);
            //print(currentPos);
            transform.position = currentPos;
        }
        if (transform.position == end.position)
        {
            move = false;
        }
    }
}
