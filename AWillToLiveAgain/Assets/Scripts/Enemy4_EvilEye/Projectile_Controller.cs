using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class Projectile_Controller : MonoBehaviour
{
    //Variables to start 

    public Rigidbody2D myRb;
    public Animator myAni;
    public CircleCollider2D myCC;
    private AnimationClip[] Clips;
    private GameObject thePlayer;

    //Parametrage
    public Vector2 NearDirToPLayer = Vector2.zero;

    private bool goStraight = false;
    private bool isDead = false;


    //timers

    private float explosionDur = 0.0f;

    private float timer = 0.0f;
    private float timeToDie = 0.0f;

    // Start is called before the first frame update



    private void Awake()
    {
        thePlayer = GameObject.FindGameObjectWithTag("Player");
        
    }
    void Start()
    {
        myRb = GetComponent<Rigidbody2D>();
        myCC = GetComponent<CircleCollider2D>();
        myAni = GetComponent<Animator>();
        getExplosionDur();
        getDirNearestPlayer();

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


 
    private void getExplosionDur()
    {
        //Fonction to find the Explosion animation and calculate duration
        //We take all the clips in the game
        Clips = myAni.runtimeAnimatorController.animationClips;

        foreach (AnimationClip clip in Clips)
        {

            if (clip.name == "Explode")
            {
                //We find the animation clip and stock the duration that the explosion must have to match the animation
                explosionDur = clip.length;

                break;

            }
        }

    }


    // Update is called once per frame
    void Update()
    {
        objectDirection();

        timer += Time.deltaTime;
        if (timer > 4.0f) 
        {
            isDead = true;
            myRb.velocity = Vector2.zero;
            myAni.SetTrigger("dead");

        }


        if (isDead)
        {

            timeToDie += Time.deltaTime;

            if (timeToDie > explosionDur)
            {
                Destroy(this.gameObject);
            }



        }



    }

    private void FixedUpdate()
    {

        if(!isDead)
        {
            attack();
        }
        
    }

    private void attack()
    {

        //a first impulse, track player until certain distance, then go that direction , after 5 sec destroy.

        if (Vector2.Distance(thePlayer.transform.position, transform.position) > 2f && !goStraight)
        {
            getDirNearestPlayer();


            myRb.velocity = new Vector2(8f * NearDirToPLayer.x, 8f * NearDirToPLayer.y);


        }
        else
        {
            myRb.velocity = new Vector2(8f * NearDirToPLayer.x, 8f * NearDirToPLayer.y);
            goStraight = true;
        }


    }

    private void getDirNearestPlayer()
    {
        /*calculate the nearest direction to the player,
        * 1. take current enemy pos
        * 2. add a unit vector from the possible direction to the enemy pos 
        * 3. compare the distance from this new vector to the player to the distance from the current "nearest" unit vector to the player
        */

        //Vector2[] possibleDir = { new Vector2(-1, -1), new Vector2(-1, 0), new Vector2(-1, 1), new Vector2(0, -1), new Vector2(0, 1), new Vector2(1, -1), new Vector2(1, 0), new Vector2(1, 1) };

        Vector2 posToTest, currentNpos;


        for(float i = 0; i< (Mathf.PI*2); i = i +0.1f)
        {
            posToTest = new Vector2(transform.position.x + Mathf.Cos(i), transform.position.y + Mathf.Sin(i));
            currentNpos = new Vector2(transform.position.x + NearDirToPLayer.x, transform.position.y + NearDirToPLayer.y);


            if (Vector2.Distance(posToTest, thePlayer.transform.position) < Vector2.Distance(currentNpos, thePlayer.transform.position))
            {
                NearDirToPLayer = new Vector2 (Mathf.Cos(i), Mathf.Sin(i));
            }
        }

    }



    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" && !isDead){
            myRb.velocity = Vector2.zero;
            isDead = true;
            myAni.SetTrigger("dead");
            myCC.isTrigger = true;
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(new Vector2(transform.position.x + NearDirToPLayer.x, transform.position.y + NearDirToPLayer.y),0.2f);
    }


}
