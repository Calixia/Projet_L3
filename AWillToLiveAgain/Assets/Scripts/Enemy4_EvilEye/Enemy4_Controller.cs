using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Enemy4_Controller : Enemy
{
    //Variables to start 

    //Horizontal mouvement Limits
  //public Vector2 limitL, limitR;

   // public Rigidbody2D myRb;
    //public Animator myAni;
    public CircleCollider2D myCC;
    public BoxCollider2D myBxC;
    //public LayerMask theGroundMask;
    //public GameObject thePlayer;
    private Enemy4_Interactions InteractionManager;
    //private AnimationClip[] Clips;
    public GameObject ProjectilePrefab;



    //Parametrage

    //Possible actions : W - Waiting, L - Going Left, R - Going Right 
    //public char currentAction = 'W';
    //public char nextAction = 'L';

    protected int patrolDistance = 5;
    protected float enemyMouvSpeed = 5;

    //public int Health = 1;


    //Status
    //public bool isAttacking = false;
    //public bool isDead = false;
    //private bool gCheck = false;
    protected bool toDestroy = false;


    //Timers-Coldowns
    //public float attackDur = 0.0f;
    //private float attackColdown = 3.0f;
    public float timer = 0.0f;


    private void Awake()
    {
        currentAction = 'W';
        nextAction = 'L';
        Health = 1;

        isAttacking = false;
        isDead = false;
        gCheck = false;

        attackDur = 0.0f;
        attackColdown = 3.0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Set Horizontal Limits
        limitL = new Vector2(transform.position.x - patrolDistance, transform.position.y);
        limitR = new Vector2(transform.position.x + patrolDistance, transform.position.y);


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
        ProjectilePrefab = (GameObject)Resources.Load("Prefabs/Projectile", typeof(GameObject));
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



    protected override void Mouvement(char Action)
    {

        switch (Action)
        {
            case 'W':

                waitTime();

                break;

            case 'L':

                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                myRb.velocity = new Vector2(-enemyMouvSpeed, 0);
                getDir();

                break;

            case 'R':

                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                myRb.velocity = new Vector2(enemyMouvSpeed, 0);
                getDir();

                break;

        }
    }

    protected override void attack()
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

                Instantiate(ProjectilePrefab, this.transform.position, this.transform.rotation);
                attackDur = 0.0f;
                isAttacking = false;

            }
        }

    }
    


    public override void Dies()
    {
        isDead = true;
        myCC.isTrigger = true;
        myAni.SetBool("Dead", true);
        myRb.velocity = Vector2.zero;
        myRb.isKinematic = false;
        myRb.gravityScale = 2;
    }

   



}
