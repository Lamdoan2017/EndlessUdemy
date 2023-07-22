using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;

    [Header("Speed info")]
    [SerializeField] private float maxSpeed;
    [SerializeField] private float speedMultiplier;
    private float defaultSpeed;
    [Space]
    [SerializeField] private float milestoneIncreaser;
    private float defaultMilestoneIncreaser;
    private float speedMilestone;


    

    [Header("Move info")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float doubleJumpForce;

    [Header("Slide info")]
    [SerializeField] private float slideSpeed;
    [SerializeField] private float slideTime;
    [SerializeField] private float slideCooldown;
    private float slideCooldownCounter;
    private float slideTimeCounter;
    private bool isSliding;

   

    private bool canDoubleJump;
    private bool playerUnlock;



    [Header("Collision info")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] LayerMask whatIsGround;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Vector2 wallCheckSize;
    [SerializeField] private float cellingCheckDistance;


    private bool isGrounded = true;
    private bool wallDetected;
    private bool cellingDetected;

    [Header("Ledge info")]
    [SerializeField] private Vector2 offset1;
    [SerializeField] private Vector2 offset2;

    private Vector2 climpBegunPosition;
    private Vector2 climpOverPosition;
    
    private bool canGrabLedge= true;
    [SerializeField] private bool canClimb;

    [HideInInspector] public bool ledgeDetected;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        speedMilestone = milestoneIncreaser;
        defaultSpeed = moveSpeed;
        defaultMilestoneIncreaser = milestoneIncreaser;
    }


    public void Update()
    {
        AnimatorController();
        CheckInput();
        CheckCollision();
        CheckForSlide();
        CheckForLedge();
        speedController();
             
        if (playerUnlock)
        {
            Movement();

        }

        if(isGrounded)
        {
            canDoubleJump = true;
        }
    }

    #region  Speed Control
    private void speedReset()
    {
        moveSpeed = defaultSpeed;
        milestoneIncreaser = defaultMilestoneIncreaser;
    }
    private void speedController()
    {
        if(moveSpeed == maxSpeed)
        {
            return;
        }

        if(transform.position.x > speedMilestone)
        {
            speedMilestone = speedMilestone + milestoneIncreaser;

            moveSpeed = moveSpeed * speedMultiplier;
            milestoneIncreaser = milestoneIncreaser * speedMultiplier;

            if(moveSpeed > maxSpeed)
            {
                moveSpeed = maxSpeed;
            }
        }
    }
    #endregion

    #region Ledge Control
    private void CheckForLedge()
    {
        if(ledgeDetected && canGrabLedge)
        {
            canGrabLedge = false;            
            Vector2 ledgePosition = GetComponentInChildren<LedgeDetection>().transform.position;

            climpBegunPosition = ledgePosition+ offset1;
            climpOverPosition = ledgePosition+ offset2;

            canClimb = true;

        }

        if (canClimb)
        {
            transform.position = climpBegunPosition;
        }
    }
    private void LedgeClimbOver()
    {
        canClimb=false;        
        transform.position = climpOverPosition;
        Invoke("AllowledGrab", .1f);
    }

    private void AllowledGrab()
    {
        canGrabLedge = true;
        
    }
    #endregion

    private void Movement()
    {
        if (wallDetected)
        {
            speedReset();
            return;
        }
        if (isSliding)
        {
            rb.velocity = new Vector2(slideSpeed, rb.velocity.y);
        }
        else
        {
        rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
        }
    }

    private void AnimatorController()
    {


        anim.SetBool("canClimb", canClimb);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("canDoubleJump", canDoubleJump);
        anim.SetBool("isSliding", isSliding);

        anim.SetFloat("xVelocity", rb.velocity.x);
        anim.SetFloat("yVelocity", rb.velocity.y);

        if(rb.velocity.y<-20)
        {
            anim.SetBool("canRoll", true);
        }
    }
           
    private void RollAnimFinished()
    {
        anim.SetBool("canRoll", false);
    }


    private void CheckInput()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            playerUnlock = true;

        }


        if (Input.GetButtonDown("Jump"))
        {
            JumpButton();

        }
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            SlideButton();
        }
    }
    private void CheckForSlide()
    {
        slideTimeCounter -= Time.deltaTime;
        slideCooldownCounter -= Time.deltaTime;

        if (slideTimeCounter < 0 && !cellingDetected)
        {
            isSliding = false;
            
        }
    }


    private void SlideButton()
    {
        if(rb.velocity.x!=0 && slideCooldownCounter <0)
        {
            isSliding = true;
            slideTimeCounter = slideTime;
            slideCooldownCounter = slideCooldown;
        }
    }

    private void JumpButton()
    {
        if (isSliding)
        {
            return;
        }

        if (isGrounded)
        {
            
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
        else if (canDoubleJump)
        {            
            canDoubleJump = false;
            rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
        }
    }

    private void CheckCollision()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
        cellingDetected = Physics2D.Raycast(transform.position, Vector2.up, cellingCheckDistance, whatIsGround);
        wallDetected = Physics2D.BoxCast(wallCheck.position, wallCheckSize, 0, Vector2.zero,0,whatIsGround);

        Debug.Log(ledgeDetected);
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y+ cellingCheckDistance));
        Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
    }
}
