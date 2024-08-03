using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float h_input;
    public float v_input;
    private readonly float h_speed = 5f;
    private readonly float v_speed = 5f;
    private readonly float turn_speed = 75f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        h_input = Input.GetAxis("Horizontal");
        v_input = Input.GetAxis("Vertical");

        //move player based off of input vals
        transform.Translate(Vector3.forward * Time.deltaTime * v_input * v_speed);
        transform.Translate(Vector3.right * Time.deltaTime * h_input * h_speed);

        //rotate when we have horizontal input about up axis
        transform.Rotate(Vector3.up * Time.deltaTime * h_input * turn_speed);
    }
}
