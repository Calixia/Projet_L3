using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy1IA_scrpt : MonoBehaviour
{

    //Variables to start 

        //Horizontal mouvement Limits
    public Vector2 limitL, limitR;

    public Rigidbody2D myRb;
    public Animator myAni;
    public BoxCollider2D myBxC;
    public LayerMask theGroundMask;
    private Enemy1_Ineractions InteractionManager;
    private AnimationClip[] Clips;


    //Parametrage

     //Possible actions : W - Waiting, A - Attacking, L - Going Left, R - Going Right 
    public char currentAction = 'W';
    public char nextAction = 'L';
    public int Health = 1;

    //Status
    public bool isAttacking = false;
    public bool isDead = false;

        //Edge Check
    private bool edgCheck = false;
        //Groung Check
    private bool gCheck = false;
        //Obstacule Check
    private bool obsCheck = false;


    //Timers-Coldowns
    public float attackDur = 0.0f;
    public float timer = 0.0f;
    private float timerToDestroy = 0.0f;


    // Start is called before the first frame update
    void Start()
    {
        //Set Horizontal Limits
        limitL = new Vector2(transform.position.x - 4, transform.position.y);
        limitR = new Vector2(transform.position.x + 4, transform.position.y);

        //Ignore other enemies collisions
        Physics2D.IgnoreLayerCollision(8, 8);
        
        

        //Chose a Random Direction to go First
        int rand = UnityEngine.Random.Range(0, 2);
        if (rand == 0){
            nextAction = 'L';
        }
        else{
            nextAction = 'R';
        }
        
        myRb = GetComponent<Rigidbody2D>();
        myAni = GetComponent<Animator>();
        InteractionManager = GetComponent<Enemy1_Ineractions>();
        theGroundMask = LayerMask.GetMask("Ground");
        getAttackDur();

    }

   
    private void getAttackDur()
    {
        //Fonction to find the attack animation and calculate duration
            //We take all the clips in the game
        Clips = myAni.runtimeAnimatorController.animationClips;

        foreach (AnimationClip clip in Clips)
        {

            if (clip.name == "Attack_en1")
            {   
                //We find the animation clip and stock the duration taht the attack must have to match the animation
                attackDur = clip.length;

                //Debug.Log("attack duration:");
                //Debug.Log(attackDur);

                break;

            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        /*Because the mob always spawns on air to prevent being stuck in the map , every frame we recalculate
             the y postion of the limits */
        limitL = new Vector2(limitL.x, transform.position.y);
        limitR = new Vector2(limitR.x, transform.position.y);

        if (Health == 0 && !isDead)
        {
            Dies();
        }


        edgCheck = InteractionManager.edgeCheck();
        gCheck = InteractionManager.groundChck();
        obsCheck = InteractionManager.obstacleCheck();


        if (!gCheck)
        {
  
            if(currentAction != 'W')
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

        Interactions();


        if (!isDead)
        {
            //if not dead then do something
            Mouvement(currentAction);

            //wakling animation based on the velocity of its rigidBody
            myAni.SetFloat("walk", Mathf.Abs(myRb.velocity.x));
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


        if(currentAction == 'A')
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

    public void Dies()
    {
        Debug.Log("skeleton is killed");

        myAni.SetTrigger("IsKilled");
        myBxC.isTrigger = true;
        myRb.velocity = Vector3.zero;
        myRb.gravityScale = 0f;
        isDead = true;
    }
    private void Mouvement(char Action) {
        
    

        switch(Action)
        {
            case 'W' :
              
                waitTime();

                break;

            case 'L' :
              
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


    private void gonnaDestroy()
    {
        timerToDestroy += Time.deltaTime;

        if (timerToDestroy > 4)
        {
            Destroy(this.gameObject);
        }
    }


}

