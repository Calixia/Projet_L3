using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy3_Jump : MonoBehaviour
{

    private Enemy3 Controller;



    public Vector2 playerJumped, playerLanded = Vector2.zero;

    //Jump - Berzier cuadratic
    [SerializeField] private float jumpSpeed = 0.8f;
    public float t = 0f;

    private void Start()
    {
       Controller = GetComponent<Enemy3>();
        
    }


    public void resgisterJumpLanding()
    {
        if (playerJumped == Vector2.zero || playerLanded == Vector2.zero)
        {

            if (!Controller.playerController.gCheck && playerJumped == Vector2.zero && !Controller.playerController.isHit)
            {//catches if the player jumps  for the first time and saves the position
                //Debug.Log("Start of condition player jumped");

                playerJumped = Controller.thePlayer.transform.position;

                //Debug.Log(playerJumped);

                //Why the fuck FixedUpdate is slower on the prefab

                if (yDistancePlJumpedEnemy() < 0.2f || yDistancePlJumpedEnemy() > 1f)
                {
                    playerJumped = Vector2.zero;
                }



                //Debug.Log("end of condition player jumped");
                //Debug.Log(playerJumped);

            }

            if (Controller.playerController.gCheck && playerJumped != Vector2.zero)
            {//Catches if the player has landed after jumping
                if (Controller.thePlayer.transform.position.y > playerJumped.y)
                {
                    //Debug.Log("Goblin has registered player landed");
                    playerLanded = Controller.thePlayer.transform.position;

                    Controller.currentAction = 'J';
                    Controller.nextAction = 'P';

                }
                else
                {
                    //the player landed in the same height he jumped
                    // so  vectors are reset
                    playerJumped = Vector2.zero;
                    playerLanded = Vector2.zero;
                }

            }
        }
    }

    public float yDistancePlJumpedEnemy()
    {


        float distance;

        distance = Mathf.Abs(Controller.thePlayer.transform.position.y - this.transform.position.y);

        /*
        Debug.Log(" thePlayer.transform.position.y =");
        Debug.Log(Controller.thePlayer.transform.position.y);

        Debug.Log("this.transform.position.y = ");
        Debug.Log(this.transform.position.y);
        */

        Debug.Log(this.gameObject);
        Debug.Log("distance = ");
        Debug.Log(distance);
        

        return distance;


    }



    public void guidedJump()
    {

        Vector3 P0 = playerJumped;
        Vector3 P2 = playerLanded;
        Vector3 P1;

        if (P0.x > P2.x)
        {
            P1 = new Vector3(P2.x + (Vector3.Distance(P0, P2) / 2), P2.y + 5, 0);

        }
        else
        {
            P1 = new Vector3(P2.x - (Vector3.Distance(P0, P2) / 2), P2.y + 5, 0);
        }


        if (t < 1f)
        {
            transform.position = P1 + Mathf.Pow((1 - t), 2) * (P0 - P1) + Mathf.Pow(t, 2) * (P2 - P1);

            t = t + jumpSpeed * Time.deltaTime;

        }
        else
        {
            playerJumped = Vector2.zero;
            playerLanded = Vector2.zero;
            t = 0f;
            Controller.Jump = false;
            Controller.deBugStuck = 0.0f;


            //control
            Controller.currentAction = 'P';
            Controller.nextAction = 'W';
        }
    }


}
