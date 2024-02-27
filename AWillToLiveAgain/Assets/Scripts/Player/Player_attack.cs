using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_attack : MonoBehaviour
{
    public Player_scrpt PlMouvScript;
    private CircleCollider2D myCC;

    private bool check = false;
    
    // Start is called before the first frame update
    void Start()
    {
        //gameObject.SetActive(false);
        myCC = GetComponent<CircleCollider2D>();
        myCC.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        check = PlMouvScript.isAttacking;


        if (check)
        {
            myCC.enabled = true;
        }
        else
        {
           myCC.enabled=false;
        }
        
    }


}
