using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	Rigidbody2D rb;

	RaycastHit2D raycast;

	float moveInput;
	float coyote;
	float buffer;
	bool isJumping;
	
	public float avgFrameRate;

	DistanceJoint2D joint;
	Vector2 grappleBetterPoint;
	
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
	Vector3 pastPos;
		

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
		avgFrameRate = Time.frameCount / Time.time;
		
		#region Movement
		moveInput = Input.GetAxisRaw("Horizontal");

		coyote -= Time.deltaTime;
		buffer -= Time.deltaTime;
		
		if(Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0f, groundLayer))
		{
			coyote = coyoteTime;
			inGrappleMode = false;
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
		AwesomeBetterGrapple();

		/*if(Input.GetMouseButtonDown(0))
		{
			OnGrappleDown();
		}
		if(isSwinging)
		{
			Grapple();
		}*/
		#endregion
	}

	void FixedUpdate()
	{
		#region Run
		float targetSpeed = moveInput * moveSpeed;
		float speedDif = targetSpeed - rb.velocity.x;
		float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? (!inGrappleMode ? acceleration : swingAcceleration) : (!inGrappleMode ? decceleration : swingDecceleration);
		float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
		if (!Input.GetMouseButton(0))
		{
			rb.AddForce(movement * Vector2.right);
		}
		#endregion

		#region Friction
		if(coyote > 0 && Mathf.Abs(moveInput) == 0f)
		{
			float amount = Mathf.Min(Mathf.Abs(rb.velocity.x), Mathf.Abs(frictionAmount));
			amount *= Mathf.Sign(rb.velocity.x);
			if (!Input.GetMouseButton(0))
			{
				rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
			}
		}
		#endregion

		#region Jump Gravity
		if(rb.velocity.y < 0 && !Input.GetMouseButton(0))
		{
			rb.gravityScale = gravityScale * fallGravityMultiplier;
			rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
		} else
		{
			rb.gravityScale = gravityScale;
		}
		#endregion
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
		inGrappleMode = true;
		if(Vector2.Distance(transform.position, grapplePoint) < grappleRadius)
		{
			grappleRadius = Vector2.Distance(transform.position, grapplePoint);
		} else if(Vector2.Distance(transform.position, grapplePoint) > grappleRadius)
		{
			float radius = Vector2.Distance(transform.position, grapplePoint) - grappleRadius;
			float theta = Mathf.Atan2(transform.position.y - grapplePoint.y, transform.position.x - grapplePoint.x);
			transform.position += new Vector3((radius * Mathf.Cos(theta)) * -1, (radius * Mathf.Sin(theta)) * -1, 0);

			radius = Vector2.Distance(Vector2.zero, rb.velocity);
			theta = Mathf.Atan2(pastPos.y - transform.position.y, pastPos.x - transform.position.x);
			rb.velocity = new Vector2((radius * Mathf.Cos(theta)) * -1, (radius * Mathf.Sin(theta)) * -1);
		}
		pastPos = transform.position;

		if(Input.GetMouseButtonUp(0))
		{
			isSwinging = false;
		}
	}

	void OnGrappleDown()
	{
		isSwinging = true;
		grapplePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		grappleRadius = Vector2.Distance(transform.position, grapplePoint);
		GameObject currentPrefab = Instantiate(grapplePrefab);
		currentPrefab.GetComponent<Rope>().player = this;
	}

	void AwesomeBetterGrapple()
	{
		if (Input.GetMouseButtonDown(0))
        {
            grappleBetterPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            joint.connectedAnchor = grappleBetterPoint;
            joint.distance = Vector2.Distance(transform.position, grappleBetterPoint);
            joint.enabled = true;
        } else
        {
            joint.enabled = false;
        }
	}
}