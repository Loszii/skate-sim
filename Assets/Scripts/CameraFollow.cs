using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject skateboard;
    private Transform target;
    private float speed = 10f;
    private SkateboardController script;

    void Start() {
        script = skateboard.GetComponent<SkateboardController>();
        target = skateboard.transform;
    }

    void FixedUpdate()
    {
        //make the camera follow the board object
        //pos
        if (script.is_grinding) {
            speed = 5f;
        } else {
            speed = 10f;
        }
        Vector3 offset = new(0, 0, -2f); //-2 back in local position
        Vector3 target_position = target.TransformPoint(offset) + new Vector3(0, 1f, 0); //transformPoint is relative to object coors sys, then +1 relative to world
    
        transform.position = Vector3.Lerp(transform.position, target_position, speed * Time.fixedDeltaTime); //lerp for smoothing

        //rotation
        var direction = target.position - transform.position + new Vector3(0, 0.5f, 0); //vector pointing 0.5 above board
        var rotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, speed * Time.fixedDeltaTime);   
    }
}
