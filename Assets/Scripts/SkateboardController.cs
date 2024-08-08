using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkateboardController : MonoBehaviour
{

    public bool on_ground = false;
    private Rigidbody rb;
    private Vector3 velocity;
    private readonly Vector3 gravity = new(0, -50f, 0);
    private readonly float sideways_friction = 1f;
    private readonly float max_speed = 5f;
    void Start()
    {   
        velocity = new Vector3(0, 0, 0);
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        //PHYSIC

        //gravity
        rb.AddForce(gravity * Time.fixedDeltaTime, ForceMode.Acceleration);

        //add sideways friction for realistic turning
        Vector3 local_velocity = transform.InverseTransformDirection(rb.velocity);
        float sideways_velocity = local_velocity.x;
        if (sideways_velocity > 1) {
            sideways_velocity -= sideways_friction * Time.fixedDeltaTime;
        } else if (sideways_velocity < -1) {
            sideways_velocity += sideways_friction * Time.fixedDeltaTime;
        } else {
            sideways_velocity = 0;
        }
        local_velocity.x = sideways_velocity; //set back

        rb.velocity = transform.TransformDirection(local_velocity); //set back to world rel


        //INPUT

        //controls
        float h_input = Input.GetAxis("Horizontal");
        float v_input = Input.GetAxis("Vertical");

        //forward
        if (rb.velocity.magnitude < max_speed) {
            rb.AddForce(transform.forward * v_input * Time.fixedDeltaTime * 200, ForceMode.Acceleration);
        }

        //rotate
        if (Math.Abs(local_velocity.z) > 0) {
            rb.AddTorque(transform.up * h_input * 60 * Time.fixedDeltaTime);
        }

    }

    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.name == "Floor") {
            on_ground = true;
        }
    }
}
