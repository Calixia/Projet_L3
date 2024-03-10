using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy3 : MonoBehaviour
{
    // at start
    public GameObject thePlayer;
    private Rigidbody2D myRb;
    private Animator myAni;
    public BoxCollider2D myBxC;
    private LayerMask theGroundMask;
    private Vector2 limitL, limitR;


    //Status
    public char currentAction = 'W';
    public char nextAction = 'L';
    private int Health = 2;

    private bool edgCheck = false;
    private bool obsCheck = false;
    private bool gCheck = false;
    private bool isDead = false;


    //parametrage

    private float timer = 0.0f;

    private bool dash = false;

    // Start is called before the first frame update
    void Start()
    {

        limitL = new Vector2(transform.position.x - 4, transform.position.y);
        limitR = new Vector2(transform.position.x + 4, transform.position.y);

        //Physics2D.IgnoreLayerCollision(8, 8, true);

        myRb = GetComponent<Rigidbody2D>();
        myBxC = GetComponent<BoxCollider2D>();

        myAni = GetComponent<Animator>();

        theGroundMask = LayerMask.GetMask("Ground");
    }

    // Update is called once per frame
    void Update()
    {
        //objectDirection();


        limitL = new Vector2(limitL.x, transform.position.y);
        limitR = new Vector2(limitR.x, transform.position.y);



        edgCheck = edgeCheck();
        gCheck = groundChck();
        obsCheck = obstacleCheck();

        dash = Input.GetKeyDown(KeyCode.T);


        if (!edgCheck)
        {
            //check current dir
            edgeFallPrevention();

        }

        if (obsCheck)
        {
            obstaclePrevention();
        }


        if (!isDead)
        {
            myAni.SetFloat("Running", Mathf.Abs(myRb.velocity.x));
            Mouvement(currentAction);
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

    private bool groundChck()
    {
        RaycastHit2D GrounfChckBoxCast = Physics2D.BoxCast(myBxC.bounds.center - new Vector3(0, 0.55f), myBxC.bounds.size - new Vector3(0f, 1f), 0f, Vector2.down, 0f, theGroundMask);


        return GrounfChckBoxCast.collider;
    }

    private bool edgeCheck()
    {
        float BoxCastDir = 0;

        if (transform.rotation.y == 0)
        {
            BoxCastDir = -1;
        }
        else
        {
            BoxCastDir = 1;
        }

        RaycastHit2D edgeChckBoxCast = Physics2D.BoxCast(myBxC.bounds.center - new Vector3(0.8f * BoxCastDir, 0.6f), myBxC.bounds.size - new Vector3(0f, 0.9f), 0f, Vector2.down, 0f, theGroundMask);


        return edgeChckBoxCast.collider;


    }

    private bool obstacleCheck()
    {
        float RayCastDir = 0;

        if (transform.rotation.y == 0)
        {
            RayCastDir = 1;
        }
        else
        {
            RayCastDir = -1;
        }

        RaycastHit2D obstChckRayCast = Physics2D.Raycast(myBxC.bounds.center + new Vector3(0.4f * RayCastDir, 0f), Vector2.right * RayCastDir, 0.5f, theGroundMask);

        return obstChckRayCast.collider;
    }



    private void objectDirection()
    {

        if (myRb.velocity.x < 0)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else
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
                //attack();

                break;
        }
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
