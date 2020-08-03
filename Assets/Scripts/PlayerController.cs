using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [System.Serializable]
    public class JumpProperties
    {
        public float jumpForce = 9;

        private bool _inFloor;
        public Transform groundCheck;
        public float checkRadius = 0.3f;
        public LayerMask whatIsGround;

        private float jumpTimeCounter;
        public float jumpTime = 0.2f;

        private int extraJumps;
        public int extraJumpsValue = 1;
        private bool isJumping;

        public void JumpUpdate(Rigidbody2D rb, Animator anim, bool isSwimming)
        {
            _inFloor = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);

            if (_inFloor == true || isSwimming) {
                extraJumps = extraJumpsValue;
                if (!isSwimming) {
                    anim.SetBool("isJumping", false);
                }
            }

            if (Input.GetKeyDown(KeyCode.Space) && extraJumps > 0) {
                if (_inFloor == false && !isSwimming) {
                    anim.SetTrigger("anotherJump");
                    extraJumps--;
                }
                isJumping = true;
                anim.SetBool("isJumping", true);
                jumpTimeCounter = jumpTime;
                rb.velocity = Vector2.up * jumpForce;
            }

            if (Input.GetKeyUp(KeyCode.Space)) {
                isJumping = false;
            }

            if (isJumping && jumpTimeCounter > 0) {
                rb.velocity = Vector2.up * jumpForce;
                jumpTimeCounter -= Time.deltaTime;
            } else {
                isJumping = false;
            }
        }

        public bool inFloor()
        {
            return _inFloor;
        }
    }
    public JumpProperties jumpProperties;

    public float speed;
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

        if (jumpProperties.inFloor() && crounching) {
            rb.velocity = new Vector2(0, rb.velocity.y);
        } else {
            if (isSwimming) {
                rb.velocity = new Vector2(inputHorizontal * (speed / 2), rb.velocity.y);
            } else {
                rb.velocity = new Vector2(inputHorizontal * speed, rb.velocity.y);
            }
        }

        if (isClimbing) {
            rb.velocity = new Vector2(rb.velocity.x, inputVertical * speed);
            rb.gravityScale = 0;
        } else {
            rb.gravityScale = defaultGravityScale;
        }
        anim.SetFloat("ySpeed", rb.velocity.y);
        anim.SetBool("inFloor", jumpProperties.inFloor());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & whatIsLadder) != 0)
        {
            isClimbing = true;
        } else if (((1 << collision.gameObject.layer) & whatIsWater) != 0)
        {
            isSwimming = true;
            anim.SetTrigger("waterEnter");
            anim.SetBool("inWater", isSwimming);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & whatIsLadder) != 0)
        {
            isClimbing = false;
        }
        else if (((1 << collision.gameObject.layer) & whatIsWater) != 0)
        {
            isSwimming = false;
            anim.SetBool("inWater", isSwimming);
        }
    }

    private void Update()
    {
        inputHorizontal = Input.GetAxisRaw("Horizontal");
        inputVertical = Input.GetAxisRaw("Vertical");

        jumpProperties.JumpUpdate(rb, anim, isSwimming);

        anim.SetBool("isRunning", inputHorizontal != 0);

        if ((facingRight == false && inputHorizontal > 0) || (facingRight == true && inputHorizontal < 0)) {
            Flip();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) && jumpProperties.inFloor()) {
            crounching = true;
            col.size = new Vector2(col.size.x, colSize / 2);
            col.offset = new Vector2(col.offset.x, colOffset * 2);
            anim.SetBool("crounching", crounching);
        }

        if (Input.GetKey(KeyCode.DownArrow) && isSwimming)
        {
            rb.velocity = new Vector2(rb.velocity.x, inputVertical * speed / 2);
        }

        if (Input.GetKeyUp(KeyCode.DownArrow)) {
            crounching = false;
            col.size = new Vector2(col.size.x, colSize);
            col.offset = new Vector2(col.offset.x, colOffset);
            anim.SetBool("crounching", crounching);
        }
    }

    void Flip()
    {
        facingRight = !facingRight;

        GetComponent<SpriteRenderer>().flipX = !facingRight;
    }
}
