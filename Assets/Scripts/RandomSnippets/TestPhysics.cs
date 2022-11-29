using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPhysics : MonoBehaviour
{
    public float forceFactor = 1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Camera cam = Camera.main;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.tag == "BouncingBall")
                {
                    if(hit.rigidbody != null)
                    {
                        hit.rigidbody.AddForceAtPosition(ray.direction * forceFactor, hit.point);
                    }
                }
            }
        }
    }
}
