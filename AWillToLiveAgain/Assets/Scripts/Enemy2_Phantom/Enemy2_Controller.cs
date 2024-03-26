using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy2_scrpt : Enemy
{
    // at start
    public BoxCollider2D myBxC; //Box Collider
    private CircleCollider2D myCrlC; //Circle collider trigger/ sonar

    //To configure
    public float PatrolSpeed = 0.8f;

    //Coldowns
    private float timer = 0f;

    // cuadratic berzier  for parabolic mouvement
    private float t = 0f;

    private Vector3 P0;
    private Vector3 P1;
    private Vector3 P2;
    private int YdirSign = 0;



    // Start is called before the first frame update
    void Start()
    {

        //Ignore other enemies collisions
        Physics2D.IgnoreLayerCollision(8, 8);

        
        myRb = GetComponent<Rigidbody2D>();
        myBxC = GetComponent<BoxCollider2D>();
        myCrlC = GetComponent<CircleCollider2D>();
        myAni = GetComponent<Animator>();

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

        //set Control points  of berzier curve
        setControlPoints(nextAction);

    }



    // Update is called once per frame
    void Update()
    {

        if (thePlayer != null)
        {
            //if the player is located then switch to attaack action
            currentAction = 'A';
            nextAction = 'W';
            t = 0f;
        }

        if (currentAction == 'A')
        {//IF the enemy is attacking it sonar zone gets bigger
            objectDirection();
            myCrlC.radius = 2.4f;
        }
        else
        {
            myCrlC.radius = 2f;
        }


        if(Health == 0 && !isDead)
        {
            Dies();
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

         if (!isDead)
        {
            Mouvement(currentAction);
        }
        else
        {
            toResurrect();
        }
    }

    protected override void objectDirection()
    {
        //Methode to manage GHameObject rotation depending whre it is going

        if (myRb.velocity.x > 0)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

    }



    protected override void Mouvement(char Action)
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

                //quadratic Berzier

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

    public override void getDir()
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
        //Method called when dead, to ressurect the emnemy after 3 seconds

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
    protected override void attack()
    {
        //Go towards the player to hurt him.
        getDirNearestPlayer();

        myRb.velocity = new Vector2(8f * NearDirToPLayer.x, 8f * NearDirToPLayer.y);
    }



    public override void Dies()
    {
        //When it dies animation is set, its collider becomes trigger and its velocity is 0
        myAni.SetTrigger("IsKilled");
        myBxC.isTrigger = true;
        myRb.velocity = Vector3.zero;
        isDead = true;
    }

    protected  override void waitTime()
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
