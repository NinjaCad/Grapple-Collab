using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    [HideInInspector] public Player player;
    LineRenderer lineRenderer;
    Rigidbody2D rb;
    DistanceJoint2D joint;

    [Header("Swing")]
    [SerializeField] float swingDecceleration;
    [SerializeField] float velPower;
    [SerializeField] float gravityScale;
    
    bool isCut;
    [SerializeField] float fullTime;
    [SerializeField] float invisTime;
    float timeAfterCut;

    Vector2 ownGrapplePoint;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        rb = GetComponent<Rigidbody2D>();
        joint = GetComponent<DistanceJoint2D>();
    }
    void Start()
    {
        lineRenderer.startWidth = 0.15f;
        lineRenderer.endWidth = 0.15f;
        timeAfterCut = 0.0f;
        rb.gravityScale = 0;

        joint.enabled = false;
    }

    void Update()
    {
        if(player.isSwinging == false && isCut == false)
        {
            isCut = true;
            joint.enabled = true;
            transform.position = player.transform.position;
            rb.velocity = player.GetComponent<Rigidbody2D>().velocity;
            rb.gravityScale = gravityScale;
            joint.connectedAnchor = ownGrapplePoint;
        }

        if(isCut == false)
        {
            lineRenderer.SetPositions(new Vector3[] {player.grapplePoint, player.transform.position});
            ownGrapplePoint = player.grapplePoint;
            
        } else
        {
            timeAfterCut += Time.deltaTime;
            float speedDif = -rb.velocity.x;
            float accelRate = swingDecceleration;
            float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
            rb.AddForce(movement * Vector2.right);
            
            if(timeAfterCut > fullTime)
            {
                Destroy(this.gameObject);
            }
            lineRenderer.SetPositions(new Vector3[] {ownGrapplePoint, transform.position});
        }
    }
}
