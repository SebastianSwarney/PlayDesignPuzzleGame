using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRotation : MonoBehaviour
{
    // Update is called once per frame
    public float speed;
    public float amplitude;
    public float floatFrequency;

    float x;
    float y;
    float z;

    Vector3 posOffset = new Vector3();
    Vector3 tempsPos = new Vector3();

    private void Start()
    {
        posOffset = transform.position;

        x = Random.Range(0, 360);
        y = Random.Range(0, 360);
        z = Random.Range(0, 360);
    }
    void Update()
    {
        transform.Rotate(new Vector3(x * Time.deltaTime * speed, y * Time.deltaTime * speed, z * Time.deltaTime * speed));

        tempsPos = posOffset;
        tempsPos.y += Mathf.Sin(Time.time * floatFrequency) * amplitude;
        transform.position = tempsPos;
    }
}
