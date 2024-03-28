using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class Enemy : MonoBehaviour
{
    //Variables to Start
        //Every common variable that enemies will fill at their spawn
    public Rigidbody2D myRb; //The rigidBody of the enemy
    public Animator myAni; // The Animator of the enemy
    public LayerMask theGroundMask; // Variable that stocks the ground layer define on unity for enemi's ground check
    protected  AnimationClip[] Clips; // Animation clips

    //Horizontal mouvement Limits
    public Vector2 limitL, limitR; //Vector that defines the limits of the zone to patrol 

    // Variables to configure
    public char currentAction ;
    public char nextAction ;
    public int Health ;
    public Vector2 NearDirToPLayer; // Neares direction Vector from the enemy to the player
    public GameObject thePlayer; // The player Game object

    //Ground Check
    public bool gCheck; // Variable for check if the Enemy is on the ground 

    //Status
    public bool isAttacking;
    public bool isDead;
    public bool isHit;


    //Timers-Coldowns
    public float waitTimer; // Timer  for the enemies 'w' actions
    protected float timerToDestroy;
    protected float attackDur; // Attack Duration
    protected float attackColdown; 


    protected virtual void waitTime()
    {
        
        waitTimer += Time.deltaTime;
        if (waitTimer > 1.5f)
        {
            //Wait for 1.5 seconds then change action
            currentAction = nextAction;
            nextAction = 'W';
            waitTimer = 0.0f;

        }
    }

    //Methodes a implimenter par les enemies
    public abstract void Dies();
    protected abstract void Mouvement(char Action);


    protected abstract void attack();

    public virtual void getDir()
    {

        //IF the enemy has reached the left limit of its patrol zone , change action to wait and then to go right
        if (Vector2.Distance(transform.position, limitL) < 0.5f && currentAction == 'L')
        {
            myRb.velocity = Vector2.zero;
            currentAction = 'W';
            nextAction = 'R';

        }//If the enemy has reached the right limit of its patrol zone , change action to wait and then to go right
        else if (Vector2.Distance(transform.position, limitR) < 0.5f && currentAction == 'R')
        {
            myRb.velocity = Vector2.zero;
            currentAction = 'W';
            nextAction = 'L';
        }

        //if this method is called and the enemy is attacking , then end the attact and wait
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
    protected virtual void gonnaDestroy()
    {
        //after 4 seconds of the enemi being dead, destroy the game oobject
        timerToDestroy += Time.deltaTime;

        if (timerToDestroy > 4)
        {
            Destroy(this.gameObject);
        }
    }

    protected virtual void getDirNearestPlayer()
    {
        /*calculate the nearest direction to the player,
         * 1. take current enemy pos
         * 2. add a unit vector from the possible direction to the enemy pos 
         * 3. compare the distance from this new vector to the player to the distance from the current "nearest" unit vector to the player
         */



        Vector2 posToTest, currentNpos;


        for (float i = 0; i < (Mathf.PI * 2); i = i + 0.1f)
        {
            posToTest = new Vector2(transform.position.x + Mathf.Cos(i), transform.position.y + Mathf.Sin(i));
            currentNpos = new Vector2(transform.position.x + NearDirToPLayer.x, transform.position.y + NearDirToPLayer.y);


            if (Vector2.Distance(posToTest, thePlayer.transform.position) < Vector2.Distance(currentNpos, thePlayer.transform.position))
            {
                NearDirToPLayer = new Vector2(Mathf.Cos(i), Mathf.Sin(i));
            }
        }


    }

    protected virtual void objectDirection()
    {
        //if the velocity is less than 0 it means the enemy is going left, there for we flip the game object so every component is going the needed direction
        //We do this by rotating the game object with a inner unity method. Quaternions are vectors that represent rotation.
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
            if (thePlayer.transform.position.x < this.transform.position.x)
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
   
