using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy3_Interactions : MonoBehaviour
{
    private Enemy3 Controller;



    // Start is called before the first frame update
    void Start()
    {
        Controller = GetComponent<Enemy3>();
        
    }

    public bool groundChck()
    {
        RaycastHit2D GrounfChckBoxCast = Physics2D.BoxCast(Controller.myBxC.bounds.center - new Vector3(0, 0.55f), Controller.myBxC.bounds.size - new Vector3(0f, 1f), 0f, Vector2.down, 0.09f, Controller.theGroundMask);


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

        RaycastHit2D edgeChckBoxCast = Physics2D.BoxCast(Controller.myBxC.bounds.center - new Vector3(0.8f * BoxCastDir, 0.6f), Controller.myBxC.bounds.size - new Vector3(0f, 0.9f), 0f, Vector2.down, 0f, Controller.theGroundMask);


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

        RaycastHit2D obstChckRayCast = Physics2D.Raycast(Controller.myBxC.bounds.center + new Vector3(0.4f * RayCastDir, 0f), Vector2.right * RayCastDir, 0.5f, Controller.theGroundMask);

        return obstChckRayCast.collider;
    }


    public bool PlayerOnSight()
    {

        RaycastHit2D SightRayCast = Physics2D.Raycast(Controller.myBxC.bounds.center, Controller.thePlayer.transform.position - Controller.transform.position , Vector2.Distance(Controller.transform.position, Controller.thePlayer.transform.position), Controller.theGroundMask);

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
            Gizmos.DrawWireCube(Controller.myBxC.bounds.center - new Vector3(0.75f * BoxCastDir, 0.6f), Controller.myBxC.bounds.size - new Vector3(0f, 0.9f));

            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(Controller.myBxC.bounds.center - new Vector3(0, 0.55f), Controller.myBxC.bounds.size - new Vector3(0f, 1f));


            Gizmos.color = Color.red;
            Gizmos.DrawLine(Controller.myBxC.bounds.center + new Vector3(0.4f * RayCastDir, 0f), Controller.myBxC.bounds.center + new Vector3(0.9f * RayCastDir, 0f));

            if (Controller.thePlayer != null)
            {

                Gizmos.color = Color.black;
                Gizmos.DrawRay(Controller.myBxC.bounds.center, Controller.thePlayer.transform.position - Controller.transform.position);
            }


            


            
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (Controller.myBxC.IsTouching(collision.collider) && collision.gameObject.CompareTag("Player"))
        {
            if (Controller.Health > 0)
            {
                Controller.myRb.isKinematic = true;
            }

            Controller.myRb.velocity = Vector3.zero;

        }

        if (Controller.myBxC.IsTouching(collision.collider) && collision.collider.gameObject.CompareTag("chAtk") && !Controller.isHit && !Controller.isDead)
        {
            Controller.Health--;

            if (Controller.Health < 0)
            {
                Controller.Health = 0;
            }

            if (Controller.Health > 0) { 
                Controller.Hitted();
            }
            int pushBackDir = choseDir(collision.contacts[0].point.x);
            Controller.setStaggerControlPoints(pushBackDir);
        }


    }


    private int choseDir(float x_contactPt)
    {
        if (x_contactPt - transform.position.x < 0)
        {
            return 1;
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


    private void OnCollisionExit2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Player"))
        {
            Controller.myRb.isKinematic = false;

        }

    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !Controller.isDead)
        {
            Controller.thePlayer = collision.gameObject;
            Controller.playerController = collision.gameObject.GetComponent<Player_scrpt>();
            Controller.myBxTrg.enabled = false;
            Controller.isPursuing = true;
        }
    }





}
