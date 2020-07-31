using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalPlatform : MonoBehaviour
{
    private PlatformEffector2D effector;
    private float waitTime;
    private bool isGoingDown;

    void Start()
    {
        effector = GetComponent<PlatformEffector2D>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.DownArrow)) {
            if (waitTime <= 0) {
                effector.rotationalOffset = 180f;
                waitTime = 0.1f;
                isGoingDown = true;
            }
        }

        if (waitTime <= 0 && isGoingDown == true) {
            effector.rotationalOffset = 0;
            isGoingDown = false;
        } else {
            waitTime -= Time.deltaTime;

        }
    }
}
