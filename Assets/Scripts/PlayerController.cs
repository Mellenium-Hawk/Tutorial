using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D coll;

    private enum State { idle, running, jumping, falling, hurt }
    private State state = State.idle;

    [SerializeField] private LayerMask ground;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float jumpForce = 18f;
    [SerializeField] private int gems = 0;
    [SerializeField] private TextMeshProUGUI gemText;
    [SerializeField] private float hurtForce = 40f;
    [SerializeField] private AudioSource gem;
    [SerializeField] private AudioSource powerup;
    [SerializeField] private AudioSource footstep;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(state != State.hurt)
        {
            Movement();
        }
        AnimationState();
        anim.SetInteger("state", (int)state);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Collectable")
        {
            gem.Play();
            Destroy(collision.gameObject);
            gems += 1;
            gemText.text = gems.ToString();
        }
        if(collision.tag == "PowerUp")
        {
            //powerup.Play();
            Destroy(collision.gameObject);
            jumpForce = 40f;
            GetComponent<SpriteRenderer>().color = Color.magenta;
            StartCoroutine(ResetPower());

        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            if (state == State.falling)
            {
                enemy.JumpedOn();
                Jump();
            }
            else
            {
                state = State.hurt;
                if(other.gameObject.transform.position.x > transform.position.x)
                {
                    //Enemy is to playrs right and playr should be damaged and move left.
                    rb.velocity = new Vector2(-hurtForce, rb.velocity.y);
                }
                else
                {
                    //enemy is to players left and player should be damaged and mover right.
                    rb.velocity = new Vector2(hurtForce, rb.velocity.y);

                }
            }

        }
    }

    private void Movement()
    {
        float hDirection = Input.GetAxis("Horizontal");

        if (hDirection < 0)
        {
            rb.velocity = new Vector2(-speed, rb.velocity.y);
            transform.localScale = new Vector2(-1, 1);
        }
        else if (hDirection > 0)
        {
            rb.velocity = new Vector2(speed, rb.velocity.y);
            transform.localScale = new Vector2(1, 1);

        }

        if (Input.GetButtonDown("Jump") && coll.IsTouchingLayers(ground))
        {
            Jump();
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        state = State.jumping;
    }

    private void AnimationState()
    {
        if (state == State.jumping)
        {
            if(rb.velocity.y < 0.1f)
            {
                state = State.falling;
            }
        }
        else if (state == State.falling)
        {
            if (coll.IsTouchingLayers(ground))
            {
                state = State.idle;
            }
        }
        else if(state == State.hurt)
        {
            if(Mathf.Abs(rb.velocity.x) < 0.1f)
            {
                state = State.idle;
            }
        }
        else if(Mathf.Abs(rb.velocity.x) > 2f)
        {
            //Moving
            state = State.running;
        }else
        {
            state = State.idle;
        }
    }

    private void Footstep()
    {
        footstep.Play();
    }

    private IEnumerator ResetPower() 
    {
        yield return new WaitForSeconds(10);
        jumpForce = 30f;
        GetComponent<SpriteRenderer>().color = Color.white;
    }
}
