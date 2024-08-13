using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using TreeEditor;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UIElements;
using UnityEngine.VFX;
using UnityEngine.XR;

public class SkateboardController : MonoBehaviour
{
    public GameObject board_visual;
    public bool on_ground = true;
    public bool upside_down = false; //on ground with up facing down

    private Rigidbody rb;
    private Transform deck;
    private readonly Vector3 gravity = new(0, -250f, 0);
    private readonly float sideways_friction = 15f;
    private readonly float max_speed = 7.5f;
    private readonly float kickturn_thresh = 2f;
    private readonly float kickturn_speed = 100f;
    private readonly float turn_speed = 15f;
    private readonly float pop = 50f;
    private readonly float steez = 25f;

    void Start()
    {   
        rb = GetComponent<Rigidbody>();
        deck = board_visual.transform.Find("Deck");
    }

    void FixedUpdate()
    {
        Vector3 local_velocity = transform.InverseTransformDirection(rb.velocity);
        float h_input = Input.GetAxis("Horizontal");
        float v_input = Input.GetAxis("Vertical");

        physics(local_velocity);
        inputs(h_input, v_input, local_velocity);
        vfx(h_input, v_input, local_velocity);
    }

    void physics(Vector3 local_velocity) {
        //applys custom physics like sideways friction and gravity

        //gravity
        rb.AddForce(gravity * Time.fixedDeltaTime, ForceMode.Acceleration);

        //add sideways friction for realistic turning when on ground
        if (on_ground && !upside_down) {
            float sideways_velocity = local_velocity.x;
            if (sideways_velocity > 0.3) {
                sideways_velocity -= sideways_friction * Time.fixedDeltaTime;
            } else if (sideways_velocity < -0.3) {
                sideways_velocity += sideways_friction * Time.fixedDeltaTime;
            } else {
                sideways_velocity = 0;
            }
            local_velocity.x = sideways_velocity; //set back

            rb.velocity = transform.TransformDirection(local_velocity); //set back to world rel
        }
    }

    void inputs(float h_input, float v_input, Vector3 local_velocity) {
        //controls player inputs like turning and moving

        if (upside_down) {
            if (Input.GetKey("o")) {
                rb.AddForce(Vector3.up * pop * Time.fixedDeltaTime, ForceMode.Impulse);
            }
        } else if (on_ground) {
            //ground movement
            //forward
            if (rb.velocity.magnitude < max_speed) {
                rb.AddForce(transform.forward * v_input * Time.fixedDeltaTime * 400, ForceMode.Acceleration);
            }

            //rotate rigid
            Quaternion delta_rotation;
            if (Math.Abs(local_velocity.z) < kickturn_thresh && Math.Abs(v_input) == 0) {
                delta_rotation = Quaternion.Euler(new Vector3(0, h_input, 0) * Time.fixedDeltaTime * kickturn_speed);
            } else {
                delta_rotation = Quaternion.Euler(new Vector3(0, h_input, 0) * Time.fixedDeltaTime * local_velocity.z * turn_speed);
            }

            //using MoveRotation for physics handling
            rb.MoveRotation(rb.rotation * delta_rotation);

            //ollie 
            if (Input.GetKey("o")) {
                rb.AddForce(transform.up * pop * Time.fixedDeltaTime, ForceMode.Impulse);
                rb.MoveRotation(rb.rotation * Quaternion.Euler(-(pop / 10f), 0, 0));

                //forward force to offset popping up while rotated backwards
                rb.AddForce(transform.forward * (pop/4) * Time.fixedDeltaTime, ForceMode.Impulse);
            }
        } else {
            //in air
            //board levels itself
            if (transform.eulerAngles.x != 0) {
                rb.MoveRotation(Quaternion.Lerp(rb.rotation, Quaternion.Euler(rb.rotation.eulerAngles + new Vector3(-rb.rotation.eulerAngles.x, 0, 0)), 5f * Time.fixedDeltaTime));
            }

            //kickflip and heelfip
            if (Input.GetKey("i")) {
                rb.MoveRotation(rb.rotation * Quaternion.Euler(new Vector3(0, 0, 1f) * Time.fixedDeltaTime * 300));
            } else if (Input.GetKey("p")) {
                rb.MoveRotation(rb.rotation * Quaternion.Euler(new Vector3(0, 0, -1f) * Time.fixedDeltaTime * 300));
            }

            //side to side movement
            rb.MoveRotation(Quaternion.Euler(rb.rotation.eulerAngles + (new Vector3(0, h_input, 0) * Time.fixedDeltaTime * 200)));
        }
    }

    void vfx(float h_input, float v_input, Vector3 local_velocity) {
        //controls the way the board looks, however, just the visual aspect, not the actual rigidbody

        //first set vfx rotation back to normal, unless kickturning/turning
        board_visual.transform.rotation = Quaternion.Lerp(board_visual.transform.rotation, Quaternion.Euler(transform.eulerAngles), 5f * Time.fixedDeltaTime); //set vfx to parent vals
        board_visual.transform.position = Vector3.Lerp(board_visual.transform.position, transform.position, 5f * Time.fixedDeltaTime); //position to parent

        //set deck to parent rotation by default
        deck.rotation = Quaternion.Lerp(deck.rotation, Quaternion.Euler(board_visual.transform.eulerAngles), 5f * Time.fixedDeltaTime);

        //change the above in case we want to change the board visual upon movement

        //kickturn
        if (on_ground && !upside_down && Math.Abs(h_input) > 0 && Math.Abs(v_input) == 0 && Math.Abs(local_velocity.z) < kickturn_thresh) { //kickturn
            //gets a quaternion that uses the current angles about axis and rotates 15 less from x (tilt up)
            Quaternion kickturn_rotation = Quaternion.Euler(transform.eulerAngles + new Vector3(-15f, 0, 0));
            //to interpolate smoothly, adjusting delta time multiplier for faster
            board_visual.transform.rotation = Quaternion.Lerp(board_visual.transform.rotation, kickturn_rotation, 5f * Time.fixedDeltaTime);

            //move graphics slightly up
            board_visual.transform.position = Vector3.Lerp(board_visual.transform.position, transform.position + new Vector3(0, 0.02f, 0), 5f * Time.fixedDeltaTime);
        }
        //board tilt
        if (on_ground && !upside_down && Math.Abs(h_input) > 0.1) {
            //rotate h_input*20 from board visual rotation
            Quaternion deck_angle = Quaternion.Euler(board_visual.transform.eulerAngles + new Vector3(0, 0, -15f * h_input));
            deck.rotation = Quaternion.Lerp(deck.rotation, deck_angle, 5f * Time.fixedDeltaTime);
        }

        if (!on_ground && !upside_down) {
            //can slightly tilt foward and back for steez
            if (v_input > 0) {
                board_visual.transform.rotation = Quaternion.Lerp(board_visual.transform.rotation, Quaternion.Euler(transform.eulerAngles + new Vector3(steez, 0, 0)), 5f * Time.fixedDeltaTime);
            } else if (v_input < 0) {
                board_visual.transform.rotation = Quaternion.Lerp(board_visual.transform.rotation, Quaternion.Euler(transform.eulerAngles + new Vector3(-steez, 0, 0)), 5f * Time.fixedDeltaTime);
            }
        }
    }

    //Colission functions (called by untiy)
    void OnCollisionStay(Collision collision) {
        ContactPoint[] contacts = collision.contacts;

        //check normal vectors to see if the object is colliding with something from below
        if (collision.gameObject.transform.parent.name == "Map") {
            for (int i=0; i < contacts.Length; i++) {
                if (contacts[i].thisCollider.name == "Front Truck") {
                    if (contacts[i].normal.y > 0.5) {
                        on_ground = true;
                    }
                } else if (contacts[i].thisCollider.name == "Back Truck") {
                    if (contacts[i].normal.y > 0.5) {
                        on_ground = true;
                    }
                }
            }

            if (Vector3.Dot(-transform.up, Vector3.down) > 0.5) {
                upside_down = false;
            } else {
                for (int i=0; i < contacts.Length; i++) {
                    if (contacts[i].thisCollider.name == "Deck") {
                        if (contacts[i].normal.y > 0.5) {
                            upside_down = true;
                        }
                    }
                }
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
