using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	Rigidbody2D rb;
	DistanceJoint2D joint;
	Rope ropeScript;

	RaycastHit2D raycast;

	float moveInput;
	float coyote;
	float buffer;
	bool isJumping;
	bool onGround;

	[HideInInspector] public Vector2 grapplePoint;
	[HideInInspector] public float grappleRadius;
	[HideInInspector] public bool isHeld;
	[HideInInspector] public bool isGrappled;
	[HideInInspector] public bool inGrappleAccel;
	[HideInInspector] public bool isHanging;
	[HideInInspector] public float pullSpeed;
	[HideInInspector] public bool isPulling;

	Vector2 startPull;
	float startPullDistance;
	
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
	public LayerMask grappleLayers;
	[Space(5)]
	public float minPullSpeed;

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
			inGrappleAccel = false;
		}

		coyote -= Time.deltaTime;
		buffer -= Time.deltaTime;
		
		if(Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0f, groundLayer))
		{
			coyote = coyoteTime;
			onGround = true;
			inGrappleAccel = false;
		} else if(onGround == true)
		{
			onGround = false;
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
		if(Input.GetMouseButtonDown(0) && !isGrappled)
		{
			OnGrappleDown();
		}
		if(isGrappled)
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
			float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? (!inGrappleAccel ? acceleration : swingAcceleration) : (!inGrappleAccel ? decceleration : swingDecceleration);
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
			if(rb.velocity.y < 0 && !(isGrappled && isHanging))
			{
				rb.gravityScale = gravityScale * fallGravityMultiplier;
				rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
				isJumping = false;
			} else
			{
				rb.gravityScale = gravityScale;
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
	}

	void Grapple()
	{
		ropeScript.points[ropeScript.points.Count - 1].pastPos = ropeScript.points[ropeScript.points.Count - 1].currentPos;
		ropeScript.points[ropeScript.points.Count - 1].currentPos = transform.position;
		
		if(Input.GetMouseButtonUp(0))
		{
			isGrappled = false;
			joint.enabled = false;
			OnGrappleUp();
			return;
		}

		if(Vector2.Distance(transform.position, grapplePoint) >= grappleRadius - 0.1f && !onGround)
		{
			inGrappleAccel = true;
			isHanging = true;
		} else
		{
			isHanging = false;
		}

		if(Input.GetButtonDown("Pull") || Input.GetMouseButtonDown(1))
		{
			isJumping = false;
			isPulling = true;
			joint.enabled = false;
			rb.gravityScale = 0f;

			startPull = transform.position;
			startPullDistance = Vector2.Distance(transform.position, grapplePoint);
			if(Vector2.Distance(Vector2.zero, rb.velocity) >= minPullSpeed)
			{
				pullSpeed = Vector2.Distance(Vector2.zero, rb.velocity);
			} else
			{
				pullSpeed = minPullSpeed;
			}
		}
	}

	void Pull()
	{
		if(Vector2.Distance(transform.position, startPull) >= startPullDistance - 0.5f)
		{
			isPulling = false;
			isGrappled = false;
			inGrappleAccel = true;
			return;
		}
		
		Vector2 targetSpeed = Vector2.LerpUnclamped(transform.position, grapplePoint, pullSpeed / Vector2.Distance(transform.position, grapplePoint));
		Vector2 speedDif = targetSpeed - rb.velocity;
		rb.AddForce(Vector2.Lerp(Vector2.zero, speedDif, 0.5f), ForceMode2D.Impulse);

		
	}

	void OnGrappleDown()
	{
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		float angle = Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x);
		Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
		RaycastHit2D raycast = Physics2D.Raycast(transform.position, direction, 50f, grappleLayers);
		if(raycast.collider != null)
		{
			isGrappled = true;
			TargetPosition hitCollider = raycast.collider.gameObject.GetComponentInParent<TargetPosition>();
			grapplePoint = new Vector2(Mathf.Clamp(raycast.point.x, hitCollider.gameObject.transform.position.x + hitCollider.minGrappleBounds.x, hitCollider.gameObject.transform.position.x + hitCollider.maxGrappleBounds.x), Mathf.Clamp(raycast.point.y, hitCollider.gameObject.transform.position.y + hitCollider.minGrappleBounds.y, hitCollider.gameObject.transform.position.y + hitCollider.maxGrappleBounds.y));
			joint.connectedAnchor = grapplePoint;
			joint.enabled = true;
			grappleRadius = Vector2.Distance(transform.position, grapplePoint);
			joint.distance = grappleRadius;
			CreateRope();
		}
	}

	void OnGrappleUp()
	{
		ropeScript.points[ropeScript.points.Count - 1].isLocked = false;
		for(int i = 0; i < ropeScript.lines.Count; i++)
		{
			ropeScript.lines[i].lineLength = Vector2.Distance(ropeScript.points[ropeScript.lines[i].pointIndexes.x].currentPos, ropeScript.points[ropeScript.lines[i].pointIndexes.y].currentPos);
		}
	}

	void CreateRope()
	{
		GameObject currentPrefab = Instantiate(grapplePrefab);
		ropeScript = currentPrefab.GetComponent<Rope>();
		ropeScript.player = this;

		ropeScript.points.Add(new Point());
		ropeScript.points[0].currentPos = grapplePoint;
		ropeScript.points[0].isLocked = true;

		for(float i = 0.5f; i < grappleRadius; i += 0.5f)
		{
			ropeScript.points.Add(new Point());
			ropeScript.points[(int) (i * 2)].currentPos = Vector2.Lerp(grapplePoint, transform.position, i / grappleRadius);
			ropeScript.lines.Add(new Line());
			ropeScript.lines[(int) (i * 2) - 1].pointIndexes = new Vector2Int((int) (i * 2), (int) (i * 2) - 1);
			ropeScript.lines[(int) (i * 2) - 1].lineLength = 0.3f;
		}

		ropeScript.points.Add(new Point());
		ropeScript.points[ropeScript.points.Count - 1].currentPos = transform.position;
		ropeScript.points[ropeScript.points.Count - 1].isLocked = true;
		ropeScript.lines.Add(new Line());
		ropeScript.lines[ropeScript.lines.Count - 1].pointIndexes = new Vector2Int(ropeScript.points.Count - 1, ropeScript.points.Count - 2);
		ropeScript.lines[ropeScript.lines.Count - 1].lineLength = grappleRadius % 0.5f;
	}
}