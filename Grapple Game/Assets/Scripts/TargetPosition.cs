using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPosition : MonoBehaviour
{
    public Vector2 grappleBounds;

    void Start()
    {
        grappleBounds = transform.position;
    }
}
