using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy7_Interactions : MonoBehaviour
{
    private Enemy7_Controller Controller;

    private void Start()
    {
        Controller = GetComponent<Enemy7_Controller>();
    }



    public bool groundChck()
    {
        //create a boxcast at the feet of the character  to detect ground collisions
        RaycastHit2D GrounfChckBoxCast = Physics2D.BoxCast(Controller.myBxC.bounds.center - new Vector3(0, 0.526f), Controller.myBxC.bounds.size - new Vector3(0, 1f), 0f, Vector2.down, 0f, Controller.theGroundMask);


        return GrounfChckBoxCast.collider;
    }

    public bool edgeCheck()
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

        RaycastHit2D edgeChckBoxCast = Physics2D.BoxCast(Controller.myBxC.bounds.center - new Vector3(0.5f * BoxCastDir, 0.6f), Controller.myBxC.bounds.size - new Vector3(0, 1f), 0f, Vector2.down, 0f, Controller.theGroundMask);


        return edgeChckBoxCast.collider;


    }

    public bool obstacleCheck()
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

        //create a boxcast in front of the feet of the caracter to detect ground collisions

        RaycastHit2D obstChckRayCast = Physics2D.Raycast(Controller.myBxC.bounds.center + new Vector3(0.4f * RayCastDir, 0f), Vector2.right * RayCastDir, 0.5f, Controller.theGroundMask);

        return obstChckRayCast.collider;
    }

    public bool PlayerOnSight()
    {
        //Create a Raycast in front the character to detect any obstacle

        RaycastHit2D SightRayCast = Physics2D.Raycast(Controller.myBxC.bounds.center, Controller.playerPosCatch - Controller.transform.position, Vector2.Distance(Controller.transform.position, Controller.playerPosCatch), Controller.theGroundMask);

        return SightRayCast.collider;

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

        if (Controller != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Controller.limitL, new Vector2(0.5f, 0.5f));
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Controller.limitR, new Vector2(0.5f, 0.5f));

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Controller.myBxC.bounds.center - new Vector3(0.5f * BoxCastDir, 0.6f), Controller.myBxC.bounds.size - new Vector3(0f, 1f));


            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(Controller.myBxC.bounds.center - new Vector3(0, 0.526f), Controller.myBxC.bounds.size - new Vector3(0, 1f));


            Gizmos.color = Color.red;
            Gizmos.DrawLine(Controller.myBxC.bounds.center + new Vector3(0.4f * RayCastDir, 0f), Controller.myBxC.bounds.center + new Vector3(0.9f * RayCastDir, 0f));

            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(Controller.playerPosCatch, 0.5f);
        }

    }




 

    private void OnCollisionEnter2D(Collision2D collision)
    {


        //if hitted by the player attack lose health
        if (Controller.myBxC.IsTouching(collision.collider) && collision.collider.gameObject.CompareTag("chAtk") && !Controller.isDead && !Controller.isHit)
        {
            Controller.Health--;

            if (Controller.Health <0)
            {
                //Bug prevention
                Controller.Health = 0;
            }

            if (Controller.Health > 0)
            {//if not dead , then call hiited method
                Controller.Hitted();
            }


        }




    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if the player enter trigger box of enemy, start attack action
        if (collision.CompareTag("Player") && !Controller.isAttacking && !Controller.isDead)
        {
            Controller.playerPosCatch = collision.gameObject.transform.position;
            if (!PlayerOnSight())
            {

                Controller.myRb.velocity = Vector3.zero;
                Controller.currentAction = 'T';
                Controller.waitTimer = 0.0f;
                Controller.myBxTrg.enabled = false;
            }
            else
            {
                Controller.playerPosCatch = Vector3.zero;
            }
        }
    }
}
