using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class Enemy7_Controller : MonoBehaviour
{

    //Variables to start 

    //Horizontal mouvement Limits
    public Vector2 limitL, limitR;

    public Rigidbody2D myRb;
    public Animator myAni;
    public BoxCollider2D myBxC;
    public BoxCollider2D myBxTrg;
    public LayerMask theGroundMask;
    private Enemy7_Interactions InteractionManager;
    private AnimationClip[] Clips;
    public GameObject fantomPrefab;


    //Parametrage

    //Possible actions : W - Waiting, A - Attacking, L - Going Left, R - Going Right 
    public char currentAction = 'W';
    public char nextAction = 'L';

    [SerializeField]private bool spawnFantome;
    
    //Status
    public bool isAttacking = false;
    public bool isDead = false;
    public int Health = 2;

    //Edge Check
    private bool edgCheck = false;
    //Groung Check
    private bool gCheck = false;
    //Obstacule Check
    private bool obsCheck = false;

    public bool isHit;


    //Timers-Coldowns
    public float waitTimer = 0.0f;
    public float attackDur = 0.0f;
    public float attackTimer = 0.0f;
    private float hitTimer = 0.0f;
    private float hitDur = 0.0f;
    private float timerToDestroy = 0.0f;


    // Start is called before the first frame update
    void Start()
    {
        //Set Horizontal Limits
        limitL = new Vector2(transform.position.x - 7, transform.position.y);
        limitR = new Vector2(transform.position.x + 7, transform.position.y);

        //Ignore other enemies collisions
        Physics2D.IgnoreLayerCollision(8, 8);



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

        //Chose if to spawn a fantom when it dies or not
        // probability of 40%
        int rand2 = UnityEngine.Random.Range(0, 11);
        if (rand2 < 6)
        {
            spawnFantome = true;
        }
        else
        {
            spawnFantome = false;
        }

        myRb = GetComponent<Rigidbody2D>();
        myAni = GetComponent<Animator>();
        InteractionManager = GetComponent<Enemy7_Interactions>();
        theGroundMask = LayerMask.GetMask("Ground");
        fantomPrefab = (GameObject)Resources.Load("Prefabs/Enemy2", typeof(GameObject));
        getAnimationDur();

    }


    private void getAnimationDur()
    {
        //Fonction to find the attack animation and calculate duration
        //We take all the clips in the game
        Clips = myAni.runtimeAnimatorController.animationClips;

        foreach (AnimationClip clip in Clips)
        {

            if (clip.name == "Enm7_Attack")
            {
                //We find the animation clip and stock the duration that the attack must have to match the animation
                attackDur = clip.length;

                

            }

            if (clip.name == "Enm7_Hit")
            {
                //We find the animation clip and stock the duration that the hit must have to match the animation
                hitDur = clip.length;

                
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


        if (!gCheck)
        {
            /*Because the mob always spawns on air to prevent being stuck in the map , every frame that he is not on the ground we recalculate
             the y postion of the limits */
            limitL = new Vector2(limitL.x, transform.position.y);
            limitR = new Vector2(limitR.x, transform.position.y);

            if (currentAction != 'W')
            {
                //Just to be sure, if for any reason the GameObject it's in the air it'll wait
                nextAction = currentAction;
                currentAction = 'W';
            }

        }

        if(gCheck && !myRb.isKinematic)
        {
            myRb.isKinematic = true;
        }



        if (isDead)
        {
            gonnaDestroy();
        }






    }


    private void Interactions()
    {

        if (!edgCheck)
        {
            //If edge check is fals it would mean that if the enemy continues to walk it will fall
            //There for it will call a function to prevent from falling from plataforms edges
            edgeFallPrevention();

        }

        if (obsCheck)
        {
            //if it detects an obstacle created by the map itself then its calls a function to prevent
            //the enemy to walk forever agains a wall
            obstaclePrevention();
        }



    }


    private void FixedUpdate()
    {
        if (!isDead)
        {
            edgCheck = InteractionManager.edgeCheck();
            gCheck = InteractionManager.groundChck();
            obsCheck = InteractionManager.obstacleCheck();
        }


        Interactions();


        if (!isDead && !isHit)
        {
            //if not dead then do something
            Mouvement(currentAction);

            //wakling animation based on the velocity of its rigidBody
            myAni.SetFloat("walk", Mathf.Abs(myRb.velocity.x));
        }

        if (isHit)
        {
            hitColdown();
        }


    }


    private void hitColdown()
    {
        hitTimer += Time.deltaTime;
        if (hitTimer > hitDur)
        {
            isHit = false;
            hitTimer = 0;
            myBxC.excludeLayers = LayerMask.GetMask("Nothing");
            if (!myBxTrg.enabled)
            {
                myBxTrg.enabled = true;
            }

        }

    }


    public void Hitted()
    {
        myRb.velocity = Vector3.zero;
        isHit = true;
        myAni.SetTrigger("Hitted");
        waitTimer = 0f;
        currentAction = 'W';
        nextAction = 'L';
        myBxC.excludeLayers = LayerMask.GetMask("Player");

    }



    private void waitTime()
    {

        //Debug.Log("is waiting");
        waitTimer += Time.deltaTime;
        if (waitTimer > 1.5f)
        {
            //Wait for 1.5 seconds then change action
            currentAction = nextAction;
            nextAction = 'W';
            waitTimer = 0.0f;
            //Debug.Log("Enemy Waiting ends");

        }
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
            myRb.velocity = Vector2.zero;
            limitL = new Vector2(transform.position.x - 7, transform.position.y);
            limitR = new Vector2(transform.position.x + 7, transform.position.y);
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

    public void Dies()
    {
        Debug.Log("skeleton is killed");

        myAni.SetTrigger("IsKilled");
        myBxC.isTrigger = true;
        myRb.velocity = Vector3.zero;
        myRb.gravityScale = 0f;
        isDead = true;

        if (spawnFantome)
        {
            Instantiate(fantomPrefab,this.transform.position,transform.rotation);
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
                myRb.velocity = new Vector2(-7, 0);
                getDir();

                break;

            case 'R':

                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                myRb.velocity = new Vector2(7, 0);
                getDir();

                break;


            case 'T':
                // Towards Player position catched
                InteractionManager.toAttack();
                break;

            case 'A':

                InteractionManager.attack();

                break;
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

            case 'T':

                if (myRb.velocity.x > 0)
                {
                    myRb.velocity = Vector3.zero;
                    limitR = new Vector2(transform.position.x + 0.3f, limitL.y);
                    currentAction = 'L';

                }
                else
                {
                    myRb.velocity = Vector3.zero;
                    limitL = new Vector2(transform.position.x - 0.3f, limitL.y);
                    currentAction = 'R';
                }

                myBxTrg.enabled = true;
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

            case 'T':

                if(myRb.velocity.x > 0)
                {
                    myRb.velocity = Vector3.zero;
                    limitR = new Vector2(transform.position.x + 0.3f, limitL.y);
                    currentAction = 'L';

                }
                else
                {
                    myRb.velocity = Vector3.zero;
                    limitL = new Vector2(transform.position.x - 0.3f, limitL.y);
                    currentAction = 'R';
                }

                myBxTrg.enabled = true;
                break;
                
        }

    }


    private void gonnaDestroy()
    {
        timerToDestroy += Time.deltaTime;

        if (timerToDestroy > 4)
        {
            Destroy(this.gameObject);
        }
    }
}
