using UnityEngine;

public class WheelCollision : MonoBehaviour
{
    public SkateboardController skate_script;
    private string cur;
    
    void Start() {
        cur = transform.name;
    }

    void OnTriggerStay(Collider collider) {
        if (cur == "Front Left") {
            skate_script.front_left = true;
        } else if (cur == "Front Right") {
            skate_script.front_right = true;
        } else if (cur == "Back Left") {
            skate_script.back_left = true;
        } else if (cur == "Back Right") {
            skate_script.back_right = true;
        }
    }

    void OnTriggerExit(Collider collider) {
        if (cur == "Front Left") {
            skate_script.front_left = false;
        } else if (cur == "Front Right") {
            skate_script.front_right = false;
        } else if (cur == "Back Left") {
            skate_script.back_left = false;
        } else if (cur == "Back Right") {
            skate_script.back_right = false;
        }
    }
}
