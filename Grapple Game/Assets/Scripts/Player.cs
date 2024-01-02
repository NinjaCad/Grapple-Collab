using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	Rigidbody2D rb;
	DistanceJoint2D joint;
	Rope ropeScript;

	float moveInput;
	float coyote;
	float rightCoyote;
	float leftCoyote;
	float buffer;
	float jumpDelay;
	bool isJumping;
	bool onGround;
	int onWall;
	bool isClinging;
	bool resetVelocity;
	int inWallJump;

	[HideInInspector] public Vector2 grapplePoint;
	[HideInInspector] public float grappleRadius;
	[HideInInspector] public bool isGrappled;
	[HideInInspector] public bool inGrappleAccel;
	[HideInInspector] public bool isHanging;
	
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
	[Space(5)]
	public float wallSlideGravity;
	public float startWallSlideSpeed;
	public float maxWallSlideSpeed;
	
	[Header("Checks")]
	public Transform groundCheckPoint;
	public Vector2 groundCheckSize;
	public LayerMask groundLayer;
	[Space(5)]
	public Transform leftCheckPoint;
	public Transform rightCheckPoint;
	public Vector2 wallCheckSize;
	public LayerMask wallLayer;

	[Header("Grapple")]
	public GameObject grapplePrefab;
	public float swingAcceleration;
	public float swingDecceleration;
	public float grappleRange;
	public LayerMask grappleLayers;
	[Space(5)]
	public float minPullSpeed;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		joint = GetComponent<DistanceJoint2D>();
	}

	void Update()
	{
		#region Movement
		moveInput = Input.GetAxisRaw("Horizontal");

		if (inWallJump != 0 && inWallJump == moveInput) { moveInput = 0; }

		if (moveInput == -Mathf.Sign(rb.velocity.x)) { inGrappleAccel = false; }

		coyote -= Time.deltaTime;
		rightCoyote -= Time.deltaTime;
		leftCoyote -= Time.deltaTime;
		buffer -= Time.deltaTime;
		jumpDelay -= Time.deltaTime;

		onWall = Physics2D.OverlapBox(leftCheckPoint.position, wallCheckSize, 0f, wallLayer) ? -1 :
			Physics2D.OverlapBox(rightCheckPoint.position, wallCheckSize, 0f, wallLayer) ? 1 : 0;
		isClinging = onWall != 0 && moveInput == onWall;

		if (onWall == -1 && isClinging) { leftCoyote = wallCoyoteTime; }
		else if (onWall == 1 && isClinging) { rightCoyote = wallCoyoteTime; }
		
		if (isClinging && rb.velocity.y < 0 && !(isGrappled && isHanging))
		{
			if (!resetVelocity)
			{
				rb.velocity = new Vector2(rb.velocity.x, -startWallSlideSpeed);
				resetVelocity = true;
			}
		}
		else { resetVelocity = false; }
		
		if (Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0f, groundLayer))
		{
			coyote = coyoteTime;
			onGround = true;
			inGrappleAccel = false;
		}
		else if (onGround) { onGround = false; }
		
		if (Input.GetButtonDown("Jump")) { buffer = bufferTime; }
		
		if (buffer > 0f && jumpDelay <= 0f && !isJumping)
		{
			if (coyote > 0f) { Jump(); }
			else if (rightCoyote > 0f || leftCoyote > 0f)
			{
				if (leftCoyote > rightCoyote) { WallJump(1); }
				else { WallJump(-1); }
			}
		}
		
		if (Input.GetButtonUp("Jump")) { OnJumpUp(); }
		#endregion
		
		#region Grapple
		if (Input.GetMouseButtonDown(0) && !isGrappled) { OnGrappleDown(); }
		
		if (isGrappled) { Grapple(); }
		#endregion
	}

	void FixedUpdate()
	{
		#region Run
		float targetSpeed = moveInput * moveSpeed;
		float speedDif = targetSpeed - rb.velocity.x;
		float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? (!inGrappleAccel ? acceleration : swingAcceleration) : (inWallJump == 0 ? (!inGrappleAccel ? decceleration : swingDecceleration) : wallJumpDecceleration);
		float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
		rb.AddForce(movement * Vector2.right);
		#endregion

		#region Friction
		if (coyote > 0 && Mathf.Abs(moveInput) == 0f)
		{
			float amount = Mathf.Min(Mathf.Abs(rb.velocity.x), frictionAmount) * Mathf.Sign(rb.velocity.x);
			rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
		}
		#endregion

		#region Gravity
		if (rb.velocity.y < 0 && !(isGrappled && isHanging))
		{
			rb.gravityScale = !isClinging ? gravityScale * fallGravityMultiplier : wallSlideGravity;
			rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, !isClinging ? -maxFallSpeed : -maxWallSlideSpeed));
			isJumping = false;
			inWallJump = 0;
		}
		else { rb.gravityScale = gravityScale; }
		#endregion
	}

	void Jump()
	{
		rb.velocity = new Vector2(rb.velocity.x, jumpForce);
		coyote = 0f;
		buffer = 0f;
		jumpDelay = jumpDelayTime;
		isJumping = true;
	}

	void WallJump(int dir)
	{
		rb.velocity = new Vector2(wallJumpForce.x * dir, wallJumpForce.y);
		rightCoyote = 0f;
		leftCoyote = 0f;
		buffer = 0f;
		jumpDelay = jumpDelayTime;
		inGrappleAccel = false;
		isClinging = false;
		isJumping = true;
		inWallJump = -dir;
	}

	void OnJumpUp()
	{
		if (rb.velocity.y > 0 && isJumping) { rb.AddForce(Vector2.down * rb.velocity.y * (1 - jumpCutMultiplier), ForceMode2D.Impulse); }
		isJumping = false;
		buffer = 0f;
	}

	void Grapple()
	{
		ropeScript.points[ropeScript.points.Count - 1].pastPos = ropeScript.points[ropeScript.points.Count - 1].currentPos;
		ropeScript.points[ropeScript.points.Count - 1].currentPos = transform.position;
		
		if (Input.GetMouseButtonUp(0))
		{
			isGrappled = false;
			joint.enabled = false;
			OnGrappleUp();
			return;
		}

		if (Vector2.Distance(transform.position, grapplePoint) >= grappleRadius - 0.1f && !onGround)
		{
			inGrappleAccel = true;
			isHanging = true;
		}
		else { isHanging = false; }

		if (Input.GetButtonDown("Pull") || Input.GetMouseButtonDown(1)) { Pull(); }
	}

	void Pull()
	{
		isJumping = false;
		joint.enabled = false;
		inGrappleAccel = true;
		rb.velocity = (grapplePoint - (Vector2) transform.position).normalized * Mathf.Max(minPullSpeed, rb.velocity.magnitude);
	}

	void OnGrappleDown()
	{
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 direction = (mousePos - (Vector2) transform.position).normalized;
		RaycastHit2D raycast = Physics2D.Raycast(transform.position, direction, grappleRange, grappleLayers);
		if (raycast.collider != null)
		{
			isGrappled = true;
			TargetPosition hitCollider = raycast.collider.gameObject.GetComponentInParent<TargetPosition>();
			grapplePoint = new Vector2(
				Mathf.Clamp(raycast.point.x, hitCollider.gameObject.transform.position.x + hitCollider.minGrappleBounds.x, hitCollider.gameObject.transform.position.x + hitCollider.maxGrappleBounds.x),
				Mathf.Clamp(raycast.point.y, hitCollider.gameObject.transform.position.y + hitCollider.minGrappleBounds.y, hitCollider.gameObject.transform.position.y + hitCollider.maxGrappleBounds.y));
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
		for (int i = 0; i < ropeScript.lines.Count; i++)
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

		for (float i = 0.5f; i < grappleRadius; i += 0.5f)
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

// fast fall and fast slide
// make level
// make level mechanics
// fix pull length and floor thing