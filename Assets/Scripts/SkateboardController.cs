using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SkateboardController : MonoBehaviour
{

    public WheelCollider front_left;
    public WheelCollider front_right;
    public WheelCollider back_left;
    public WheelCollider back_right;
    private readonly float speed = 40f;
    private readonly float max_turn_angle = 15f;

    void FixedUpdate() 
    {
        float h_input = Input.GetAxis("Horizontal");
        float v_input = Input.GetAxis("Vertical");

        //move foward
        front_left.motorTorque = v_input * speed;
        front_right.motorTorque = v_input * speed;
        back_left.motorTorque = v_input * speed;
        back_right.motorTorque = v_input * speed;

        //turn
        front_left.steerAngle = h_input * max_turn_angle;
        front_right.steerAngle = h_input * max_turn_angle;

    }
}
