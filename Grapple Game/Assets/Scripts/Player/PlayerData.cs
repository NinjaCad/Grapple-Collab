using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PlayerData", order = 1)]
public class PlayerData : ScriptableObject
{
    [Header("Run")]
	public float moveSpeed;
	public float acceleration;
	public float decceleration;
	public float velPower;
	[Space(5)]
	public float frictionAmount;

	[Header("Jump")]
	public float jumpForce;
	public Vector2 wallJumpForce;
	[Range(0f, 1)] public float jumpCutMultiplier;
	public float wallJumpDecceleration;
	[Space(5)]
	public float coyoteTime;
	public float wallCoyoteTime;
	public float bufferTime;
	public float jumpDelayTime;
	[Space(5)]
	public float gravityScale;
	public float fallGravityMultiplier;
	public float maxFallSpeed;
	public float fastFallMultiplier;
	[Space(5)]
	public float wallSlideGravity;
	public float startWallSlideSpeed;
	public float maxWallSlideSpeed;
	
	[Header("Checks")]
	public Vector2 groundCheckPoint;
	public Vector2 groundCheckSize;
	public LayerMask groundLayer;
	[Space(5)]
	public Vector2 leftCheckPoint;
	public Vector2 rightCheckPoint;
	public Vector2 wallCheckSize;
	public LayerMask wallLayer;
	[Space(5)]
	public Vector2 ceilingCheckPoint;
	public Vector2 ceilingCheckSize;
	public LayerMask ceilingLayer;

	[Header("Grapple")]
	public GameObject grapplePrefab;
	public float swingAcceleration;
	public float swingDecceleration;
	public float grappleRange;
	public LayerMask grappleLayers;
	[Space(5)]
	public float minPullSpeed;

	[Header("Checkpoint")]
	public Vector2 checkpointCheckSize;
	public LayerMask checkpointLayer;

	[Header("Hazards")]
	public Vector2 hazardCheckSize;
	public LayerMask hazardLayer;
}
