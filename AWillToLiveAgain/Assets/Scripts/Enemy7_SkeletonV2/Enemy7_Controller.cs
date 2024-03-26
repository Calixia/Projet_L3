using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class Enemy7_Controller : Enemy1IA_scrpt
{

    //Variables to start 

    public BoxCollider2D myBxTrg; // box collider that is a trigger

    private Enemy7_Interactions thisInteractionManager; // Script that manages interaction manager
    public GameObject fantomPrefab; // Fantom to spawn if the conditions are met


    //Parametrage

    [SerializeField]private bool spawnFantome;

    public Vector3 playerPosCatch;
 
    //Timers-Coldowns
    public float attackTimer = 0.0f;
    private float hitTimer = 0.0f;
    private float hitDur = 0.0f;

    private void Awake()
    {
        //Parametrage
        patrolDistance = 7;
        enemyMouvSpeed = 7;
        currentAction = 'W';
        nextAction = 'L';

        //private Vector2 NearDirToPLayer = Vector2.zero;

        //Status
        isAttacking = false;
        isDead = false;
        Health = 2;

        //Edge Check
        edgCheck = false;
        //Groung Check
        gCheck = false;
        //Obstacule Check
        obsCheck = false;

        //Timers-Coldowns
        waitTimer = 0.0f;
        attackDur = 0.0f;
        timerToDestroy = 0.0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Set Horizontal Limits
        limitL = new Vector2(transform.position.x - patrolDistance, transform.position.y);
        limitR = new Vector2(transform.position.x + patrolDistance, transform.position.y);

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
        thisInteractionManager = GetComponent<Enemy7_Interactions>();
        theGroundMask = LayerMask.GetMask("Ground");
        fantomPrefab = (GameObject)Resources.Load("Prefabs/Enemy2", typeof(GameObject)); //Find fantome prefab from files
        getAnimationDur();

    }


    private void getAnimationDur()
    {
        //Fonction to find the animations and get their duration
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


        if (isDead)
        {
            gonnaDestroy();
        }






    }




    private void FixedUpdate()
    {
        if (!isDead)
        {
            edgCheck = thisInteractionManager.edgeCheck();
            gCheck = thisInteractionManager.groundChck();
            obsCheck = thisInteractionManager.obstacleCheck();
            Interactions();
        }

        if (gCheck && !myRb.isKinematic)
        {
            myRb.velocity = Vector3.zero;
            myRb.isKinematic = true;
        }



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
        //time of iinvulnerability  the enemy takes after being hit
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
        //function called when hitted
        // sets its velocity to zero, sets the hit animation, and makes the enemy to wait after the hit coldown timer ends.
        myRb.velocity = Vector3.zero;
        isHit = true;
        myAni.SetTrigger("Hitted");
        waitTimer = 0f;
        currentAction = 'W';
        nextAction = 'L';
        myBxC.excludeLayers = LayerMask.GetMask("Player");

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

    public override void Dies()
    {
        //Method called when the enemi is killed. it sets the dead animation, make the enemy collider go trigger, removes gravity so it doesnt fall, set velocity to zero
        //And if at the start of the creation of the enemy  the variable "spawnFantome" is true then spawn a enemy2 Fantome

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


            case 'T':
                // Towards Player position catched
                toAttack();
                break;

            case 'A':

                attack();

                break;
        }
    }

    private void toAttack()
    {

        if (Mathf.Abs(playerPosCatch.x - this.transform.position.x) > 1f)
        {
            Debug.Log("going towards player");
            objectDirection();
            getDirNearestPlayer();
            myRb.velocity = new Vector2(12f * NearDirToPLayer.x, myRb.velocity.y);
        }
        else
        {
            myRb.velocity = Vector2.zero;
            myAni.SetTrigger("Attack");
            currentAction = 'A';
        }

    }

    protected override void attack()
    {

        //methode qui prend en compte la duration de l'animation de l'attaque pour synchroniser avec l'activation de attack Boxes


        attackTimer += Time.deltaTime;
        if (attackTimer > attackDur - 0.5 && attackTimer < (attackDur - 0.2))
        {
            isAttacking = true;
        }
        else if (attackTimer > (attackDur - 0.2))
        {
            isAttacking = false;
            attackTimer = 0.0f;
            playerPosCatch = Vector2.zero;
            myBxTrg.enabled = true;
            getDir();

        }

    }

    protected override void getDirNearestPlayer()
    {
        // Get nearest vector direction to the player, in this case as the enemy only moves sideways , we only need right and left mouvement, x coordinates

        if (playerPosCatch.x < this.transform.position.x)
        {
            NearDirToPLayer = new Vector2(-1, 0);

        }
        else
        {
            NearDirToPLayer = new Vector2(1, 0);

        }

    }

    protected override void edgeFallPrevention()
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

    protected override void obstaclePrevention()
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

    protected override void objectDirection()
    {

        if (myRb.velocity.x < 0f)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }

        if (myRb.velocity.x > 0f)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }


        if (myRb.velocity.x == 0f)
        {
            if (playerPosCatch.x < this.transform.position.x)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }

    }

}
