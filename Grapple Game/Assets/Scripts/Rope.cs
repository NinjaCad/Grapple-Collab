using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Rope : MonoBehaviour
{
    LineRenderer lineRenderer;

    public List<Point> points = new List<Point>();
    public List<Line> lines = new List<Line>();

    bool isDetached;
    float timeAfterDetached;
    
    [Header("Swing")]
    [SerializeField] float gravityScale;
    
    [SerializeField] float fullTime;
    [SerializeField] float invisTime;
    
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Start()
    {
        for (int i = 0; i < points.Count; i++)
        {
            points[i].pastPos = points[i].currentPos;
        }
        
        lineRenderer.startWidth = 0.15f;
        lineRenderer.endWidth = 0.15f;
        timeAfterDetached = 0.0f;

        lineRenderer.positionCount = points.Count;
    }

    void Update()
    {
        #region Update Renderer Position
        Vector3[] pointPositions = new Vector3[points.Count];
        for (int i = 0; i < pointPositions.Length; i++)
        {
            pointPositions[i] = points[i].currentPos;
        }
        lineRenderer.SetPositions(pointPositions);
        #endregion
        
        if (!isDetached) { return; }
        
        timeAfterDetached += Time.deltaTime;

        if (timeAfterDetached > fullTime) { Destroy(this.gameObject); }
    }

    void FixedUpdate()
    {
        #region Change Velocity
        for (int i = 0; i < points.Count; i++)
        {
            if (!points[i].isLocked)
            {
                Vector2 newPos = points[i].currentPos;
                newPos += new Vector2(points[i].currentPos.x - points[i].pastPos.x, points[i].currentPos.y - points[i].pastPos.y - (Time.fixedDeltaTime * gravityScale));
                points[i].pastPos = points[i].currentPos;
                points[i].currentPos = newPos;
            }
        }
        #endregion
        
        #region Find Positions
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < lines.Count; j++)
            {
                int2 pointIndexes = lines[j].pointIndexes;
                if (!points[pointIndexes[0]].isLocked)
                {
                    float radius = Vector2.Distance(points[pointIndexes[0]].currentPos, points[pointIndexes[1]].currentPos) - lines[j].lineLength;
                    radius /= points[pointIndexes[1]].isLocked ? 1 : 2;
                    Vector2 direction = (points[pointIndexes[1]].currentPos - points[pointIndexes[0]].currentPos).normalized;
                    points[pointIndexes[0]].currentPos += radius * direction;
                }
                if (!points[pointIndexes[1]].isLocked)
                {
                    float radius = Vector2.Distance(points[pointIndexes[1]].currentPos, points[pointIndexes[0]].currentPos) - lines[j].lineLength;
                    Vector2 direction = (points[pointIndexes[0]].currentPos - points[pointIndexes[1]].currentPos).normalized;
                    points[pointIndexes[1]].currentPos += radius * direction;
                }
            }
        }
        #endregion
    }

    public void DetachRope()
    {
        isDetached = true;
        timeAfterDetached = 0.0f;
    }
}

[System.Serializable]
public class Point
{
    public Vector2 currentPos;
    public Vector2 pastPos;
    public bool isLocked;
}

[System.Serializable]
public class Line
{
    public int2 pointIndexes;
    public float lineLength;
}
