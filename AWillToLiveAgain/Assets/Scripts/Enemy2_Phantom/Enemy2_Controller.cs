using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy2_scrpt : MonoBehaviour
{
    // at start
    public GameObject thePlayer;
    private Rigidbody2D myRb;
    public Animator myAni;
    public BoxCollider2D myBxC;
    private CircleCollider2D myCrlC;

    //Controller
    public char currentAction = 'W';
    public char nextAction = 'L';
    public float PatrolSpeed = 0.8f;

    public Vector2 NearDirToPLayer = Vector2.zero;


    //Status
    public int Health = 1;
    public bool isDead = false;

    //Coldowns
    private float timer = 0f;


    // cuadratic berzier 
    private float t = 0f;

    private Vector3 P0;
    private Vector3 P1;
    private Vector3 P2;
    private int YdirSign = 0;



    // Start is called before the first frame update
    void Start()
    {

        Physics2D.IgnoreLayerCollision(8, 8 , true);

       
      
        myRb = GetComponent<Rigidbody2D>();
        myBxC = GetComponent<BoxCollider2D>();
        myCrlC = GetComponent<CircleCollider2D>();
        myAni = GetComponent<Animator>();

        int rand = UnityEngine.Random.Range(0, 2);
        if (rand == 0)
        {
            nextAction = 'L';
        }
        else
        {
            nextAction = 'R';
        }

        setControlPoints(nextAction);

    }



    // Update is called once per frame
    void Update()
    {

        if (thePlayer != null)
        {
            currentAction = 'A';
            nextAction = 'W';
            t = 0f;
        }

        if (currentAction == 'A')
        {
            objectDirection();
        }


        if(Health == 0 && !isDead)
        {
            Dies();
        }


        if (!isDead)
        {
            Mouvement(currentAction);
        }
        else
        {
            toResurrect();
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(P2, 0.25f);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(P1, 0.25f);

    }


    private void FixedUpdate()
    {
        if(currentAction == 'A')
        {
            getDirNearestPlayer();
        }

    }

    private void objectDirection()
    {

        if (myRb.velocity.x > 0)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

    }
    







    private void Mouvement(char Action)
    {

        switch (Action)
        {
            case 'W':
                //Waiting

                waitTime();

                break;

            case 'L':
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);

                //quadratic Berzier exemple

                if (t < 1f)
                {
                    transform.position = P1 + Mathf.Pow((1 - t), 2) * (P0 - P1) + Mathf.Pow(t, 2) * (P2 - P1);

                    t = t + PatrolSpeed * Time.deltaTime;

                }

                getDir();

                break;

            case 'R':
                
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);

                //quadratic Berzier exemple

                if (t < 1f)
                {
                    transform.position = P1 + Mathf.Pow((1 - t), 2) * (P0 - P1) + Mathf.Pow(t, 2) * (P2 - P1);

                    t = t + PatrolSpeed * Time.deltaTime;

                }


                getDir();

                break;


            case 'A':
                //Attacking
                timer = 0;
                attack();

                break;
        }
    }



    private void setControlPoints(char NextAction)
    {
        int rand = UnityEngine.Random.Range(0, 2);
        if (rand == 0)
        {
            YdirSign = 1;
        }
        else
        {
            YdirSign = -1;
        }

        float Ydir = UnityEngine.Random.Range(0f,12f);
        Ydir = 1 + Ydir / 10;

        P0 = transform.position;

        if (NextAction == 'L')
        {
            P2 = new Vector3(transform.position.x - 5, transform.position.y, 0);
            P1 = new Vector3(P2.x + (Vector3.Distance(transform.position, P2) / 2), transform.position.y + Ydir * YdirSign, 0);

        }
        else
        {
            P2 = new Vector3(transform.position.x + 5, transform.position.y, 0);
            P1 = new Vector3(P2.x - (Vector3.Distance(transform.position, P2) / 2), transform.position.y + Ydir * YdirSign, 0);
        }

    }

    public void getDir()
    {

        if (Vector2.Distance(transform.position, P2) < 0.1f)
        {

            t = 0;
            if (currentAction == 'L')
            {

                currentAction = 'W';
                nextAction = 'R';
                setControlPoints(nextAction);
            }
            else
            {
                currentAction = 'W';
                nextAction = 'L';
                setControlPoints(nextAction);
            }


        }


        if (currentAction == 'A')
        {
            currentAction = 'W';

            if (transform.rotation.y == 0)
            {
                nextAction = 'L';
            }
            else
            {
                nextAction = 'R';
            }
        }


    }



    private void toResurrect() {

        timer += Time.deltaTime;

        if(timer > 3f)
        {

            myAni.SetTrigger("Appear");
            Health++;
            myBxC.isTrigger = false;
            isDead = false;
            timer = 0f;
        }
    
    }
    private void attack()
    {
        if (myRb.velocity.x < 8f && myRb.velocity.x > -8f)
        {
            myRb.AddForce(NearDirToPLayer * 4f);
        }
        else
        {

            myRb.velocity = new Vector2(8f * NearDirToPLayer.x, myRb.velocity.y );
        }



        if (myRb.velocity.y < 8f && myRb.velocity.y > -8f)
        {
            myRb.AddForce(NearDirToPLayer * 4f);
        }
        else
        {

            myRb.velocity = new Vector2(myRb.velocity.x, 8f * NearDirToPLayer.y) ;
        }


    }


    private void getDirNearestPlayer()
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

    public void Dies()
    {
        Debug.Log("phantom is killed temporally");

        myAni.SetTrigger("IsKilled");
        myBxC.isTrigger = true;
        myRb.velocity = Vector3.zero;
        isDead = true;
    }

    private void waitTime()
    {
        
        if(myRb.velocity.x < 0.5f && myRb.velocity.y < 0.5f && myRb.velocity.x > -0.5 && myRb.velocity.y > -0.5f)
        {
            //stop
            myRb.velocity = Vector2.zero;

        }
        else
        {
            //desacceleration
            myRb.AddForce(new Vector2(-myRb.velocity.x / 1.5f, -myRb.velocity.y / 1.5f));


        }


        timer += Time.deltaTime;
        if (timer > 1.2f)
        {
            myRb.velocity = Vector2.zero;
            setControlPoints(nextAction);
            currentAction = nextAction;
            nextAction = 'W';
            timer = 0.0f;

        }
    }

 




}
