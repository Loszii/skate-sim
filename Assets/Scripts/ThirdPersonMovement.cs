using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform cam;
    public float speed = 6f;
    public float turn_smooth_time = 0.1f;
    float turn_smooth_velocity;

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // [-1, 1] range
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized; //normalized for unit vector

        if (direction.magnitude > 0) {
            float target_angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y; //arctan of dir vector to get turning angle
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, target_angle, ref turn_smooth_velocity, turn_smooth_time);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 move_dir = Quaternion.Euler(0f, target_angle, 0f) * Vector3.forward;
            controller.Move(move_dir.normalized * speed * Time.deltaTime);
        }
    }
}
