using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float jumpForce = 12;

    private bool inFloor;
    public Transform groundCheck;
    public float checkRadius = 0.3f;
    public LayerMask whatIsGround;

    private float jumpTimeCounter;
    public float jumpTime = 0.2f;

    private int extraJumps;
    public int extraJumpsValue = 1;
    private bool isJumping;

    private int sideTouch;
    private int sideJumped;
    public Transform rightCheck;
    public Transform leftCheck;
    public float wallSlideSpeed = 4;

    public float xWallForce = 12;
    public float wallJumpTime = 0.2f;
    private bool isWallJumping;

    public float speed = 12;
    private float inputHorizontal;
    private float inputVertical;

    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private Animator anim;

    private bool facingRight = true;

    private bool crounching = false;
    private float colSize;
    private float colOffset;

    public LayerMask whatIsLadder;
    public LayerMask whatIsWater;
    private bool isClimbing;
    private bool isSwimming;
    private bool isSlidingOnWall;

    public float defaultGravityScale = 5;

    void Start()
    {
        anim = GetComponent<Animator>();    
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        colSize = col.size.y;
        colOffset = col.offset.y;
    }

    void FixedUpdate()
    {
        if (inFloor && crounching) {
            rb.velocity = new Vector2(0, rb.velocity.y);
        } else if (isSwimming) {
                rb.velocity = new Vector2(inputHorizontal * (speed / 2), rb.velocity.y);
        } else if (isClimbing) {
            rb.velocity = new Vector2(rb.velocity.x, inputVertical * speed);
            rb.gravityScale = 0;
        }
        else if (!inFloor && !isWallJumping && ((sideTouch < 0 && inputHorizontal < 0) || (sideTouch > 0 && inputHorizontal > 0))) {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlideSpeed, float.MaxValue));
            isSlidingOnWall = true;
        }
        else {
            if (!isWallJumping) {
                rb.velocity = new Vector2(inputHorizontal * speed, rb.velocity.y);
            }
            rb.gravityScale = defaultGravityScale;
            isSlidingOnWall = false;
        }
        anim.SetFloat("ySpeed", rb.velocity.y);
        anim.SetBool("inFloor", inFloor);
        anim.SetBool("isSlidingOnWall", isSlidingOnWall);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & whatIsLadder) != 0) {
            isClimbing = true;
        } else if (((1 << collision.gameObject.layer) & whatIsWater) != 0) {
            isSwimming = true;
            anim.SetTrigger("waterEnter");
            anim.SetBool("inWater", isSwimming);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & whatIsLadder) != 0) {
            isClimbing = false;
        }
        else if (((1 << collision.gameObject.layer) & whatIsWater) != 0) {
            isSwimming = false;
            anim.SetBool("inWater", isSwimming);
        }
    }

    private void Update()
    {
        inputHorizontal = Input.GetAxisRaw("Horizontal");
        inputVertical = Input.GetAxisRaw("Vertical");

        inFloor = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);

        if (inFloor || isSwimming || isSlidingOnWall) {
            extraJumps = extraJumpsValue;
            if (!isSwimming) {
                anim.SetBool("isJumping", false);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && extraJumps > 0) {
            if (!inFloor && !isSwimming) {
                anim.SetTrigger("anotherJump");
                extraJumps--;
            }
            isJumping = true;
            anim.SetBool("isJumping", true);
            jumpTimeCounter = jumpTime;
            if (isSlidingOnWall) {
                isWallJumping = true;
                sideJumped = sideTouch;
                Invoke("SetWallJumpingFalse", wallJumpTime);
            }
        }

        if (Input.GetKeyUp(KeyCode.Space)) {
            isJumping = false;
        }

        if (isJumping && jumpTimeCounter > 0) {
            if (isSlidingOnWall) {
                rb.velocity = new Vector2(-sideJumped * xWallForce, jumpForce);
            } else {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
            jumpTimeCounter -= Time.deltaTime;
        } else {
            isJumping = false;
        }

        if (Physics2D.OverlapCircle(rightCheck.position, checkRadius, whatIsGround)) {
            sideTouch = 1;
        } else if (Physics2D.OverlapCircle(leftCheck.position, checkRadius, whatIsGround)) {
            sideTouch = -1;
        } else {
            sideTouch = 0;
        }

        anim.SetBool("isRunning", inputHorizontal != 0);

        if ((facingRight == false && inputHorizontal > 0) || (facingRight == true && inputHorizontal < 0)) {
            Flip();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) && inFloor) {
            crounching = true;
            col.size = new Vector2(col.size.x, colSize / 2);
            col.offset = new Vector2(col.offset.x, colOffset * 2);
            anim.SetBool("crounching", crounching);
        }

        if (Input.GetKey(KeyCode.DownArrow) && isSwimming) {
            rb.velocity = new Vector2(rb.velocity.x, inputVertical * speed / 2);
        }

        if (Input.GetKeyUp(KeyCode.DownArrow)) {
            crounching = false;
            col.size = new Vector2(col.size.x, colSize);
            col.offset = new Vector2(col.offset.x, colOffset);
            anim.SetBool("crounching", crounching);
        }
    }

    void SetWallJumpingFalse()
    {
        isWallJumping = false;
    }

    void Flip()
    {
        facingRight = !facingRight;

        GetComponent<SpriteRenderer>().flipX = !facingRight;
    }
}
