using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Rigidbody2D rb;
    BoxCollider2D bC;

    RaycastHit2D raycast;
    [SerializeField] LayerMask collisionLayers;
    
    float velX;
    float velY;
    [SerializeField] float speedX;
    [SerializeField] float jumpSpeed;
    float buffer;
    float coyote;
    [SerializeField] float groundFriction;
    [SerializeField] float airResistance;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bC = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        
    }
    
    void Update()
    {
        controller();


    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, velX, isGrounded() ? groundFriction : airResistance), rb.velocity.y);
    }
    
    void controller()
    {
        if (isGrounded())
        {
            coyote = 0.10f;
        } else
        {
            coyote -= Time.deltaTime;
        }
        
        if (Input.GetButtonDown("Jump"))
        {
            buffer = 0.15f;
        } else
        {
            buffer -= Time.deltaTime;
        }

        if (coyote > 0f && buffer > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
            buffer = 0f;
        }
        
        velX = Input.GetAxisRaw("Horizontal") * speedX;
    }

    bool isGrounded()
    {
        raycast = Physics2D.BoxCast(new Vector3(transform.position.x, transform.position.y - 0.55f, 0), new Vector3(1, 0.1f, 1), 0f, Vector2.down, 0f, collisionLayers);
        return raycast.collider != null;
    }
}
