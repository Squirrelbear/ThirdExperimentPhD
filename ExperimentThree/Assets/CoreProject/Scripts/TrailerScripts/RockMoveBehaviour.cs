using UnityEngine;
using System.Collections;

public class RockMoveBehaviour : MonoBehaviour {

    Vector3 startPos;
    Quaternion startRotation;
    Vector3 endPos;
    float height = 12f;
    bool startMove = false;
    float incrementor = 0;
    float incrementorAmount = 0.02f;

    void Start()
    {
        startPos = transform.position;
        startRotation = transform.rotation;
        incrementorAmount = Random.Range(0.5f, 0.7f);
    }

    // Update is called once per frame
    void Update()
    {
        if (startMove)
        {
            incrementor += (incrementorAmount * Time.deltaTime);
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, incrementor);
            currentPos.y += height * Mathf.Sin(Mathf.Clamp01(incrementor) * Mathf.PI);
            transform.position = currentPos;

            Vector3 targetDir = endPos - startPos;
            //float step = incrementorAmount * Time.deltaTime;
            transform.Rotate(targetDir * incrementor/2);
            /*Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, incrementor*1000, 0.0F);
            print(newDir);
            Debug.DrawRay(transform.position, newDir, Color.red);
            transform.rotation = Quaternion.LookRotation(newDir);*/
        }
        if (transform.position == endPos)
        {
            startMove = false;
            incrementor = 0;
            /*Vector3 tempPos = startPos;
            startPos = transform.position;
            endPos = tempPos;*/
        }
    }

    public void resetPosition()
    {
        transform.position = startPos;
        transform.rotation = startRotation;
    }

    public void beginTossTo(Vector3 pos)
    {
        endPos = pos;
        startMove = true;
    }
}
