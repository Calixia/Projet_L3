using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1_Ineractions : MonoBehaviour
{

    private Enemy1IA_scrpt Controller;

    private void Start()
    {
        Controller = GetComponent<Enemy1IA_scrpt>();
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
        //create a boxcast in front of the feet of the caracter to detect ground collisions
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

        //Create a Raycast in front the character to detect any obstacle

        RaycastHit2D obstChckRayCast = Physics2D.Raycast(Controller.myBxC.bounds.center + new Vector3(0.4f * RayCastDir, 0f), Vector2.right * RayCastDir, 0.5f, Controller.theGroundMask);

        return obstChckRayCast.collider;
    }

    private void OnDrawGizmos()
    {
        //method to draw in the scene  different raycasts and important things for debug

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

        if (Controller != null )
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
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //if hitted by the player attack lose health
        if (Controller.myBxC.IsTouching(collision.collider) && collision.collider.gameObject.CompareTag("chAtk") && !Controller.isDead)
        {
            Controller.Health--;
  
        }




    }

    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if the player enter trigger box of enemy, start attack
        if (collision.CompareTag("Player") && !Controller.isAttacking && !Controller.isDead)
        {
            Controller.myRb.velocity = Vector3.zero;
            Controller.currentAction = 'A';
            Controller.myAni.SetTrigger("Attack");
            Controller.waitTimer = 0.0f;
        }
    }

}
