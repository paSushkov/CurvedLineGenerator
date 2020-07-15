using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    // Start is called before the first frame update
    public float RotationSpeed;

    private Transform myTransform;
    void Start()
    {
        myTransform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        myTransform.Rotate(Vector3.forward, RotationSpeed * Time.deltaTime);
    }
}
