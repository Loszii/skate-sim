using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject skateboard;
    private Transform target;
    private readonly float speed = 10f;
    private SkateboardController script;

    void Start() {
        script = skateboard.GetComponent<SkateboardController>(); //can use script.on_ground to change air camera settings
        target = skateboard.transform;
    }

    void FixedUpdate()
    {
        //pos
        Vector3 offset = new(0, 0, -2f); //-2 back in local position
        Vector3 target_position = target.TransformPoint(offset) + new Vector3(0, 1f, 0); //transformPoint is relative to object coors sys, then +1 relative to world
    
        transform.position = Vector3.Lerp(transform.position, target_position, speed * Time.fixedDeltaTime); //lerp for smoothing

        //rotation
        var direction = target.position - transform.position + new Vector3(0, 0.5f, 0); //vector pointing 0.5 above board
        var rotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, speed * Time.fixedDeltaTime);   
    }
}
