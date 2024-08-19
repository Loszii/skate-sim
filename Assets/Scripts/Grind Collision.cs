using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrindCollision : MonoBehaviour
{   
    public SkateboardController skate_script;
    private string cur;

    void Start()
    {
        cur = transform.name;
    }

    void OnTriggerStay(Collider collider) {
        if (collider.name == "Grindable" && !skate_script.is_grinding) {
            if (cur == "Front Truck") {
                skate_script.front_truck_grind = true;
            } else if (cur == "Back Truck") {
                skate_script.back_truck_grind = true;
            } else if (cur == "Board Slide") {
                skate_script.board_slide_grind = true;
            } else if (cur == "Nose Slide") {
                skate_script.nose_slide_grind = true;
            } else if (cur == "Tail Slide") {
                skate_script.tail_slide_grind = true;
            } 

            skate_script.rb.angularVelocity = new Vector3(0, 0, 0);
            skate_script.grind_object = collider.gameObject;
            skate_script.grind_rotation = skate_script.transform.rotation;
            skate_script.grind_speed = Vector3.Dot(skate_script.rb.velocity, skate_script.grind_object.transform.forward);
        }
    }
    
    void OnTriggerExit(Collider collider) {
        if (collider.name == "Grindable") {
            if (cur == "Front Truck") {
                skate_script.front_truck_grind = false;
            } else if (cur == "Back Truck") {
                skate_script.back_truck_grind = false;
            } else if (cur == "Board Slide") {
                skate_script.board_slide_grind = false;
            } else if (cur == "Nose Slide") {
                skate_script.nose_slide_grind = false;
            } else if (cur == "Tail Slide") {
                skate_script.tail_slide_grind = false;
            } 
        }
    }
}
