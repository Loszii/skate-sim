using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkateboardMovement : MonoBehaviour
{
    public float h_input;
    public float v_input;
    private bool on_ground = true;
    private readonly float speed = 50f;
    private readonly float turn_speed = 75f;
    private readonly float air_turn_speed = 150f;
    private readonly float jump_height = 10f;
    private readonly float max_speed = 20f;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        h_input = Input.GetAxis("Horizontal");
        v_input = Input.GetAxis("Vertical");

        //move player foward based off of vertical input values
        if (rb.velocity.magnitude < max_speed && on_ground) {
            rb.AddForce(transform.forward * Time.deltaTime * v_input * speed, ForceMode.VelocityChange);
        }

        //rotate when we have horizontal input about up axis
        if (on_ground) {
            transform.Rotate(Vector3.up * Time.deltaTime * h_input * turn_speed);
        } else {
            transform.Rotate(Vector3.up * Time.deltaTime * h_input * air_turn_speed);
        }

        //jump force
        if (Input.GetButtonDown("Jump") && on_ground) {
            rb.AddForce(new Vector3(0, jump_height, 0), ForceMode.Impulse);
            on_ground = false;
        }
    }

    //detect if player is touching ground
    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.name == "Floor") {
            on_ground = true;
        }
    }
}
