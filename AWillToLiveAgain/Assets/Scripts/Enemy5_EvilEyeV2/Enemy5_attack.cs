using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy5_attack : MonoBehaviour
{
    public Enemy5_Controller Controller;
    private CircleCollider2D myCC;

    private bool check = false;

    // Start is called before the first frame update
    void Start()
    {
        myCC = GetComponent<CircleCollider2D>();
        
        myCC.enabled = false;
        

    }

    // Update is called once per frame
    void Update()
    {

        check = Controller.isAttacking;


        if (check)
        {


            myCC.enabled = true;
            
        }
        else
        {


            myCC.enabled = false;
            
        }

    }
}
