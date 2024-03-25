using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Enemy4_Controller : MonoBehaviour
{
    //Variables to start 

    //Horizontal mouvement Limits
    public Vector2 limitL, limitR;

    public Rigidbody2D myRb;
    public Animator myAni;
    public CircleCollider2D myCC;
    public BoxCollider2D myBxC;
    public LayerMask theGroundMask;
    public GameObject thePlayer;
    private Enemy4_Interactions InteractionManager;
    private AnimationClip[] Clips;
    public GameObject Projectile;



    //Parametrage

    //Possible actions : W - Waiting, L - Going Left, R - Going Right 
    public char currentAction = 'W';
    public char nextAction = 'L';

    public int Health = 1;


    //Status
    public bool isAttacking = false;
    public bool isDead = false;
    private bool gCheck = false;
    private bool toDestroy = false;


    //Timers-Coldowns
    public float attackDur = 0.0f;
    private float attackColdown = 3.0f;
    public float timer = 0.0f;


    // Start is called before the first frame update
    void Start()
    {
        //Set Horizontal Limits
        limitL = new Vector2(transform.position.x - 5, transform.position.y);
        limitR = new Vector2(transform.position.x + 5, transform.position.y);


        //Chose a Random Direction to go First
        int rand = UnityEngine.Random.Range(0, 2);
        if (rand == 0)
        {
            nextAction = 'L';
        }
        else
        {
            nextAction = 'R';
        }

        //Ignore other enemies collisions
        Physics2D.IgnoreLayerCollision(8, 8);
        myCC = GetComponent<CircleCollider2D>();
        myBxC = GetComponent<BoxCollider2D>();
        myRb = GetComponent<Rigidbody2D>();
        myAni = GetComponent<Animator>();
        InteractionManager = GetComponent<Enemy4_Interactions>();

        theGroundMask = LayerMask.GetMask("Ground");

    }

    private void FixedUpdate()
    {
        // if the player has been spotted by the enemy and is not that far, attack else dont attack

        if (thePlayer != null && Vector3.Distance(transform.position, thePlayer.transform.position) < 12f && !isDead)
        {

            attack();
        }
        else
        {
            isAttacking = false;
            attackDur = 0f;
        }


        if (!isDead)
        {
            //if not dead then move
            Mouvement(currentAction);
        }
        else
        {
            //if is dead  and falls to the ground
            if (gCheck && !toDestroy)
            {
                myRb.velocity = Vector2.zero;
                myRb.gravityScale = 0f;
                myAni.SetBool("OnGround", true);
                myAni.SetBool("Dead", false);
                toDestroy = true;

            }
            else
            {
                //if is in the ground , destroy
                gonnaDestroy();
            }
            
        }


    }



    // Update is called once per frame
    void Update()
    {
        gCheck = InteractionManager.groundChck();

        if(Health == 0 && !isDead)
        {
            Dies();
        }

    }


    private void gonnaDestroy()
    {
        timer += Time.deltaTime;

        if (timer > 4)
        {
            Destroy(this.gameObject);
        }
    }

    private void Mouvement(char Action)
    {

        switch (Action)
        {
            case 'W':

                waitTime();

                break;

            case 'L':

                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                myRb.velocity = new Vector2(-5, 0);
                getDir();

                break;

            case 'R':

                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                myRb.velocity = new Vector2(5, 0);
                getDir();

                break;

        }
    }

    private void attack()
    {
        if (!isAttacking)
        {
            //attack coldown

            attackColdown += Time.deltaTime;
            if (attackColdown > 2f)
            {
                myAni.SetTrigger("Attack");
                isAttacking = true;

                attackColdown = 0f;

            }

        }

        if (isAttacking)
        {
            //attack synchronized with animation
            attackDur += Time.deltaTime;
            myRb.velocity = Vector2.zero;

            if (attackDur > 0.4f)
            {

                Instantiate(Projectile, this.transform.position, this.transform.rotation);
                attackDur = 0.0f;
                isAttacking = false;

            }
        }

    }
    private void waitTime()
    {

        //Debug.Log("is waiting");
        timer += Time.deltaTime;
        if (timer > 1.5f)
        {
            //Wait for 1.5 seconds then change action
            currentAction = nextAction;
            nextAction = 'W';
            timer = 0.0f;
            //Debug.Log("Enemy Waiting ends");

        }
    }


    private void Dies()
    {
        isDead = true;
        myCC.isTrigger = true;
        myAni.SetBool("Dead", true);
        myRb.velocity = Vector2.zero;
        myRb.isKinematic = false;
        myRb.gravityScale = 2;
    }

    public void getDir()
    {

        if (Vector2.Distance(transform.position, limitL) < 0.5f && currentAction == 'L')
        {
            myRb.velocity = Vector2.zero;
            currentAction = 'W';
            nextAction = 'R';

        }
        else if (Vector2.Distance(transform.position, limitR) < 0.5f && currentAction == 'R')
        {
            myRb.velocity = Vector2.zero;
            currentAction = 'W';
            nextAction = 'L';
        }


        if (currentAction == 'A')
        {
            currentAction = 'W';

            if (transform.rotation.y == 0)
            {
                nextAction = 'R';
            }
            else
            {
                nextAction = 'L';
            }
        }


    }



}
