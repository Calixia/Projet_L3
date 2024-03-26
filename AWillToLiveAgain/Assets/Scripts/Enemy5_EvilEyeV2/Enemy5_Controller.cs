using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy5_Controller : Enemy4_Controller
{
    //Variables to start 

    //Horizontal mouvement Limits
    //public Vector2 limitL, limitR;

   // public Rigidbody2D myRb;
    //public Animator myAni;
    //public CircleCollider2D myCC;
    //public BoxCollider2D myBxC;
    //public LayerMask theGroundMask;
    //public GameObject thePlayer;
    private Enemy5_Interactions thisInteractionManager;
    //private AnimationClip[] Clips;



    //Parametrage

    //Possible actions : W - Waiting, L - Going Left, R - Going Right 
   // public char currentAction = 'W';
    //public char nextAction = 'L';

        //berzier curve attack
        [SerializeField] float AttackSpeed = 0.5f;
        public float t = 0f;
        private Vector3 P0 = Vector2.zero, P1 = Vector2.zero, P2 = Vector2.zero;
        
    //public int Health = 2;
    private float gonnaAtkDur = 0.0f;
   

    //Status
   // public bool isAttacking = false;
   // public bool isDead = false;
    //private bool gCheck = false;
    //private bool toDestroy = false;
    //public bool isHit = false;


    //Timers-Coldowns
    //public float attackDur = 0.0f;
    //private float attackColdown = 3.0f;
    private float timerToAttack = 0.0f;
    private float staggerTimer = 0.0f;
   

    private void Awake()
    {
        currentAction = 'W';
        nextAction = 'L';
        Health = 2;
        patrolDistance = 8;
        enemyMouvSpeed = 5;

        NearDirToPLayer = Vector2.zero;

        isAttacking = false;
        isDead = false;
        gCheck = false;
        isHit = false;
        toDestroy = false;

        attackDur = 0.0f;
        attackColdown = 3.0f;
        timer = 0.0f;
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
        thisInteractionManager = GetComponent<Enemy5_Interactions>();

        theGroundMask = LayerMask.GetMask("Ground");

        getToAttackDuration();

    }

    private void getToAttackDuration()
    {
        Clips = myAni.runtimeAnimatorController.animationClips;

        foreach (AnimationClip clip in Clips)
        {
            if (clip.name == "Enm5_ToAttack")
            {
                gonnaAtkDur = clip.length;


            }

     
        }

    }


    private void FixedUpdate()
    {
        

        
        

        if(isDead)
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


        if (isHit)
        {
            hittedColdown();

        }


        // if the player has been spotted by the enemy and is not that far, attack else dont attack

        if (thePlayer != null && !isDead && !isHit)
        {

            attack();
        }


        if (!isDead && !isHit)
        {
            //if not dead then move
            Mouvement(currentAction);
        }

    }



    // Update is called once per frame
    void Update()
    {
        if (!isAttacking)
        {
            gCheck = thisInteractionManager.groundChck();
        }


        if(!isAttacking && thePlayer != null)
        {
            P1 = new Vector2(thePlayer.transform.position.x, thePlayer.transform.position.y - 9f);
        }

        if (Health == 0 && !isDead)
        {
            Dies();
        }

    }

 

    

    protected override void getDirNearestPlayer()
    {


        if (thePlayer.transform.position.x < this.transform.position.x)
        {
            NearDirToPLayer = new Vector2(-1, 0);

        }
        else
        {
            NearDirToPLayer = new Vector2(1, 0);

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
                    myRb.velocity = new Vector2(8f * NearDirToPLayer.x, myRb.velocity.y);

                }
                else
                {
                    myRb.velocity = Vector2.zero;
                }



                break;

        }
    }

    protected override void attack()
    {

        if (!isAttacking && Vector3.Distance(transform.position, thePlayer.transform.position) > 12f && timerToAttack == 0.0f)
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
                    if (timerToAttack == 0.0f)
                    {
                        myAni.SetTrigger("GoingToAtk");
                    }
                    goingToAttack();


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
                    myAni.SetBool("Atk", false);
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
    

    private void goingToAttack()
    {
        timerToAttack += Time.deltaTime;

        if ( timerToAttack > gonnaAtkDur)
        {

            myAni.SetBool("Atk", true);
            isAttacking = true;
            attackColdown = 0f;
            timerToAttack = 0.0f;

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

    public void Hitted()
    {
        myRb.velocity = Vector2.zero;
        isHit = true;
        myAni.SetTrigger("Hitted");
        timer = 0f;
        nextAction = currentAction;
        currentAction = 'W';

    }

    private void hittedColdown()
    {
        staggerTimer += Time.deltaTime;

        if (staggerTimer > 1.5f)
        {
            isHit = false;
            currentAction = nextAction;
            nextAction = 'W';
            staggerTimer = 0.0f;

            
        }
    }


    public override void Dies()
    {
        isDead = true;
        isAttacking = false;
        myCC.isTrigger = true;
        myAni.SetBool("Dead", true);
        myRb.velocity = Vector2.zero;
        myRb.isKinematic = false;
        myRb.gravityScale = 2;
    }

    public override void getDir()
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
