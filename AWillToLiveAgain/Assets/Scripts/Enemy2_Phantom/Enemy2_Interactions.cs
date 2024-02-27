using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy2_Interactions : MonoBehaviour
{
    private Enemy2_scrpt Controller;


    // Start is called before the first frame update
    void Start()
    {
        Controller = GetComponent<Enemy2_scrpt>();
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !Controller.isDead)
        {
            Controller.myAni.SetTrigger("Scare");
        }
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !Controller.isDead)
        {
            Controller.thePlayer = collision.gameObject;
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {

            Controller.thePlayer = null;
            Controller.NearDirToPLayer = Vector2.zero;
            Controller.getDir();
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (Controller.myBxC.IsTouching(collision.collider) && collision.collider.gameObject.CompareTag("chAtk") && !Controller.isDead)
        {

            Controller.Health--;

            Controller.Dies();

        }


    }

}
