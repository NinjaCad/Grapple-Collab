using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	Rigidbody2D rb;
	DistanceJoint2D joint;

	RaycastHit2D raycast;

	float moveInput;
	float coyote;
	float buffer;
	bool isJumping;
	bool onGround;
	
	[Header("Run")]
	public float moveSpeed;
	public float acceleration;
	public float decceleration;
	public float velPower;
	[Space(5)]
	public float frictionAmount;

	[Header("Jump")]
	public float jumpForce;
	[Range(0f, 1)] public float jumpCutMultiplier;
	[Space(5)]
	public float coyoteTime;
	public float bufferTime;
	[Space(5)]
	public float gravityScale;
	public float fallGravityMultiplier;
	public float maxFallSpeed;
	
	[Header("Checks")]
	public Transform groundCheckPoint;
	public Vector2 groundCheckSize;
	public LayerMask groundLayer;

	[Header("Grapple")]
	public GameObject grapplePrefab;
	public float swingAcceleration;
	public float swingDecceleration;
	[HideInInspector] public Vector2 grapplePoint;
	[HideInInspector] public float grappleRadius;
	[HideInInspector] public bool isSwinging;
	[HideInInspector] public bool inGrappleMode;
	[Space(5)]
	public float minPullSpeed;
	[HideInInspector] public bool isPulling;
	[HideInInspector] public float timeToPoint;
	[HideInInspector] public float timeAfterPull;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	void Start()
	{
		joint = GetComponent<DistanceJoint2D>();
	}

	void Update()
	{
		#region Movement
		moveInput = Input.GetAxisRaw("Horizontal");
		if(moveInput != 0 && moveInput != Mathf.Sign(rb.velocity.x))
		{
			inGrappleMode = false;
		}

		coyote -= Time.deltaTime;
		buffer -= Time.deltaTime;
		
		if(Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0f, groundLayer))
		{
			coyote = coyoteTime;
			inGrappleMode = false;
			onGround = true;
		} else if(onGround == true)
		{
			onGround = false;
			OnGrappleRestart();
		}
		if(Input.GetButtonDown("Jump"))
		{
			buffer = bufferTime;
		}
		if(coyote > 0f && buffer > 0f && !isJumping)
		{
			Jump();
		}
		if(Input.GetButtonUp("Jump"))
		{
			OnJumpUp();
		}
		#endregion
		
		#region Grapple
		if(Input.GetMouseButtonDown(0) && !isPulling)
		{
			OnGrappleDown();
		}
		if(isSwinging)
		{
			Grapple();
		}
		if(isPulling)
		{
			Pull();
		}
		#endregion
	}

	void FixedUpdate()
	{
		if(!isPulling)
		{
			#region Run
			float targetSpeed = moveInput * moveSpeed;
			float speedDif = targetSpeed - rb.velocity.x;
			float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? (!inGrappleMode ? acceleration : swingAcceleration) : (!inGrappleMode ? decceleration : swingDecceleration);
			float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
			rb.AddForce(movement * Vector2.right);
			#endregion

			#region Friction
			if(coyote > 0 && Mathf.Abs(moveInput) == 0f)
			{
				float amount = Mathf.Min(Mathf.Abs(rb.velocity.x), Mathf.Abs(frictionAmount));
				amount *= Mathf.Sign(rb.velocity.x);
				rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
			}
			#endregion

			#region Jump Gravity
			if(rb.velocity.y < 0 && !isSwinging)
			{
				rb.gravityScale = gravityScale * fallGravityMultiplier;
				rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
				isJumping = false;
			} else
			{
				rb.gravityScale = gravityScale;
			}

			if(rb.velocity.y < 0 && isJumping)
			{
				isJumping = false;
				OnGrappleRestart();
			} else if(rb.velocity.y < 0)
			{
				isJumping = false;
			}
			#endregion
		}
	}

	void Jump()
	{
		rb.velocity = new Vector2(rb.velocity.x, jumpForce + (0.5f * Time.fixedDeltaTime * -gravityScale) / rb.mass);
		coyote = 0f;
		buffer = 0f;
		isJumping = true;
	}

	void OnJumpUp()
	{
		if(rb.velocity.y > 0 && isJumping)
		{
			rb.AddForce(Vector2.down * rb.velocity.y * (1 - jumpCutMultiplier), ForceMode2D.Impulse);
		}
		isJumping = false;
		buffer = 0f;
		OnGrappleRestart();
	}

	void Grapple()
	{
		if(!onGround && !isJumping)
		{
			inGrappleMode = true;
			if(Vector2.Distance(transform.position, grapplePoint) < grappleRadius)
			{
				grappleRadius = Vector2.Distance(transform.position, grapplePoint);
				joint.distance = grappleRadius;
			}
		} else
		{
			joint.enabled = false;
			inGrappleMode = false;
		}
		
		if(Input.GetMouseButtonUp(0))
		{
			isSwinging = false;
			joint.enabled = false;
		}
		if(Input.GetMouseButtonDown(1))
		{
			isJumping = false;
			isSwinging = false;
			isPulling = true;
			joint.enabled = false;
			rb.gravityScale = 0f;

			float radius = Mathf.Max(minPullSpeed, Vector2.Distance(Vector2.zero, rb.velocity));
			float theta = Mathf.Atan2(grapplePoint.y - transform.position.y, grapplePoint.x - transform.position.x);
			rb.velocity = new Vector2(radius * Mathf.Cos(theta), radius * Mathf.Sin(theta));
			timeToPoint = Vector2.Distance(grapplePoint, transform.position) / radius;
			timeAfterPull = 0.0f;
		}
	}

	void Pull()
	{
		timeAfterPull += Time.deltaTime;
		if(timeAfterPull >= timeToPoint)
		{
			isPulling = false;
		}
	}

	void OnGrappleRestart()
	{
		if(isSwinging)
		{
			joint.enabled = true;
			grappleRadius = Vector2.Distance(transform.position, grapplePoint);
			joint.distance = grappleRadius;
		}
	}

	void OnGrappleDown()
	{
		isSwinging = true;
		joint.enabled = true;
		grapplePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		joint.connectedAnchor = grapplePoint;
		grappleRadius = Vector2.Distance(transform.position, grapplePoint);
		joint.distance = grappleRadius;
		GameObject currentPrefab = Instantiate(grapplePrefab);
		currentPrefab.GetComponent<Rope>().player = this;
	}
}