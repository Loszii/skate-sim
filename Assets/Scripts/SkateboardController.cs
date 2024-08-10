using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

public class SkateboardController : MonoBehaviour
{
    public GameObject board_visual;
    public bool on_ground = true;
    public bool upside_down = false; //on ground with up facing down

    private Rigidbody rb;
    private Transform deck;
    private readonly Vector3 gravity = new(0, -150f, 0);
    private readonly float sideways_friction = 1f;
    private readonly float max_speed = 5f;
    private readonly float kickturn_thresh = 2.5f;
    private readonly float pop = 75f;
    void Start()
    {   
        rb = GetComponent<Rigidbody>();
        deck = board_visual.transform.Find("Deck");
    }

    void FixedUpdate()
    {
        //PHYSICS

        //gravity
        rb.AddForce(gravity * Time.fixedDeltaTime, ForceMode.Acceleration);

        //add sideways friction for realistic turning when on ground
        Vector3 local_velocity = transform.InverseTransformDirection(rb.velocity);
            if (on_ground) {
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
        }


        //INPUT

        //controls
        float h_input = Input.GetAxis("Horizontal");
        float v_input = Input.GetAxis("Vertical");

        //ground movement
        if (on_ground && !upside_down) {
            //forward
            if (rb.velocity.magnitude < max_speed) {
                rb.AddForce(transform.forward * v_input * Time.fixedDeltaTime * 400, ForceMode.Acceleration);
            }

            //rotate rigid
            Quaternion delta_rotation;
            if (Math.Abs(local_velocity.z) < kickturn_thresh && Math.Abs(v_input) == 0) {
                delta_rotation = Quaternion.Euler(new Vector3(0, h_input, 0) * Time.fixedDeltaTime * 100);
            } else {
                delta_rotation = Quaternion.Euler(new Vector3(0, h_input, 0) * Time.fixedDeltaTime * local_velocity.z*20);
            }

            //using MoveRotation for physics handling
            rb.MoveRotation(rb.rotation * delta_rotation);

            //ollie 
            if (Input.GetKey("o")) {
                rb.AddForce(transform.up * pop * Time.fixedDeltaTime, ForceMode.Impulse);
            }
        } else {
            //in air

            //try to add smooth ollie steeze below
            /*
            float cur_angle = transform.eulerAngles.x;
            if (cur_angle < 20 || cur_angle > 340) {
                Quaternion delta_rotation = Quaternion.Euler(new Vector3(rb.velocity.y * -50f, 0, 0) * Time.fixedDeltaTime);
                rb.MoveRotation(rb.rotation * delta_rotation);
            } else {
                //disable until we hit the ground again
                rb.MoveRotation(Quaternion.Euler(Timetransform.eulerAngles + new Vector3(transform.eulerAngles.x, 0, 0)));
            }*/
        }

        //VISUAL EFFECTS

        //kickturn
        if (on_ground && !upside_down && Math.Abs(h_input) > 0 && Math.Abs(v_input) == 0 && Math.Abs(local_velocity.z) < kickturn_thresh) { //kickturn
            //gets a quaternion that uses the current angles about axis and rotates 15 less from x (tilt up)
            Quaternion kickturn_rotation = Quaternion.Euler(transform.eulerAngles + new Vector3(-15f, 0, 0));
            //to interpolate smoothly, adjusting delta time multiplier for faster
            board_visual.transform.rotation = Quaternion.Lerp(board_visual.transform.rotation, kickturn_rotation, 5f * Time.fixedDeltaTime);

            //move graphics slightly up
            board_visual.transform.position = Vector3.Lerp(board_visual.transform.position, transform.position + new Vector3(0, 0.02f, 0), 5f * Time.fixedDeltaTime);
        } else {
            //undo the kickturn visual
            Quaternion kickturn_rotation = Quaternion.Euler(transform.eulerAngles); //get quaternion representing the parent object (one this script is on) rotations
            board_visual.transform.rotation = Quaternion.Lerp(board_visual.transform.rotation, kickturn_rotation, 5f * Time.fixedDeltaTime);

            //align graphics back with collider
            board_visual.transform.position = Vector3.Lerp(board_visual.transform.position, transform.position, 5f * Time.fixedDeltaTime);
        }
        //board tilt
        if (on_ground && Math.Abs(h_input) > 0.1 && !upside_down) {
            //rotate h_input*20 from board visual rotation
            Quaternion deck_angle = Quaternion.Euler(board_visual.transform.eulerAngles + new Vector3(0, 0, -15f * h_input));
            deck.rotation = Quaternion.Lerp(deck.rotation, deck_angle, 5f * Time.fixedDeltaTime);
        } else {
            //reset back to board visual rotation
            Quaternion deck_angle = Quaternion.Euler(board_visual.transform.eulerAngles);
            deck.rotation = Quaternion.Lerp(deck.rotation, deck_angle, 5f * Time.fixedDeltaTime);
        }
    }

    void OnCollisionStay(Collision collision) {
        if (collision.gameObject.transform.parent.name == "Map") {
            on_ground = true;
            if (Vector3.Dot(transform.up, Vector3.down) > 0) { //up direction has overlap with down vector
                upside_down = true;
            }
        }
    }

    void OnCollisionExit(Collision collision) {
        if (collision.gameObject.transform.parent.name == "Map") {
            on_ground = false;
            upside_down = false;
        }
    }
}
