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
    public bool on_ground;
    private Rigidbody rb;
    private readonly float speed = 40f;
    private readonly float max_turn_angle = 15f;
    private readonly float pop = 100f;
    private readonly float flip_speed = 400f;

    void Start() {
        rb = GetComponent<Rigidbody>();
    }


    void FixedUpdate() 
    {
        float h_input = Input.GetAxis("Horizontal");
        float v_input = Input.GetAxis("Vertical");
        on_ground = front_left.isGrounded && front_right.isGrounded && back_left.isGrounded && back_right.isGrounded;

        //move foward
        front_left.motorTorque = v_input * speed;
        front_right.motorTorque = v_input * speed;
        back_left.motorTorque = v_input * speed;
        back_right.motorTorque = v_input * speed;

        //turn
        front_left.steerAngle = h_input * max_turn_angle;
        front_right.steerAngle = h_input * max_turn_angle;

        //jump
        if (Input.GetKey("space") && on_ground) {
            rb.AddForce(Vector3.up * pop, ForceMode.Impulse);
        }

        //flipping
        if (!on_ground) {
            Quaternion delta_rotation = Quaternion.Euler(new Vector3(0, 0, h_input) * Time.fixedDeltaTime * -flip_speed);
            rb.MoveRotation(rb.rotation * delta_rotation);
        }
    }
}
