using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy5_Controller : MonoBehaviour
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
    private Enemy5_Interactions InteractionManager;
    private AnimationClip[] Clips;



    //Parametrage

    //Possible actions : W - Waiting, L - Going Left, R - Going Right 
    public char currentAction = 'W';
    public char nextAction = 'L';

    private Vector2 NearDirToPlayer = Vector2.zero;

        //berzier curve attack
        [SerializeField] float AttackSpeed = 0.5f;
        public float t = 0f;
        private Vector3 P0 = Vector2.zero, P1 = Vector2.zero, P2 = Vector2.zero;
        
    public int Health = 2;


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
        limitL = new Vector2(transform.position.x - 8, transform.position.y);
        limitR = new Vector2(transform.position.x + 8, transform.position.y);


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
        InteractionManager = GetComponent<Enemy5_Interactions>();

        theGroundMask = LayerMask.GetMask("Ground");

    }

    private void FixedUpdate()
    {
        

        


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
                Destroy();
            }

        }


        // if the player has been spotted by the enemy and is not that far, attack else dont attack

        if (thePlayer != null && !isDead)
        {

            attack();
        }


    }



    // Update is called once per frame
    void Update()
    {
        if (!isAttacking)
        {
            gCheck = InteractionManager.groundChck();
        }


        if(!isAttacking && thePlayer != null)
        {
            P1 = new Vector2(thePlayer.transform.position.x, thePlayer.transform.position.y - 8f);
        }

        if (Health == 0 && !isDead)
        {
            Dies();
        }

    }

    private void objectDirection()
    {

        if (myRb.velocity.x < 0)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }


        if (myRb.velocity.x > 0)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }


    }


    private void Destroy()
    {
        timer += Time.deltaTime;

        if (timer > 4)
        {
            Destroy(this.gameObject);
        }
    }

    private void getDirNearestPlayer()
    {


        if (thePlayer.transform.position.x < this.transform.position.x)
        {
            NearDirToPlayer = new Vector2(-1, 0);

        }
        else
        {
            NearDirToPlayer = new Vector2(1, 0);

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


            case 'C':
                //case get close to player 
                getDirNearestPlayer();
                objectDirection();


                if (Mathf.Abs(thePlayer.transform.position.x - transform.position.x) > 0.1f)
                {
                    myRb.velocity = new Vector2(6f * NearDirToPlayer.x, myRb.velocity.y);

                }
                else
                {
                    myRb.velocity = Vector2.zero;
                }



                break;

        }
    }

    private void attack()
    {

        if (!isAttacking && Vector3.Distance(transform.position, thePlayer.transform.position) > 12f)
        {
            nextAction = currentAction;
            currentAction = 'C';

        }
        else
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

                    P0 = transform.position;

                    if (P0.x < P1.x)
                    {
                        P2 = new Vector2(transform.position.x + 8f, transform.position.y);

                    }
                    else
                    {
                        P2 = new Vector2(transform.position.x - 8f, transform.position.y);
                    }

                }

            }

            if (isAttacking)
            {

                myRb.velocity = Vector2.zero;



                //berzier curve attack


                if (t < 1)
                {
                    transform.position = P1 + Mathf.Pow((1 - t), 2) * (P0 - P1) + Mathf.Pow(t, 2) * (P2 - P1);

                    t = t + AttackSpeed * Time.deltaTime;

                }
                else
                {

                    t = 0f;
                    isAttacking = false;
                    transform.position = new Vector2(transform.position.x, P2.y);
                    limitL = new Vector2(transform.position.x - 8, limitL.y);
                    limitR = new Vector2(transform.position.x + 8, limitR.y);

                    /*
                    currentAction = 'A';
                    nextAction = 'W';
                    */
                }

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
        isAttacking = false;
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




    }


    private void OnDrawGizmos()
    {
       

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(P0, 0.2f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(P1, 0.2f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(P2, 0.2f);
    }

}
