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

        private bool _isGrounded;
        public Transform groundCheck;
        public float checkRadius = 0.3f;
        public LayerMask whatIsGround;

        private float jumpTimeCounter;
        public float jumpTime = 0.2f;

        private int extraJumps;
        public int extraJumpsValue = 1;
        private bool isJumping;

        public void JumpUpdate(Rigidbody2D rb, Animator anim)
        {
            _isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);

            if (_isGrounded == true) {
                extraJumps = extraJumpsValue;
                anim.SetBool("isJumping", false);
            } else {
                anim.SetBool("isJumping", true);
            }

            if (Input.GetKeyDown(KeyCode.Space) && extraJumps > 0) {
                anim.SetTrigger("takeOf");
                isJumping = true;
                jumpTimeCounter = jumpTime;
                rb.velocity = Vector2.up * jumpForce;
                extraJumps--;
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

        public bool isGrounded()
        {
            return _isGrounded;
        }
    }
    public JumpProperties jumpProperties;

    public float speed;
    private float inputHorizontal;
    private float inputVertical;

    private Rigidbody2D rb;
    private BoxCollider2D col;
    private Animator anim;

    private bool facingRight = true;

    private bool isDown = false;
    private float colSize;
    private float colOffset;

    public LayerMask whatIsLadder;
    private bool isClimbing;

    public float defaultGravityScale = 5;

    void Start()
    {
        anim = GetComponent<Animator>();    
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        colSize = col.size.y;
        colOffset = col.offset.y;
    }

    void FixedUpdate()
    {
        if (jumpProperties.isGrounded() && isDown) {
            rb.velocity = new Vector2(0, rb.velocity.y);
        } else {
            rb.velocity = new Vector2(inputHorizontal * speed, rb.velocity.y);
        }

        if (isClimbing) {
            inputVertical = Input.GetAxisRaw("Vertical");
            rb.velocity = new Vector2(rb.velocity.x, inputVertical * speed);
            rb.gravityScale = 0;
        } else {
            rb.gravityScale = defaultGravityScale;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & whatIsLadder) != 0)
        {
            isClimbing = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & whatIsLadder) != 0)
        {
            isClimbing = false;
        }
    }

    private void Update()
    {
        inputHorizontal = Input.GetAxisRaw("Horizontal");
        
        jumpProperties.JumpUpdate(rb, anim);

        anim.SetBool("isRunning", inputHorizontal != 0);

        if ((facingRight == false && inputHorizontal > 0) || (facingRight == true && inputHorizontal < 0)) {
            Flip();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            isDown = true;
            col.size = new Vector2(col.size.x, colSize / 2);
            col.offset = new Vector2(col.offset.x, colOffset * 2);
            anim.SetBool("isDown", isDown);
        }

        if (Input.GetKeyUp(KeyCode.DownArrow)) {
            isDown = false;
            col.size = new Vector2(col.size.x, colSize);
            col.offset = new Vector2(col.offset.x, colOffset);
            anim.SetBool("isDown", isDown);
        }
    }

    void Flip()
    {
        facingRight = !facingRight;

        GetComponent<SpriteRenderer>().flipX = !facingRight;
    }
}
