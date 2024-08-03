using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {   transform.rotation = player.transform.rotation; //set rotation
        transform.position = player.transform.position - (player.transform.forward * 10);
        transform.position += new Vector3(0, 1.5f, 0);
    }
}
