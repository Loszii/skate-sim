using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    private readonly float speed = 10f;

    void FixedUpdate()
    {
        //pos
        Vector3 offset = new(0, 1f, -3f);
        var target_position = target.TransformPoint(offset);
        transform.position = Vector3.Lerp(transform.position, target_position, speed * Time.deltaTime); //lerp for smoothing

        //rotation
        var direction = target.position - transform.position + new Vector3(0, 0.5f, 0); //vector pointing 0.5 above board
        var rotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, speed * Time.deltaTime);
    }
}
