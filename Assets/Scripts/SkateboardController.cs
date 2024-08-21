using System;
using TreeEditor;
using UnityEngine;

public class SkateboardController : MonoBehaviour
{
    public GameObject board_visual;
    //air
    public bool on_ground = true; //all 4 wheels touching
    public bool in_air = false; //none touching
    
    //ground
    public bool front_left = true;
    public bool front_right = true;
    public bool back_left = true;
    public bool back_right = true;

    //grinds
    public bool is_grinding = false;
    public bool front_truck_grind = false;
    public bool back_truck_grind = false;
    public bool board_slide_grind = false;
    public bool nose_slide_grind = false;
    public bool tail_slide_grind = false;
    public GameObject grind_object;
    public Quaternion grind_rotation;
    public float grind_speed;

    //physics
    public Rigidbody rb;
    
    private Transform deck;
    private readonly Vector3 gravity = new(0, -250f, 0);
    private readonly float sideways_friction = 15f;
    private readonly float max_speed = 6f;
    private readonly float kickturn_thresh = 2f;
    private readonly float kickturn_speed = 150f;
    private readonly float turn_speed = 15f;
    private readonly float pop = 60f;
    private readonly float steez = 30f;
    private readonly float flip_speed = 360f;

    void Start()
    {   
        rb = GetComponent<Rigidbody>();
        deck = board_visual.transform.Find("Deck");
    }

    void FixedUpdate()
    {
        is_grinding = front_truck_grind || back_truck_grind || board_slide_grind || nose_slide_grind || tail_slide_grind;
        on_ground = front_left & front_right && back_left && back_right;
        in_air = !front_left & !front_right && !back_left && !back_right;


        Vector3 local_velocity = transform.InverseTransformDirection(rb.velocity);
        float h_input = Input.GetAxis("Horizontal");
        float v_input = Input.GetAxis("Vertical");

        if (is_grinding) {
            //slide through the grindable collider
            rb.velocity = grind_object.transform.forward * grind_speed;
            transform.rotation = grind_rotation;

            //pop out of grinds
            float pop = 200f; //override with more pop
            if (Input.GetKey("o")) {
                rb.AddForce(Vector3.up * pop * Time.fixedDeltaTime, ForceMode.Impulse);
            }
        } else {
            physics(local_velocity);
            inputs(h_input, v_input, local_velocity);
            vfx(h_input, v_input, local_velocity);
        }
    }

    void physics(Vector3 local_velocity) {
        //applys custom physics like sideways friction and gravity

        //gravity
        rb.AddForce(gravity * Time.fixedDeltaTime, ForceMode.Acceleration);

        //add sideways friction for realistic turning when on ground
        if (on_ground) {
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

        if (on_ground) {
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
        } else if (in_air) {
            //air movement

            //steez (front and back tilt)
            Quaternion air_tilt = Quaternion.Euler(new Vector3(0, rb.rotation.eulerAngles.y, rb.rotation.eulerAngles.z)); //level

            if (v_input > 0) {
                air_tilt = Quaternion.Euler(new Vector3(steez, rb.rotation.eulerAngles.y, rb.rotation.eulerAngles.z));
            } else if (v_input < 0) {
                air_tilt = Quaternion.Euler(new Vector3(-steez, rb.rotation.eulerAngles.y, rb.rotation.eulerAngles.z));
            }

            rb.MoveRotation(Quaternion.Lerp(rb.rotation, air_tilt, 2f * Time.fixedDeltaTime));


            //side to side movement
            rb.MoveRotation(Quaternion.Euler(new Vector3(0, h_input, 0) * Time.fixedDeltaTime * 200) * rb.rotation);

            //kickflip and heelfip
            if (Input.GetKey("i")) {
                rb.MoveRotation(rb.rotation * Quaternion.Euler(new Vector3(0, 0, 1f) * Time.fixedDeltaTime * flip_speed));
            } else if (Input.GetKey("p")) {
                rb.MoveRotation(rb.rotation * Quaternion.Euler(new Vector3(0, 0, -1f) * Time.fixedDeltaTime * flip_speed));
            }
        } else {
            //on side so can rotate back upright
            if (Input.GetKey("i")) {
                rb.MoveRotation(rb.rotation * Quaternion.Euler(new Vector3(0, 0, 1f) * Time.fixedDeltaTime * flip_speed));
            } else if (Input.GetKey("p")) {
                rb.MoveRotation(rb.rotation * Quaternion.Euler(new Vector3(0, 0, -1f) * Time.fixedDeltaTime * flip_speed));
            }
        }

        //ollie 
        if (Input.GetKey("o") && back_left && back_right) {
            rb.AddForce(Vector3.up * pop * Time.fixedDeltaTime, ForceMode.Impulse);
            rb.MoveRotation(rb.rotation * Quaternion.Euler(-(pop / 10f), 0, 0));
        }
    }

    void vfx(float h_input, float v_input, Vector3 local_velocity) {
        //controls the way the board looks, however, just the visual aspect, not the actual rigidbody

        //first set vfx rotation back to normal, unless kickturning/turning
        board_visual.transform.rotation = Quaternion.Lerp(board_visual.transform.rotation, transform.rotation, 5f * Time.fixedDeltaTime); //set vfx to parent vals
        board_visual.transform.position = Vector3.Lerp(board_visual.transform.position, transform.position, 5f * Time.fixedDeltaTime); //position to parent

        //set deck to parent rotation by default
        deck.rotation = Quaternion.Lerp(deck.rotation, board_visual.transform.rotation, 5f * Time.fixedDeltaTime);

        //change the above in case we want to change the board visual upon movement

        //kickturn
        if (on_ground && Math.Abs(h_input) > 0 && Math.Abs(v_input) == 0 && Math.Abs(local_velocity.z) < kickturn_thresh) { //kickturn
            //gets a quaternion that uses the current angles about axis and rotates 15 less from x (tilt up)
            Quaternion kickturn_rotation = Quaternion.Euler(new Vector3(-15f, 0, 0));
            //to interpolate smoothly, adjusting delta time multiplier for faster
            board_visual.transform.rotation = Quaternion.Lerp(board_visual.transform.rotation, transform.rotation * kickturn_rotation, 5f * Time.fixedDeltaTime);

            //move graphics slightly up
            board_visual.transform.position = Vector3.Lerp(board_visual.transform.position, transform.position + new Vector3(0, 0.04f, 0), 5f * Time.fixedDeltaTime);
        }
        //board tilt
        if (on_ground && Math.Abs(h_input) > 0.1) {
            //rotate h_input*20 from board visual rotation
            Quaternion deck_angle = Quaternion.Euler(new Vector3(0, 0, -30f * h_input));
            deck.rotation = Quaternion.Lerp(deck.rotation, board_visual.transform.rotation * deck_angle, 5f * Time.fixedDeltaTime);
        }
    }

    //Colission functions (called by untiy)
    void OnCollisionExit(Collision collision) {
        //remove rotations from collision
        rb.angularVelocity = new Vector3(0, 0, 0);
    }
}
