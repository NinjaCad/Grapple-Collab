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

		onWall = Physics2D.OverlapBox(player.data.leftCheckPoint + (Vector2) transform.position, player.data.wallCheckSize, 0f, player.data.wallLayer) ? -1 :
			Physics2D.OverlapBox(player.data.rightCheckPoint + (Vector2) transform.position, player.data.wallCheckSize, 0f, player.data.wallLayer) ? 1 : 0;
		isClinging = onWall != 0 && moveInput.x == onWall;

		if (onWall == -1 && isClinging) { wallCoyote[0] = player.data.wallCoyoteTime; }
		else if (onWall == 1 && isClinging) { wallCoyote[1] = player.data.wallCoyoteTime; }
		
		if (isClinging && rb.velocity.y < 0 && !(isGrappled && isHanging))
		{
			if (!resetVelocity)
			{
				rb.velocity = new Vector2(rb.velocity.x, -player.data.startWallSlideSpeed);
				resetVelocity = true;
			}
		}
		else { resetVelocity = false; }
		
		if (Physics2D.OverlapBox(player.data.groundCheckPoint + (Vector2) transform.position, player.data.groundCheckSize, 0f, player.data.groundLayer))
		{
			coyote = player.data.coyoteTime;
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

		float targetSpeed = moveInput.x * player.data.moveSpeed;
		float speedDif = targetSpeed - rb.velocity.x;
		float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? (!inGrappleAccel ? player.data.acceleration : player.data.swingAcceleration) : (inWallJump == 0 ? (!inGrappleAccel ? player.data.decceleration : player.data.swingDecceleration) : player.data.wallJumpDecceleration);
		float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, player.data.velPower) * Mathf.Sign(speedDif);
		rb.AddForce(movement * Vector2.right);
		#endregion

		#region Friction
		if (onGround && Mathf.Abs(moveInput.x) == 0f)
		{
			float amount = Mathf.Min(Mathf.Abs(rb.velocity.x), player.data.frictionAmount) * Mathf.Sign(rb.velocity.x);
			rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
		}
		#endregion

		#region Gravity
		if (rb.velocity.y < 0 && !(isGrappled && isHanging))
		{
			rb.gravityScale = (!isClinging ? player.data.gravityScale * player.data.fallGravityMultiplier : player.data.wallSlideGravity) * (moveInput.y == -1 ? player.data.fastFallMultiplier : 1);
			rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, !isClinging ? -player.data.maxFallSpeed : -player.data.maxWallSlideSpeed));
			isJumping = false;
			inWallJump = 0;
		}
		else { rb.gravityScale = player.data.gravityScale; }
		#endregion
	}

	void OnJumpDown(int dir)
	{
		if (dir == 0) { rb.velocity = new Vector2(rb.velocity.x, player.data.jumpForce); }
		else { rb.velocity = new Vector2(player.data.wallJumpForce.x * dir, player.data.wallJumpForce.y); }

		coyote = 0f;
		wallCoyote[0] = 0f;
		wallCoyote[1] = 0f;
		buffer = 0f;
		jumpDelay = player.data.jumpDelayTime;
		inGrappleAccel = false;
		isClinging = false;
		isJumping = true;
		inWallJump = -dir;
	}

	void OnJumpUp()
	{
		if (rb.velocity.y > 0 && isJumping) { rb.AddForce(Vector2.down * rb.velocity.y * (1 - player.data.jumpCutMultiplier), ForceMode2D.Impulse); }
		isJumping = false;
		buffer = 0f;
	}

	void OnGrapple()
	{
		if (Vector2.Distance(transform.position, grapplePoint) >= grappleRadius - 0.1f && !onGround)
		{
			inGrappleAccel = true;
			isHanging = true;
		}
		else { isHanging = false; }
	}

	void OnPullDown()
	{
		if (!isGrappled) { return; }
		
		isJumping = false;
		inGrappleAccel = true;
		inWallJump = 0;
		rb.velocity = (grapplePoint - (Vector2) transform.position).normalized * Mathf.Max(player.data.minPullSpeed, rb.velocity.magnitude);
		OnGrappleUp();
		StartCoroutine(PullStopMovement());
	}

	void OnGrappleDown()
	{
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 direction = (mousePos - (Vector2) transform.position).normalized;
		RaycastHit2D raycast = Physics2D.Raycast(transform.position, direction, player.data.grappleRange, player.data.grappleLayers);
		
		if (raycast.collider == null) { return; }
			
		isGrappled = true;
		TargetPosition hitCollider = raycast.collider.gameObject.GetComponentInParent<TargetPosition>();
		grapplePoint = new Vector2(
			Mathf.Clamp(raycast.point.x, hitCollider.gameObject.transform.position.x + hitCollider.minGrappleBounds.x, hitCollider.gameObject.transform.position.x + hitCollider.maxGrappleBounds.x),
			Mathf.Clamp(raycast.point.y, hitCollider.gameObject.transform.position.y + hitCollider.minGrappleBounds.y, hitCollider.gameObject.transform.position.y + hitCollider.maxGrappleBounds.y));
		joint.connectedAnchor = grapplePoint;
		joint.enabled = true;
		grappleRadius = Vector2.Distance(transform.position, grapplePoint);
		joint.distance = grappleRadius;

		player.events.OnGrapple?.Invoke(grapplePoint);
	}

	IEnumerator PullStopMovement()
	{
		canMove = false;
		
		buffer = 0.0f;
		coyote = 0.0f;
		wallCoyote[0] = 0.0f;
		wallCoyote[1] = 0.0f;
		jumpDelay = 0.0f;

		rb.gravityScale = 0;

		yield return new WaitForSeconds(player.data.pullDuration);

		canMove = true;
	}

	void OnGrappleUp()
	{
		isGrappled = false;
		joint.enabled = false;
	}

    void MoveInput(Vector2 input)
    {
        moveInput = input;
    }

    void OnJumpButtonDown()
    {
        buffer = player.data.bufferTime;
    }

	void StopMovement()
	{
		canMove = false;

		buffer = 0.0f;
		coyote = 0.0f;
		wallCoyote[0] = 0.0f;
		wallCoyote[1] = 0.0f;
		jumpDelay = 0.0f;

		rb.velocity = Vector2.zero;
		rb.gravityScale = 0;

		StopAllCoroutines();

		if (isGrappled) { OnGrappleUp(); }
	}

	void StartMovement()
	{
		canMove = true;
	}

    void OnEnable()
    {
        player.events.OnXYInput += MoveInput;
        player.events.OnJumpButtonDown += OnJumpButtonDown;
        player.events.OnJumpButtonUp += OnJumpUp;
        player.events.OnGrappleButtonDown += OnGrappleDown;
        player.events.OnGrappleButtonUp += OnGrappleUp;
        player.events.OnPullButtonDown += OnPullDown;
		player.events.OnDeath += StopMovement;
		player.events.OnRespawn += StartMovement;
    }

    void OnDisable()
    {
        player.events.OnXYInput -= MoveInput;
        player.events.OnJumpButtonDown -= OnJumpButtonDown;
        player.events.OnJumpButtonUp -= OnJumpUp;
        player.events.OnGrappleButtonDown -= OnGrappleDown;
        player.events.OnGrappleButtonUp -= OnGrappleUp;
        player.events.OnPullButtonDown -= OnPullDown;
		player.events.OnDeath -= StopMovement;
		player.events.OnRespawn -= StartMovement;
    }
}
