using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    public Player player;
    LineRenderer lineRenderer;

    bool isCut;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Start()
    {
        lineRenderer.startWidth = 0.25f;
        lineRenderer.endWidth = 0.25f;
    }

    void Update()
    {
        if(player.isSwinging == false)
        {
            isCut = true;
        }

        if(isCut == false)
        {
            lineRenderer.SetPositions(new Vector3[] {player.grapplePoint, player.transform.position});
        } else
        {
            Destroy(this.gameObject);
        }
    }
}
