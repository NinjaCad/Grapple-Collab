using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerMovement : PlayerSystem
{
    Rigidbody2D rb;
	DistanceJoint2D joint;
	Rope ropeScript;

	Vector2 moveInput;
	float coyote;
	float2 wallCoyote;
	float buffer;
	float jumpDelay;
	bool isJumping;
	bool onGround;
	int onWall;
	bool isClinging;
	bool resetVelocity;
	int inWallJump;
	bool canMove = true;

	[HideInInspector] public Vector2 grapplePoint;
	[HideInInspector] public float grappleRadius;
	[HideInInspector] public bool isGrappled;
	[HideInInspector] public bool inGrappleAccel;
	[HideInInspector] public bool isHanging;

	protected override void Awake()
	{
		base.Awake();
        rb = GetComponent<Rigidbody2D>();
		joint = GetComponent<DistanceJoint2D>();
	}

	void Update()
	{
		if (!canMove) { return; }
		
		#region Timers
		coyote -= Time.deltaTime;
		wallCoyote[0] -= Time.deltaTime;
		wallCoyote[1] -= Time.deltaTime;
		buffer -= Time.deltaTime;
		jumpDelay -= Time.deltaTime;
		#endregion

		#region Checks
		if (moveInput.x == -Mathf.Sign(rb.velocity.x)) { inGrappleAccel = false; }

		onWall = Physics2D.OverlapBox(player.ID.data.leftCheckPoint + (Vector2) transform.position, player.ID.data.wallCheckSize, 0f, player.ID.data.wallLayer) ? -1 :
			Physics2D.OverlapBox(player.ID.data.rightCheckPoint + (Vector2) transform.position, player.ID.data.wallCheckSize, 0f, player.ID.data.wallLayer) ? 1 : 0;
		isClinging = onWall != 0 && moveInput.x == onWall;

		if (onWall == -1 && isClinging) { wallCoyote[0] = player.ID.data.wallCoyoteTime; }
		else if (onWall == 1 && isClinging) { wallCoyote[1] = player.ID.data.wallCoyoteTime; }
		
		if (isClinging && rb.velocity.y < 0 && !(isGrappled && isHanging))
		{
			if (!resetVelocity)
			{
				rb.velocity = new Vector2(rb.velocity.x, -player.ID.data.startWallSlideSpeed);
				resetVelocity = true;
			}
		}
		else { resetVelocity = false; }
		
		if (Physics2D.OverlapBox(player.ID.data.groundCheckPoint + (Vector2) transform.position, player.ID.data.groundCheckSize, 0f, player.ID.data.groundLayer))
		{
			coyote = player.ID.data.coyoteTime;
			onGround = true;
			inGrappleAccel = false;
		}
		else if (onGround) { onGround = false; }
		#endregion

		#region Jump
		if (buffer > 0f && jumpDelay <= 0f && !isJumping)
		{
			if (coyote > 0f) { OnJumpDown(0); }
			else if (wallCoyote[0] > 0f || wallCoyote[1] > 0f) { OnJumpDown(wallCoyote[0] > wallCoyote[1] ? 1 : -1); }
		}
		#endregion
		
		#region Grapple
		if (isGrappled) { OnGrapple(); }
		#endregion
	}

	void FixedUpdate()
	{
		if (!canMove) { return; }
		
		#region Run
		if (inWallJump != 0 && inWallJump == moveInput.x) { moveInput.x = 0; }

		float targetSpeed = moveInput.x * player.ID.data.moveSpeed;
		float speedDif = targetSpeed - rb.velocity.x;
		float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? (!inGrappleAccel ? player.ID.data.acceleration : player.ID.data.swingAcceleration) : (inWallJump == 0 ? (!inGrappleAccel ? player.ID.data.decceleration : player.ID.data.swingDecceleration) : player.ID.data.wallJumpDecceleration);
		float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, player.ID.data.velPower) * Mathf.Sign(speedDif);
		rb.AddForce(movement * Vector2.right);
		#endregion

		#region Friction
		if (onGround && Mathf.Abs(moveInput.x) == 0f)
		{
			float amount = Mathf.Min(Mathf.Abs(rb.velocity.x), player.ID.data.frictionAmount) * Mathf.Sign(rb.velocity.x);
			rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
		}
		#endregion

		#region Gravity
		if (rb.velocity.y < 0 && !(isGrappled && isHanging))
		{
			rb.gravityScale = (!isClinging ? player.ID.data.gravityScale * player.ID.data.fallGravityMultiplier : player.ID.data.wallSlideGravity) * (moveInput.y == -1 ? player.ID.data.fastFallMultiplier : 1);
			rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, !isClinging ? -player.ID.data.maxFallSpeed : -player.ID.data.maxWallSlideSpeed));
			isJumping = false;
			inWallJump = 0;
		}
		else { rb.gravityScale = player.ID.data.gravityScale; }
		#endregion
	}

	void OnJumpDown(int dir)
	{
		if (dir == 0) { rb.velocity = new Vector2(rb.velocity.x, player.ID.data.jumpForce); }
		else { rb.velocity = new Vector2(player.ID.data.wallJumpForce.x * dir, player.ID.data.wallJumpForce.y); }

		coyote = 0f;
		wallCoyote[0] = 0f;
		wallCoyote[1] = 0f;
		buffer = 0f;
		jumpDelay = player.ID.data.jumpDelayTime;
		inGrappleAccel = false;
		isClinging = false;
		isJumping = true;
		inWallJump = -dir;
	}

	void OnJumpUp()
	{
		if (rb.velocity.y > 0 && isJumping) { rb.AddForce(Vector2.down * rb.velocity.y * (1 - player.ID.data.jumpCutMultiplier), ForceMode2D.Impulse); }
		isJumping = false;
		buffer = 0f;
	}

	void OnGrapple()
	{
		ropeScript.points[ropeScript.points.Count - 1].pastPos = ropeScript.points[ropeScript.points.Count - 1].currentPos;
		ropeScript.points[ropeScript.points.Count - 1].currentPos = transform.position;

		if (Vector2.Distance(transform.position, grapplePoint) >= grappleRadius - 0.1f && !onGround)
		{
			inGrappleAccel = true;
			isHanging = true;
		}
		else { isHanging = false; }
	}

	void OnPullDown()
	{
		isJumping = false;
		inGrappleAccel = true;
		inWallJump = 0;
		rb.velocity = (grapplePoint - (Vector2) transform.position).normalized * Mathf.Max(player.ID.data.minPullSpeed, rb.velocity.magnitude);
		OnGrappleUp();
	}

	void OnGrappleDown()
	{
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 direction = (mousePos - (Vector2) transform.position).normalized;
		RaycastHit2D raycast = Physics2D.Raycast(transform.position, direction, player.ID.data.grappleRange, player.ID.data.grappleLayers);
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
		if (!isGrappled) { return; }
		
		isGrappled = false;
		joint.enabled = false;
		ropeScript.points[ropeScript.points.Count - 1].isLocked = false;
		for (int i = 0; i < ropeScript.lines.Count; i++)
		{
			ropeScript.lines[i].lineLength = Vector2.Distance(ropeScript.points[ropeScript.lines[i].pointIndexes.x].currentPos, ropeScript.points[ropeScript.lines[i].pointIndexes.y].currentPos);
		}
	}

	void CreateRope()
	{
		GameObject currentPrefab = Instantiate(player.ID.data.grapplePrefab);
		ropeScript = currentPrefab.GetComponent<Rope>();
		// ropeScript.player = this;

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

    void MoveInput(Vector2 input)
    {
        moveInput = input;
    }

    void OnJumpButtonDown()
    {
        buffer = player.ID.data.bufferTime;
    }

	void StopMovement()
	{
		canMove = false;
		rb.velocity = Vector2.zero;
		rb.gravityScale = 0;
	}

	void StartMovement()
	{
		canMove = true;
	}

    void OnEnable()
    {
        player.ID.events.OnXYInput += MoveInput;
        player.ID.events.OnJumpDown += OnJumpButtonDown;
        player.ID.events.OnJumpUp += OnJumpUp;
        player.ID.events.OnGrappleDown += OnGrappleDown;
        player.ID.events.OnGrappleUp += OnGrappleUp;
        player.ID.events.OnPullDown += OnPullDown;
		player.ID.events.OnDeath += StopMovement;
		player.ID.events.OnRespawn += StartMovement;
    }

    void OnDisable()
    {
        player.ID.events.OnXYInput -= MoveInput;
        player.ID.events.OnJumpDown -= OnJumpButtonDown;
        player.ID.events.OnJumpUp -= OnJumpUp;
        player.ID.events.OnGrappleDown -= OnGrappleDown;
        player.ID.events.OnGrappleUp -= OnGrappleUp;
        player.ID.events.OnPullDown -= OnPullDown;
		player.ID.events.OnDeath -= StopMovement;
		player.ID.events.OnRespawn -= StartMovement;
    }
}
