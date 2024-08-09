using System;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public class SkateboardController : MonoBehaviour
{
    public GameObject board_visual;
    [HideInInspector]
    public bool on_ground = true;

    private Rigidbody rb;
    private readonly Vector3 gravity = new(0, -50f, 0);
    private readonly float sideways_friction = 1f;
    private readonly float max_speed = 5f;
    private readonly float kickturn_thresh = 2.5f;
    void Start()
    {   
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

        //rotate rigid
        Quaternion delta_rotation;
        if (Math.Abs(local_velocity.z) < kickturn_thresh) {
            //kickturn speed
            delta_rotation = Quaternion.Euler(new Vector3(0, h_input, 0) * Time.fixedDeltaTime * 100);
        } else if (local_velocity.z >= 0) {
            delta_rotation = Quaternion.Euler(new Vector3(0, h_input, 0) * Time.fixedDeltaTime * local_velocity.z*20);
        } else {
            delta_rotation = Quaternion.Euler(new Vector3(0, h_input, 0) * Time.fixedDeltaTime * local_velocity.z*20);
        }

        rb.MoveRotation(rb.rotation * delta_rotation);

        //VISUAL EFFECTS

        //kickturn
        if (Math.Abs(h_input) > 0 && Math.Abs(local_velocity.z) < kickturn_thresh) { //kickturn
            Quaternion kickturn_rotation = Quaternion.Euler(new Vector3(-10f, transform.eulerAngles.y, transform.eulerAngles.z));
            board_visual.transform.rotation = Quaternion.Lerp(board_visual.transform.rotation, kickturn_rotation, 5f * Time.fixedDeltaTime);
        } else if (Math.Abs(h_input) == 0 || Math.Abs(local_velocity.z) > kickturn_thresh) {
            Quaternion kickturn_rotation = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, transform.eulerAngles.z));
            board_visual.transform.rotation = Quaternion.Lerp(board_visual.transform.rotation, kickturn_rotation, 5f * Time.fixedDeltaTime);
        }

    Transform deck = board_visual.transform.Find("Deck");
        //board tilt
        if (Math.Abs(h_input) > 0.1) {
            Quaternion deck_angle = Quaternion.Euler(new Vector3(board_visual.transform.eulerAngles.x, board_visual.transform.eulerAngles.y, h_input*-10f));
            deck.rotation = Quaternion.Lerp(deck.rotation, deck_angle, 10f * Time.fixedDeltaTime);
        } else {
            //reset
            Quaternion deck_angle = Quaternion.Euler(new Vector3(board_visual.transform.eulerAngles.x, board_visual.transform.eulerAngles.y, board_visual.transform.eulerAngles.z));
            deck.rotation = Quaternion.Lerp(deck.rotation, deck_angle, 10f * Time.fixedDeltaTime);
        }
    }

    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.name == "Floor") {
            on_ground = true;
        }
    }

    void OnCollisionExit(Collision collision) {
        if (collision.gameObject.name == "Floor") {
            on_ground = false;
        }
    }
}
