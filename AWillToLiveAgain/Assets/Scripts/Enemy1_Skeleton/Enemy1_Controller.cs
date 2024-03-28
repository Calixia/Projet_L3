using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy1IA_scrpt : Enemy
{

    //Variables to start 
   
    public BoxCollider2D myBxC; // Box Collider
    private Enemy1_Ineractions InteractionManager; // Script that manages interaction manager
   
    //Parametrage
    protected int patrolDistance = 4;
    protected float enemyMouvSpeed = 5;

    //Status
        //Edge Check
    protected bool edgCheck = false;
    protected bool obsCheck = false;

    private void Awake()
    {
        //Possible actions : W - Waiting, A - Attacking, L - Going Left, R - Going Right 
        currentAction = 'W';
        nextAction = 'L';
        Health = 1;



        //Status
        isAttacking = false;
        isDead = false;

        //Groung Check
        gCheck = false;

        //Timers-Coldowns
        attackDur = 0.0f;
        waitTimer = 0.0f;
        timerToDestroy = 0.0f;

        theGroundMask = LayerMask.GetMask("Ground");
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
        if (rand == 0){
            nextAction = 'L';
        }
        else{
            nextAction = 'R';
        }
        
        myRb = GetComponent<Rigidbody2D>();
        myAni = GetComponent<Animator>();
        InteractionManager = GetComponent<Enemy1_Ineractions>();
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
      

        if (Health == 0 && !isDead)
        {
            Dies();
        }


        if (!gCheck)
        {
            /*Because the mob always spawns on air to prevent being stuck in the map , every frame we recalculate
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
        {//if the enemy is dead, start timer to destroy
            gonnaDestroy();
        }


    }


    protected void Interactions()
    {

        if (!edgCheck)
        {
            //If edge check is fals it would mean that if the enemy continues to walk it will fall
            //There for it will call a function to prevent from falling from plataforms edges
            this.edgeFallPrevention();

        }

        if (obsCheck)
        {
            //if it detects an obstacle created by the map itself then its calls a function to prevent
            //the enemy to walk forever agains a wall
            this.obstaclePrevention();
        }



    }


    private void FixedUpdate()
    {



        if (!isDead)
        {
            edgCheck = InteractionManager.edgeCheck();
            gCheck = InteractionManager.groundChck();
            obsCheck = InteractionManager.obstacleCheck();

            Interactions();


            if (gCheck && !myRb.isKinematic)
            {
                myRb.velocity = Vector3.zero;
                myRb.isKinematic = true;
            }

            //if not dead then do something
            Mouvement(currentAction);

            //wakling animation based on the velocity of its rigidBody
            myAni.SetFloat("walk", Mathf.Abs(myRb.velocity.x));
        }

   


    }



    
    

    public override void Dies()
    {
        //Method called when the enemi is killed. it sets the dead animation, make the enemy collider go trigger, removes gravity so it doesnt fall, set velocity to zero
        myAni.SetTrigger("IsKilled");
        myBxC.isTrigger = true;
        myRb.velocity = Vector3.zero;
        myRb.gravityScale = 0f;
        isDead = true;
    }

    protected override void Mouvement(char Action) {
        
    

        switch(Action)
        {
            case 'W' :
              
                waitTime();

                break;

            case 'L' :
              
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                myRb.velocity = new Vector2(-enemyMouvSpeed, 0);
                getDir();

                break;

            case 'R':

                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                myRb.velocity = new Vector2(enemyMouvSpeed, 0);
                getDir();

                break;

            case 'A':

                attack();

                break;
        }
    }

    protected override void attack()
    {
        //methode qui prend en compte la duration de l'animation de l'attaque pour synchroniser avec l'activation de attack Boxes
        waitTimer += Time.deltaTime;
        if (waitTimer > attackDur - 0.5 && waitTimer < (attackDur - 0.2))
        {
            isAttacking = true;
        }
        else if (waitTimer > (attackDur - 0.2))
        {
            isAttacking = false;
            waitTimer = 0.0f;
            getDir();
        }

    }


    protected virtual void edgeFallPrevention()
    {
        //Methode pour prevenir que le enemi tombe des platformes
        //Elle est appele de que le boxcast n'est plus active, voulant dire que il va tomber si il continue son chemin
        //Ainsi il de que cette fonction est appele l'enemy va tourber et va enregistrer un nouvelle limite pour ca zone a patrouiller
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

    protected virtual void obstaclePrevention()
    {
        //Methode pour prevenir que le enemi reste bloque par le map
        //Elle est appele de que le raycast  est active, voulant dire que il va collisioner avec un obstacle si il continue son chemin
        //Ainsi il de que cette fonction est appele l'enemy va tourber et va enregistrer un nouvelle limite pour ca zone a patrouiller
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


 

}

