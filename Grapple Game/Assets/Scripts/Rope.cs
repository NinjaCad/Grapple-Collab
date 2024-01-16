using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    [HideInInspector] public Player player;
    LineRenderer lineRenderer;

    public List<Point> points = new List<Point>();
    public List<Line> lines = new List<Line>();

    [Header("Swing")]
    [SerializeField] float gravityScale;
    
    bool isReleased;
    [SerializeField] float fullTime;
    [SerializeField] float invisTime;
    float timeAfterRelease;

    Vector2 ownGrapplePoint;

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
        timeAfterRelease = 0.0f;
    }

    void Update()
    {
        Vector3[] pointPositions = new Vector3[points.Count];
        for (int i = 0; i < pointPositions.Length; i++)
        {
            pointPositions[i] = points[i].currentPos;
        }
        lineRenderer.positionCount = pointPositions.Length;
        lineRenderer.SetPositions(pointPositions);
        
        // if (!player.isGrappled && isReleased == false) { isReleased = true; }

        if (isReleased)
        {
            timeAfterRelease += Time.deltaTime;
            
            if (timeAfterRelease > fullTime) { Destroy(this.gameObject); }
        }
    }

    void FixedUpdate()
    {
        ChangeVelocity();
        FindPositions();
    }

    void ChangeVelocity()
    {
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
    }

    void FindPositions()
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < lines.Count; j++)
            {
                FindPos(j, lines[j].pointIndexes.x, lines[j].pointIndexes.y);
            }
        }
    }

    void FindPos(int lineIndex, int pointIndex1, int pointIndex2)
    {
        if (!points[pointIndex1].isLocked)
        {
            float radius = (Vector2.Distance(points[pointIndex1].currentPos, points[pointIndex2].currentPos) - lines[lineIndex].lineLength) / (points[pointIndex2].isLocked ? 1 : 2);
            float theta = Mathf.Atan2(points[pointIndex1].currentPos.y - points[pointIndex2].currentPos.y, points[pointIndex1].currentPos.x - points[pointIndex2].currentPos.x);
            points[pointIndex1].currentPos += new Vector2(-radius * Mathf.Cos(theta), -radius * Mathf.Sin(theta));
        }
        if (!points[pointIndex2].isLocked)
        {
            float radius = Vector2.Distance(points[pointIndex2].currentPos, points[pointIndex1].currentPos) - lines[lineIndex].lineLength;
            float theta = Mathf.Atan2(points[pointIndex2].currentPos.y - points[pointIndex1].currentPos.y, points[pointIndex2].currentPos.x - points[pointIndex1].currentPos.x);
            points[pointIndex2].currentPos += new Vector2(-radius * Mathf.Cos(theta), -radius * Mathf.Sin(theta));
        }
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
    public Vector2Int pointIndexes;
    public float lineLength;
}
