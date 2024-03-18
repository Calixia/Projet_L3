using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Enemy3 : MonoBehaviour
{
    // at start
    public GameObject thePlayer;
    public Rigidbody2D myRb;
    public Animator myAni;
    public BoxCollider2D myBxC;
    public BoxCollider2D myBxTrg;
    public LayerMask theGroundMask;

    private Enemy3_Interactions InteractionManager;
    private Enemy3_Jump Jump_Script;

    public Vector2 limitL, limitR;
    private AnimationClip[] Clips;


    //Status
    public char currentAction = 'W';
    public char nextAction = 'L';
    public int Health = 2;

    private bool edgCheck = false;
    private bool obsCheck = false;
    private bool gCheck = false;

    private bool onColdown = false;

    private bool notOnsight = false;
    public bool isPursuing = false;
    public bool isAttacking = false;
    public bool Jump = false;
    public bool isHit = false;


    public bool isDead = false;


    //parametrage
    public Vector2 NearDirToPLayer = Vector2.zero;
    public Player_scrpt playerController;

    private float staggerDur = 0f;
    private float attackDur = 0f;

    //Jump - Berzier cuadratic
    [SerializeField] float staggerSpeed = 0.5f;
        private Vector3 P0hit = Vector2.zero, P1hit = Vector2.zero, P2hit = Vector2.zero;
        private float t2 = 0f;


    //timers
    private float waitTimer = 0.0f;
    public float deBugStuck = 0.0f;
    private float attackTimer = 0.0f;
    private float atkColdown =0.0f;
    private float toDestroy = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        thePlayer = null;

        limitL = new Vector2(transform.position.x - 4, transform.position.y);
        limitR = new Vector2(transform.position.x + 4, transform.position.y);


        myRb = GetComponent<Rigidbody2D>();
        myBxC = GetComponent<BoxCollider2D>();
        myBxTrg = GetComponents<BoxCollider2D>()[1];

        myAni = GetComponent<Animator>();
        InteractionManager = GetComponent<Enemy3_Interactions>();
        Jump_Script = GetComponent<Enemy3_Jump>();

        theGroundMask = LayerMask.GetMask("Ground");

        getAnimDuration();
    }

    private void getAnimDuration()
    {
        Clips = myAni.runtimeAnimatorController.animationClips;

        foreach (AnimationClip clip in Clips)
        {
            if (clip.name == "Enm3_hit")
            {
                staggerDur = clip.length;
         
                
            }

            if (clip.name == "Enm3_attack")
            {
                attackDur = clip.length;

                
            }
        }

    }


    // Update is called once per frame
    void Update()
    {
        if (Health == 0 && !isDead)
        {
            Dies();

        }


        if (currentAction == 'P' || currentAction == 'J')
        {
            objectDirection();
        }
         
        

        if (!isPursuing && !isAttacking)
        {
            limitL = new Vector2(limitL.x, transform.position.y);
            limitR = new Vector2(limitR.x, transform.position.y);

            edgCheck = InteractionManager.edgeCheck();
            obsCheck = InteractionManager.obstacleCheck();

        }
     
        gCheck = InteractionManager.groundChck();
       
        
        if (thePlayer != null && currentAction != 'J' && currentAction != 'A' && (Jump_Script.playerJumped == Vector2.zero && Jump_Script.playerLanded == Vector2.zero) && !isDead)
        {
            notOnsight = InteractionManager.PlayerOnSight();

            LOS();
        }


        if(currentAction == 'J')
        {
            deBugStuck += Time.deltaTime;

            if(deBugStuck > 4f)
            {
                currentAction = 'P';
                deBugStuck = 0.0f;
            }
        }


    }

    private void Interactions()
    {

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
               


                break;

            case 'R':
                myRb.velocity = Vector3.zero;

                limitR = new Vector2(transform.position.x + 0.2f, limitL.y);
                currentAction = 'L';
          

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
    

    private void LOS()
    {
        if (notOnsight)
        {
            currentAction = 'W';
            nextAction = 'P';
        }
        //prototype
        if (!notOnsight)
        {
            currentAction = 'P';
            nextAction = 'W';

            if (waitTimer != 0.0f)
            {
                waitTimer = 0.0f;
            }

        }
    }



    private void objectDirection()
    {

        if (myRb.velocity.x < 0f)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        
        if(myRb.velocity.x > 0f)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }


        if(myRb.velocity.x == 0f)
        {
            if(thePlayer.transform.position.x < this.transform.position.x)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }



       

    }


    private void FixedUpdate()
    {

        

        if (currentAction == 'P')
        {
            getDirNearestPlayer();

            if (gCheck)
            {
                Jump_Script.resgisterJumpLanding();
            }

          
        }

        if (!isPursuing && !isAttacking)
        {

            Interactions();

        }




        if (!isHit)
        {
            
            myAni.SetFloat("Running", Mathf.Abs(myRb.velocity.x));
            Mouvement(currentAction);
        }
        
        if (isHit && !isDead)
        {
            pushBack();
        }
        

        if (isDead)
        {
            
            if (gCheck)
            {
                myRb.velocity = Vector2.zero;
                myRb.gravityScale = 0f;
                
                
                if (myAni.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !myAni.IsInTransition(0) && currentAction !='D')
                {
                    myAni.SetBool("OnGround", true);
                    myAni.SetBool("Dead", false);

                    currentAction = 'D';
                    nextAction = 'W';
                }
               

            }

        }

    }


    public void setStaggerControlPoints(int x_sign)
    {
        P0hit = transform.position;
        P2hit = new Vector2(transform.position.x + 2f * x_sign, transform.position.y);

        if (P0hit.x > P2hit.x)
        {
            P1hit = new Vector3(P2hit.x + (Vector3.Distance(P0hit, P2hit) / 2), P2hit.y + 1.5f, 0);

        }
        else
        {
            P1hit = new Vector3(P2hit.x - (Vector3.Distance(P0hit, P2hit) / 2), P2hit.y + 1.5f, 0);
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
                
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
             
                myRb.velocity = new Vector2(-5, 0);
                getDir();

                break;

            case 'R':
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                myRb.velocity = new Vector2(5, 0);
                getDir();

                break;

            case 'A':

                if(!onColdown)
                {
                    attack();

                }
                else
                {
                    attackColdown();
                }


                break;



            case 'P':

                if (Jump_Script.playerJumped == Vector2.zero && Jump_Script.playerLanded == Vector2.zero)
                {
                    pursue();
                }
                else
                {
                    myRb.velocity = Vector2.zero;
                }


                break;

            case 'J':


                int dir = 1;

                if (transform.position.x >  Jump_Script.playerJumped.x)
                {
                    dir = -1;

                }


                if (Mathf.Abs(Jump_Script.playerJumped.x - transform.position.x) > 0.7f && !Jump)
                {
                    if (!gCheck)
                    {
                        Jump_Script.playerJumped = Vector2.zero;
                        Jump_Script.playerLanded = Vector2.zero;

                        currentAction = 'P';

                    }

                    myRb.velocity = new Vector2(8f * dir , myRb.velocity.y);

                }else {

                    if (!Jump)
                    {

                        if(Mathf.Abs(Jump_Script.playerJumped.y - transform.position.y) > 1f)
                        {

                            Jump_Script.playerJumped = Vector2.zero;
                            Jump_Script.playerLanded = Vector2.zero;
                            currentAction = 'P';
                        }
                        else
                        {
                            Jump = true;
                            Jump_Script.playerJumped = this.transform.position;
                        }

                    }
                    else
                    {
                        myRb.velocity = Vector2.zero;
                        Jump_Script.guidedJump();
                    }
                   
                 }
                break;

            case 'D':

                gonnaDestroy();

                break;
        }
    }

    public void Hitted()
    {
        isHit = true;
     
        myAni.SetBool("Hitted", true);
        Jump_Script.t = 0f;
        waitTimer = 0f;
        currentAction = 'W';
        nextAction = 'W';
        myBxC.excludeLayers = LayerMask.GetMask("Player");
        Jump_Script.playerJumped = Vector2.zero;
        Jump_Script.playerLanded = Vector2.zero;
        Jump = false;
    }


    private void pushBack()
    {

        if (t2 < 1)
        {
            transform.position = P1hit + Mathf.Pow((1 - t2), 2) * (P0hit - P1hit) + Mathf.Pow(t2, 2) * (P2hit - P1hit);

            t2 = t2 + staggerSpeed * Time.deltaTime;

        }
        else
        {
            myAni.SetBool("Hitted", false);
            currentAction = 'P';
            nextAction = 'W';

            t2 = 0f;
            isHit = false;
            myBxC.excludeLayers = LayerMask.GetMask("Nothing");
        }
    }



    private void pursue()
    {
        if(Vector3.Distance(thePlayer.transform.position, this.transform.position) < 1.5f)
        {
            myRb.velocity = Vector2.zero;
            myAni.SetTrigger("Atk");
            currentAction = 'A';
            nextAction = 'P';

        }
        else
        {
            myRb.velocity = new Vector2(8f * NearDirToPLayer.x, myRb.velocity.y);
        }
    }

    private void attackColdown()
    {
        

        atkColdown += Time.deltaTime;

        if(atkColdown > 0.6f)
        {
            atkColdown = 0.0f;
            currentAction = 'P';
            nextAction = 'W';
            onColdown = false;
        }
    }

    private void attack()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer > attackDur - 0.2 && attackTimer < attackDur)
        {
            isAttacking = true;
        }
        else if (attackTimer > attackDur)
        {
            isAttacking = false;
            attackTimer = 0.0f;
            onColdown = true;
        }

    }


    private void waitTime()
    {
        waitTimer += Time.deltaTime;
        if (waitTimer > 1.2f)
        {
            currentAction = nextAction;
            nextAction = 'W';
            waitTimer = 0.0f;
    

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
    }


    public void Dies()
    {
        if(myRb.isKinematic){
           myRb.isKinematic = false;
        }

        Debug.Log("goblin dies");
        isDead = true;
        myAni.SetBool("Dead", true);
        currentAction = 'W';
        nextAction = 'W';
        myBxC.isTrigger = true;
    }

    private void gonnaDestroy()
    {
        toDestroy += Time.deltaTime;

        if (toDestroy > 4)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnDrawGizmos()
    {

        if (Jump_Script != null) { 

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Jump_Script.playerJumped, 0.2f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Jump_Script.playerLanded, 0.2f);

        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(P0hit, 0.2f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(P1hit, 0.2f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(P2hit, 0.2f);


    }




}
