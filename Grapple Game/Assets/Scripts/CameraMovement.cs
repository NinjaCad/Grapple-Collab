using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    GameObject target;
    Vector3 targetPosition;
    float smoothTime;
    Vector3 velocity;

    public List<Area> allAreas = new List<Area>();
    int currentArea;
    public bool showAreas = true;

    void Awake()
    {
        target = GameObject.FindWithTag("Player");
    }
    
    void Start()
    {
        velocity = Vector3.zero;
        smoothTime = 0.1f;
    }

    void Update()
    {
        targetPosition = new Vector3(target.transform.position.x, target.transform.position.y, -10);
        
        if (currentArea == -1 || !(targetPosition.x >= allAreas[currentArea].playerBounds[0].x && targetPosition.x < allAreas[currentArea].playerBounds[1].x
            && targetPosition.y >= allAreas[currentArea].playerBounds[0].y && targetPosition.y < allAreas[currentArea].playerBounds[1].y))
        {
            currentArea = FindCurrentArea(targetPosition);
        }

        if (currentArea != -1)
        {
            targetPosition = new Vector3(Mathf.Clamp(targetPosition.x, allAreas[currentArea].cameraBounds[0].x, allAreas[currentArea].cameraBounds[1].x),
                Mathf.Clamp(targetPosition.y, allAreas[currentArea].cameraBounds[0].y, allAreas[currentArea].cameraBounds[1].y), -10);
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    int FindCurrentArea(Vector2 targetPosition)
    {
        for (int i = 0; i < allAreas.Count; i++)
        {
            if (targetPosition.x >= allAreas[i].playerBounds[0].x && targetPosition.x < allAreas[i].playerBounds[1].x
                && targetPosition.y >= allAreas[i].playerBounds[0].y && targetPosition.y < allAreas[i].playerBounds[1].y)
            {
                return i;
            }
        }
        return -1;
    }

    void OnDrawGizmos()
    {
        if (!showAreas) { return; }
        
        for (int i = 0; i < allAreas.Count; i++)
        {
            Gizmos.color = Color.green;
            DrawBox(allAreas[i].cameraBounds);

            Gizmos.color = Color.white;
            DrawBox(allAreas[i].playerBounds);
        }
    }

    void DrawBox(Vector2[] points)
    {
        Gizmos.DrawLine(points[0], new Vector2(points[0].x, points[1].y));
        Gizmos.DrawLine(new Vector2(points[0].x, points[1].y), points[1]);
        Gizmos.DrawLine(points[1], new Vector2(points[1].x, points[0].y));
        Gizmos.DrawLine(new Vector2(points[1].x, points[0].y), points[0]);
    }
}

[System.Serializable]
public class Area
{
    public Vector2[] playerBounds = new Vector2[2];
    public Vector2[] cameraBounds = new Vector2[2];
}
