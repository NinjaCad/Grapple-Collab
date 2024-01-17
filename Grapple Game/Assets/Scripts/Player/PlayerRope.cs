using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerRope : PlayerSystem
{
    Rope ropeScript;
    
    void Update()
    {
        if (ropeScript == null) { return; }

        ropeScript.points[0].pastPos = ropeScript.points[0].currentPos;
		ropeScript.points[0].currentPos = transform.position;
    }

    void CreateNewRope(Vector2 grapplePoint)
    {
        if (ropeScript != null) { DetachRope(); }
        
        GameObject currentPrefab = Instantiate(player.data.grapplePrefab);
		ropeScript = currentPrefab.GetComponent<Rope>();

		float grappleRadius = Vector2.Distance(grapplePoint, transform.position);
        ropeScript.points.Add(new Point());
		ropeScript.points[0].currentPos = transform.position;
		ropeScript.points[0].isLocked = true;

		for (float i = 0.5f; i < grappleRadius; i += 0.5f)
		{
			ropeScript.points.Add(new Point());
			ropeScript.points[(int) (i * 2)].currentPos = Vector2.Lerp(transform.position, grapplePoint, i / grappleRadius);
			ropeScript.lines.Add(new Line());
			ropeScript.lines[(int) (i * 2) - 1].pointIndexes = new int2((int) (i * 2), (int) (i * 2) - 1);
			ropeScript.lines[(int) (i * 2) - 1].lineLength = 0.3f;
		}

		ropeScript.points.Add(new Point());
		ropeScript.points[ropeScript.points.Count - 1].currentPos = grapplePoint;
		ropeScript.points[ropeScript.points.Count - 1].isLocked = true;
		ropeScript.lines.Add(new Line());
		ropeScript.lines[ropeScript.lines.Count - 1].pointIndexes = new int2(ropeScript.points.Count - 1, ropeScript.points.Count - 2);
		ropeScript.lines[ropeScript.lines.Count - 1].lineLength = grappleRadius % 0.5f;
    }

    void DetachRope()
    {
        if (ropeScript == null) { return; }

        ropeScript.points[0].isLocked = false;
		for (int i = 0; i < ropeScript.lines.Count; i++)
		{
			ropeScript.lines[i].lineLength = Vector2.Distance(
                ropeScript.points[ropeScript.lines[i].pointIndexes[0]].currentPos,
                ropeScript.points[ropeScript.lines[i].pointIndexes[1]].currentPos);
		}

        ropeScript.DetachRope();
        ropeScript = null;
    }
    
    void OnEnable()
    {
        player.events.OnGrapple += CreateNewRope;
        player.events.OnGrappleButtonUp += DetachRope;
    }

    void OnDisable()
    {
        player.events.OnGrapple -= CreateNewRope;
        player.events.OnGrappleButtonUp -= DetachRope;
    }
}
