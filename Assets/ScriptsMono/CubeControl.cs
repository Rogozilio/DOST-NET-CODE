using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeControl : MonoBehaviour
{
    private void Update()
    {
        var left = Input.GetKey("left") ? -1 : 0;
        var right = Input.GetKey("right") ? 1 : 0;
        var down = Input.GetKey("down") ? -1 : 0;
        var up = Input.GetKey("up") ? 1 : 0;
        bool isControlledCube = UnityEngine.Input.GetKeyUp("space");

        var speed = Time.deltaTime * 4f;
        var moveInput = new Vector2(left + right, down + up) * speed;
        transform.position += new Vector3(moveInput.x, 0, moveInput.y);
    }
}
