using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy1_attack : MonoBehaviour
{
    public Enemy1IA_scrpt Controller;
    private CircleCollider2D[] myCCs;

    private bool check;
    // Start is called before the first frame update
    void Start()
    {

        myCCs = GetComponents<CircleCollider2D>();
        foreach(CircleCollider2D c in myCCs)
        {
            c.enabled = false;
        }

    }

    // Update is called once per frame
    void Update()
    {
        check = Controller.isAttacking;


        if (check)
        {
            foreach (CircleCollider2D c in myCCs)
            {
                c.enabled = true;
            }
        }
        else
        {
            foreach (CircleCollider2D c in myCCs)
            {
                c.enabled = false;
            }
        }

    }
}
