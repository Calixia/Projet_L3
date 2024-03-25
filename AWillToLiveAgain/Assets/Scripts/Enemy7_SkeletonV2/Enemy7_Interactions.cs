using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy7_Interactions : MonoBehaviour
{
    private Enemy7_Controller Controller;
    private Vector3 playerPosCatch;
    private Vector2 NearDirToPLayer = Vector2.zero;

    private void Start()
    {
        Controller = GetComponent<Enemy7_Controller>();
    }


    public void toAttack()
    {

        if (Mathf.Abs(playerPosCatch.x - this.transform.position.x) > 1f)
        {
            Debug.Log("going towards player");
            objectDirection();
            getDirNearestPlayer();
            Controller.myRb.velocity = new Vector2(12f * NearDirToPLayer.x, Controller.myRb.velocity.y);
        }else
        {
            Controller.myRb.velocity = Vector2.zero;
            Controller.myAni.SetTrigger("Attack");
            Controller.currentAction = 'A';
        }
       
    }

    public void attack()
    {

        Controller.attackTimer += Time.deltaTime;
        if (Controller.attackTimer > Controller.attackDur - 0.5 && Controller.attackTimer < (Controller.attackDur - 0.2))
        {
            Controller.isAttacking = true;
        }
        else if (Controller.attackTimer > (Controller.attackDur - 0.2))
        {
            Controller.isAttacking = false;
            Controller.attackTimer = 0.0f;
            playerPosCatch = Vector2.zero;
            Controller.myBxTrg.enabled = true;
            Controller.getDir();

        }

    }

    public bool groundChck()
    {
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

        RaycastHit2D obstChckRayCast = Physics2D.Raycast(Controller.myBxC.bounds.center + new Vector3(0.4f * RayCastDir, 0f), Vector2.right * RayCastDir, 0.5f, Controller.theGroundMask);

        return obstChckRayCast.collider;
    }

    public bool PlayerOnSight()
    {

        RaycastHit2D SightRayCast = Physics2D.Raycast(Controller.myBxC.bounds.center, playerPosCatch - Controller.transform.position, Vector2.Distance(Controller.transform.position, playerPosCatch), Controller.theGroundMask);

        return SightRayCast.collider;

    }

    private void getDirNearestPlayer()
    {


        if (playerPosCatch.x < this.transform.position.x)
        {
            NearDirToPLayer = new Vector2(-1, 0);

        }
        else
        {
            NearDirToPLayer = new Vector2(1, 0);

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
        }

        if(playerPosCatch != Vector3.zero)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(playerPosCatch, 0.5f);
        }
    }




    private void objectDirection()
    {

        if (Controller.myRb.velocity.x < 0f)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }

        if (Controller.myRb.velocity.x > 0f)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }


        if (Controller.myRb.velocity.x == 0f)
        {
            if (playerPosCatch.x < this.transform.position.x)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }





    }

    private void OnCollisionEnter2D(Collision2D collision)
    {



        if (Controller.myBxC.IsTouching(collision.collider) && collision.collider.gameObject.CompareTag("chAtk") && !Controller.isDead && !Controller.isHit)
        {
            Controller.Health--;

            if (Controller.Health <0)
            {
                Controller.Health = 0;
            }

            if (Controller.Health > 0)
            {
                Controller.Hitted();
            }


        }




    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !Controller.isAttacking && !Controller.isDead)
        {
            playerPosCatch = collision.gameObject.transform.position;
            if (!PlayerOnSight())
            {

                Controller.myRb.velocity = Vector3.zero;
                Controller.currentAction = 'T';
                Controller.waitTimer = 0.0f;
                Controller.myBxTrg.enabled = false;
            }
            else
            {
                playerPosCatch = Vector3.zero;
            }
        }
    }
}
