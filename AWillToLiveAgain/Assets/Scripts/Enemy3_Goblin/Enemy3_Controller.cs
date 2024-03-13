using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Fix on air mouvement, attack manager
/// </summary>
public class Enemy3 : MonoBehaviour
{
    // at start
    public GameObject thePlayer;
    public Rigidbody2D myRb;
    public Animator myAni;
    public BoxCollider2D myBxC;
    private BoxCollider2D myBxTrg;
    public LayerMask theGroundMask;
    private Enemy3_Interactions InteractionManager;
    public Vector2 limitL, limitR;


    //Status
    public char currentAction = 'W';
    public char nextAction = 'L';
    private int Health = 2;

    private bool edgCheck = false;
    private bool obsCheck = false;
    private bool gCheck = false;
    public bool isDead = false;
    private bool Onsight = false;
    private bool isAttacking = false;
    private bool Jump = false;


    //parametrage
    public Vector2 NearDirToPLayer = Vector2.zero;
    private Vector2 playerJumped, playerLanded = Vector2.zero;
    public Player_scrpt playerController;

        //Jump - Berzier cuadratic
        [SerializeField] private float jumpSpeed = 0.8f;
        private float t = 0f;
       // private Vector3 P0;
        //private Vector3 P1;
        //private Vector3 P2;


    private float timer = 0.0f;

    private bool dash = false;

    // Start is called before the first frame update
    void Start()
    {
        thePlayer = null;

        limitL = new Vector2(transform.position.x - 4, transform.position.y);
        limitR = new Vector2(transform.position.x + 4, transform.position.y);

        //Physics2D.IgnoreLayerCollision(8, 8, true);

        myRb = GetComponent<Rigidbody2D>();
        myBxC = GetComponent<BoxCollider2D>();
        myBxTrg = GetComponents<BoxCollider2D>()[1];

        myAni = GetComponent<Animator>();
        InteractionManager = GetComponent<Enemy3_Interactions>();

        theGroundMask = LayerMask.GetMask("Ground");
    }

    // Update is called once per frame
    void Update()
    {
        //objectDirection();


        if (!isAttacking)
        {
            limitL = new Vector2(limitL.x, transform.position.y);
            limitR = new Vector2(limitR.x, transform.position.y);
        }

        if (currentAction == 'A')
        {
            myBxTrg.enabled = false;
            isAttacking = true;
            objectDirection();
        }


        edgCheck = InteractionManager.edgeCheck();
        gCheck = InteractionManager.groundChck();
        obsCheck = InteractionManager.obstacleCheck();

        //dash = Input.GetKeyDown(KeyCode.T);

        if (!isAttacking)
        {
            Interactions();
        }

        
        if (thePlayer != null && currentAction != 'J')
        {
            Onsight = InteractionManager.PlayerOnSight();

            //prototype
            if (!Onsight)
            {
                currentAction = 'A';
                nextAction = 'W';
            }



        }




        if (!isDead)
        {
            myAni.SetFloat("Running", Mathf.Abs(myRb.velocity.x));
            Mouvement(currentAction);
        }


    }

    private void Interactions()
    {

        if (!gCheck )
        {
            if (currentAction != 'W')
            {
                //Just to be sure, if for any reason the GameObject it's in the air it'll wait
                nextAction = currentAction;
                currentAction = 'W';
            }
        }


        if (!edgCheck)
        {
            //check current dir
            edgeFallPrevention();

        }

        if (obsCheck)
        {
            obstaclePrevention();
        }


    }

    private void edgeFallPrevention()
    {
        switch (currentAction)
        {

            case 'L':
                myRb.velocity = Vector3.zero;

                limitL = new Vector2(transform.position.x - 0.2f, limitL.y);
                currentAction = 'R';
                //nextDir = 'W';


                break;

            case 'R':
                myRb.velocity = Vector3.zero;

                limitR = new Vector2(transform.position.x + 0.2f, limitL.y);
                currentAction = 'L';
                //nextDir = 'L';

                break;
        }
    }

    private void obstaclePrevention()
    {
        switch (currentAction)
        {

            case 'L':
                myRb.velocity = Vector3.zero;
                limitL = new Vector2(transform.position.x - 0.3f, limitL.y);
                currentAction = 'R';

                break;

            case 'R':
                myRb.velocity = Vector3.zero;
                limitR = new Vector2(transform.position.x + 0.3f, limitL.y);
                currentAction = 'L';

                break;
        }

    }
    



    private void objectDirection()
    {

        if (myRb.velocity.x < 0)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        
        
        if(myRb.velocity.x > 0)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

    }


    private void FixedUpdate()
    {
        if (dash)
        {
            myRb.AddForce(Vector2.right *12, ForceMode2D.Impulse);

        }

        if (currentAction == 'A')
        {
            getDirNearestPlayer();


            if (playerJumped == Vector2.zero || playerLanded == Vector2.zero)
            {

                if (!playerController.gCheck && playerJumped == Vector2.zero)
                {//catches if the player jumps  for the first time and saves the position
                    Debug.Log("Goblin has registered player jumping");
                    playerJumped = thePlayer.transform.position;
                }

                if (playerController.gCheck && playerJumped != Vector2.zero)
                {//Catches if the player has landed after jumping
                    if (thePlayer.transform.position.y > playerJumped.y)
                    {
                        Debug.Log("Goblin has registered player landed");
                        playerLanded = thePlayer.transform.position;

                        currentAction = 'J';
                        nextAction = 'A';

                    }
                    else
                    {
                        //the player landed in the same height he jumped
                        // so  vectors are reset
                        playerJumped = Vector2.zero;
                        playerLanded = Vector2.zero;
                    }

                }
            }



        }


    }


    private void guidedJump()
    {
        Vector3 P0 = playerJumped;
        Vector3 P2 = playerLanded;
        Vector3 P1;

        if (P0.x > P2.x)
        {
            P1 = new Vector3(P2.x + (Vector3.Distance(P0, P2) / 2), P2.y + 5, 0);

        }
        else
        {
            P1 = new Vector3(P2.x - (Vector3.Distance(P0, P2) / 2), P2.y + 5, 0);
        }


        if (t < 1f)
        {
            transform.position = P1 + Mathf.Pow((1 - t), 2) * (P0 - P1) + Mathf.Pow(t, 2) * (P2 - P1);

            t = t + jumpSpeed * Time.deltaTime;

        }
        else
        {
            playerJumped = Vector2.zero;
            playerLanded = Vector2.zero;
            t = 0f;
            Jump = false;
            //control
            currentAction = 'W';
            nextAction = 'W';
        }
    }

    private void getDirNearestPlayer()
    {


        if(thePlayer.transform.position.x < this.transform.position.x)
        {
            NearDirToPLayer = new Vector2(-1, 0);

        }
        else
        {
            NearDirToPLayer = new Vector2(1, 0);

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
                //transform.Rotate(0f, 180f,0f);
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                //Debug.Log(transform.rotation.y);
                myRb.velocity = new Vector2(-5, 0);
                getDir();

                break;

            case 'R':
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                //Debug.Log(transform.rotation.y);
                myRb.velocity = new Vector2(5, 0);
                getDir();

                break;

            case 'A':
                if (playerJumped == Vector2.zero && playerLanded == Vector2.zero)
                {

                    attack();
                }
                else
                {
                    myRb.velocity = Vector2.zero;
                }

                break;

            case 'J':
                Debug.Log(Vector3.Distance(transform.position, playerJumped));

                int dir = 1;

                if (transform.position.x > playerJumped.x)
                {
                    dir = -1;

                }


                if (Vector3.Distance(transform.position, playerJumped) > 0.7f && !Jump)
                {

                    myRb.velocity = new Vector2(8f * dir , myRb.velocity.y);

                }else {
                    //myRb.velocity = Vector2.zero;
                    Jump = true;
                    guidedJump(); 
                 }
                break;
        }
    }


    private void attack()
    {

        if (myRb.velocity.x < 6f && myRb.velocity.x > -6f)
        {
            myRb.AddForce(NearDirToPLayer * 3f);
        }
        else
        {

            myRb.velocity = new Vector2(8f * NearDirToPLayer.x, myRb.velocity.y);
        }


    }


    private void gojump()
    {
        

            myRb.gravityScale = 5;

            myRb.velocity = new Vector2(myRb.velocity.x, 0);

            myRb.AddForce(Vector2.up * 20, ForceMode2D.Impulse);
    }


    private void waitTime()
    {
        //Debug.Log("is waiting");
        timer += Time.deltaTime;
        if (timer > 1.5f)
        {
            currentAction = nextAction;
            nextAction = 'W';
            timer = 0.0f;
            //Debug.Log("Enemy Waiting ends");

        }
    }

    private void getDir()
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



    private void OnDrawGizmos()
    {

        float BoxCastDir = 0;
        float RayCastDir = 0;

        if (transform.rotation.y == 0)
        {
            BoxCastDir = -1;
            RayCastDir = 1;

        }
        else
        {
            BoxCastDir = 1;
            RayCastDir = -1;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(limitL, new Vector2(0.5f, 0.5f));
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(limitR, new Vector2(0.5f, 0.5f));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(myBxC.bounds.center - new Vector3(0.75f * BoxCastDir, 0.6f), myBxC.bounds.size - new Vector3(0f,0.9f));

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(myBxC.bounds.center - new Vector3(0, 0.55f), myBxC.bounds.size - new Vector3(0f,1f));


        Gizmos.color = Color.red;
        Gizmos.DrawLine(myBxC.bounds.center + new Vector3(0.4f * RayCastDir, 0f), myBxC.bounds.center + new Vector3(0.9f * RayCastDir, 0f));
    }




}
