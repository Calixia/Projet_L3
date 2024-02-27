using System;
using System.Diagnostics;
using System.Globalization;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Player_scrpt : MonoBehaviour
{
    //to init
    private Rigidbody2D myRb;
    private Animator myAni;
    public BoxCollider2D myBxC;
    private LayerMask theGroundMask;

    //parametrage
    public float speed = 2.0f;
    public float airSpeed = 1.0f;
    public float gravityMultp = 5.0f;
    private float fallGravity;

    private AnimationClip[] Clips;
    private float attackDur;

    private int nbjumps = 2;

        //direction to push back
    private int pushBackDir = 0;

    //mouvement conditions
    private float H_Mouvement = 0.0f;
    private bool jump;
    private bool gCheck = false;

    //actions

    private bool isHit = false;
    public bool isAttacking =false;

    //Coldowns
    private float timer1 = 0.0f;

    //actions




    // Start is called before the first frame update
    void Start()
    {
        myRb = GetComponent<Rigidbody2D>();
        myAni = GetComponent<Animator>();
        //myBxC = GetComponent<BoxCollider2D>();
        theGroundMask = LayerMask.GetMask("Ground");
        fallGravity = gravityMultp + 3;
        getAttackDur();
        
    }

    // Update is called once per frame
    void Update()
    {

        /*if (myRb.velocity.x < -0.0001f)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }*/

        animationManager();


        H_Mouvement = Input.GetAxisRaw("Horizontal");


        if (Input.GetKeyDown(KeyCode.UpArrow)){ 
            jump = true;
        }

        if (Input.GetKeyDown(KeyCode.Z) && !isAttacking && !isHit)
        {
            if (!myAni.GetBool("test"))
            {
                myAni.SetBool("test", true);
            }


            myAni.SetTrigger("Attack");
            isAttacking = true;
            myRb.velocity = new Vector2 (0, myRb.velocity.y);
            countdowns("atkCldwn");
        }
        else if (isAttacking) 
        {
            countdowns("atkCldwn");
        }




        FallSpeed();

        yVelocityTreshold();

    }

    private void getAttackDur()
    {
        Clips = myAni.runtimeAnimatorController.animationClips;

        foreach(AnimationClip clip in Clips)
        {
            if(clip.name == "Attack!")
            {   
                attackDur = clip.length;
                //Debug.Log("attack duration:");
                //Debug.Log(attackDur);
                break;
            }
        }

    }

    private void animationManager()
    {

        if (!isHit && !isAttacking)
        {
            myAni.SetBool("test", gCheck);

        }
        else
        {
            myAni.SetBool("test", true);
        }


        myAni.SetBool("Hurt", isHit);


        myAni.SetFloat("Run", Mathf.Abs(myRb.velocity.x));



    }

    private void yVelocityTreshold()
    {
        if(myRb.velocity.y > 40)
        {

            myRb.velocity = new Vector2(myRb.velocity.x, 0);
        }
    }

    private void FallSpeed()
    {
        if (!gCheck && myRb.velocity.y < 0 && myRb.gravityScale != fallGravity)
        {
            Debug.Log("Gravity changed, objt heavier");
            myRb.gravityScale = fallGravity;
        }
    }

    private void FixedUpdate()
    {

        gCheck = groundChck();


        if (gCheck)
        {
            myRb.gravityScale = 1;

            nbjumps = 2;

        }


        if (!isHit) { 

            mouvChoice();
        }
        else
        {
            Hitted(pushBackDir);
        }

 

    }

    private void mouvChoice()
    {

        if (H_Mouvement > 0.1)
        {
            goRight();

        }

        if (H_Mouvement < -0.1)
        {
            goLeft();

        }

        if (H_Mouvement < 0.1 && H_Mouvement > -0.1)
        {

            stopM();

        }

        if (jump)
        {

            gojump();

            jump = false;
        }

    }

    private void countdowns(String Choice)
    {

        switch (Choice)
        {
            case "hitCldwn":

                timer1 += Time.deltaTime;

                if (timer1 > 0.5f)
                {
                    //Physics2D.IgnoreLayerCollision(3, 8, false);

                    myBxC.excludeLayers = LayerMask.GetMask("Nothing");

                    isHit = false;
                    timer1 = 0.0f;
                    myRb.velocity = Vector2.zero;
                    Debug.Log("Hit cooldown ends");

                    
                }

                break;

            case "atkCldwn":

                timer1 += Time.deltaTime;


                if (timer1 > attackDur - 0.04f)
                {
                    isAttacking = false;
                    timer1 = 0.0f;

                    Debug.Log("Attack cooldown ends");

                }

                break;
        }

        

    }


    private void Hitted(int x_sign)
    {
        Vector2 pushedBack;


        if (timer1 == 0)
        {
            //Debug.Log("inside hitted condition timer 1 = 0");


            myRb.velocity = Vector3.zero;

            countdowns("hitCldwn");
        }
        else
        {
            pushedBack = new Vector2(10 * x_sign, 5);

            if(myRb.velocity.x > -10f && myRb.velocity.x < 10f && myRb.velocity.y < 5 )
            {
                myRb.AddForce(pushedBack, ForceMode2D.Impulse);

            }
            else
            {
                myRb.AddForce(-pushedBack / 2, ForceMode2D.Impulse);
            }



            //Debug.Log("inside hitted condition else");
            countdowns("hitCldwn");
        }

    
    }

    private void stopM()
    {
       

        if (gCheck) {
            //myRb.velocity = new Vector2(0, myRb.velocity.y);
            myRb.velocity = new Vector2(0, 0);
        }
    }



    private void goRight()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        if (/*Grouded */ gCheck)
        {
            if (myRb.velocity.x < 10f)
            {
                myRb.AddForce(Vector2.right * speed);
            }
            else
            {
                Vector2 vec_test = new Vector2(10f, myRb.velocity.y);
                myRb.velocity = vec_test;
            }

        }
        else{
            //on air
            // myRb.AddForce(Vector2.right * this.airSpeed);

            if (myRb.velocity.x < 12f && myRb.velocity.x >= 0)
            {
                myRb.AddForce(Vector2.right * (this.airSpeed));
            }
            else
            {
                myRb.velocity = new Vector2(+12f, myRb.velocity.y);
            }


        }


    }

    private void goLeft()
    {
        transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        if (/*Grouded */ gCheck) {

            if (myRb.velocity.x > (-10f))
            {
                myRb.AddForce(Vector2.right * (-speed));
            }
            else
            {
                Vector2 vec_test = new Vector2(-10f, myRb.velocity.y);
                myRb.velocity = vec_test;
            }
        }
        else
        {
            /*if(myRb.velocity.x > 0)
            {
                myRb.velocity = new Vector2(-myRb.velocity.x, myRb.velocity.y); 
            }*/

            if(myRb.velocity.x > -12f && myRb.velocity.x <= 0)
            {
                myRb.AddForce(Vector2.right * (-this.airSpeed));
            }
            else
            {
                myRb.velocity = new Vector2(-12f, myRb.velocity.y);
            }

        }

    }

    private void gojump()
    {
        //myAni.SetBool("Jump", true);
        if (/*Grouded*/ gCheck || nbjumps !=0)
        {

            //myAni.SetTrigger("Jumping");

            //myAni.SetBool("test", true);

            Debug.Log("he jumped!");
            
            myRb.gravityScale = 5;




            myRb.velocity = new Vector2(myRb.velocity.x, 0);

            myRb.AddForce(Vector2.up * 20, ForceMode2D.Impulse);
           
            nbjumps--;
            
        }
    }


    private bool groundChck()
    {

        

        RaycastHit2D GrounfChckBoxCast = Physics2D.BoxCast(myBxC.bounds.center - new Vector3(0, 0.6f), myBxC.bounds.size - new Vector3(0, 1f), 0f, Vector2.down, 0f, theGroundMask);

      

        return GrounfChckBoxCast.collider;
    }

   

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(myBxC.bounds.center - new Vector3(0, 0.6f), myBxC.bounds.size - new Vector3(0f, 1f));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        if (myBxC.IsTouching(collision.collider) && collision.gameObject.CompareTag("Enm1") && !isHit)
        {

            isHit = true;

            //Physics2D.IgnoreLayerCollision(3, 8, true);

            myBxC.excludeLayers = LayerMask.GetMask("Enemy");

            pushBackDir = choseDir(collision.contacts[0].point.x);

            Debug.Log(collision.contacts[0].point);
        }



    }

    private int choseDir(float x_contactPt)
    {
        if (x_contactPt - transform.position.x < 0)
        {
            return  1;
        }
        else if (x_contactPt - transform.position.x > 0)
        {
            return -1;
        }
        else
        {
            return 0;
        }

    }

   










}
