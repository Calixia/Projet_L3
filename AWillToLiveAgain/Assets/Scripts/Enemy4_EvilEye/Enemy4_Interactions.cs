using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy4_Interactions : MonoBehaviour
{
    private Enemy4_Controller Controller;

    // Start is called before the first frame update
    void Start()
    {

        Controller = GetComponent<Enemy4_Controller>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool groundChck()
    {
        RaycastHit2D GrounfChckBoxCast = Physics2D.BoxCast(Controller.myCC.bounds.center - new Vector3(0, 0.3f), Controller.myCC.bounds.size - new Vector3(0, 0.5f), 0.1f, Vector2.down, 0f, Controller.theGroundMask);
        

        return GrounfChckBoxCast.collider;
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !Controller.isDead)
        {
            Controller.thePlayer = collision.gameObject;
            Controller.myBxC.enabled = false;
        }
        

    }



    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.gameObject.CompareTag("chAtk") && !Controller.isDead){
            Controller.Health--;
        }
    }


    private void OnDrawGizmos()
    {

        

        if (Controller != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Controller.limitL, new Vector2(0.5f, 0.5f));
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Controller.limitR, new Vector2(0.5f, 0.5f));

            //Gizmos.color = Color.yellow;
            //Gizmos.DrawWireCube(Controller.myCC.bounds.center - new Vector3(0.5f * BoxCastDir, 0.6f), Controller.myBxC.bounds.size - new Vector3(0f, 1f));


            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(Controller.myCC.bounds.center - new Vector3(0, 0.3f), Controller.myCC.bounds.size - new Vector3(0, 0.5f));

            //Gizmos.color = Color.red;
            //Gizmos.DrawLine(Controller.myBxC.bounds.center + new Vector3(0.4f * RayCastDir, 0f), Controller.myBxC.bounds.center + new Vector3(0.9f * RayCastDir, 0f));
        }
    }


}
