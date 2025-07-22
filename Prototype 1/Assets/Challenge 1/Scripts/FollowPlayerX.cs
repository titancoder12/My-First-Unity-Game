using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayerX : MonoBehaviour
{
    public GameObject plane;
    private Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        offset = new Vector3(0, 4, -10);
        transform.position = plane.transform.position + offset;
        //plane.Find(Propellor).Rotate(Vector3.forward * 100 * Time.deltaTime);
        // Rotate the propellor at a constant speed
    }
}
