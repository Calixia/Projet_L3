using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class damageHandler : MonoBehaviour
{

    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int currentHealth;

    [SerializeField] private bool canBeDamaged=true;
    [SerializeField] private int staggerTime;

    [SerializeField] private string ennemyTag, ennemyAttackTag;
    MovementController moveControl;
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        moveControl = GetComponent<MovementController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (canBeDamaged && (collision.gameObject.CompareTag(ennemyTag) || collision.gameObject.CompareTag(ennemyAttackTag)) )
        {
            currentHealth--;
            if (currentHealth <= 0)
                death();
            else
                stagger();
        }
    }
    private void death()
    {
        moveControl.Death();
    }
    private void stagger() 
    {
        Debug.Log("you got hit");
        /*temps pendant lequel on ne prend pas de dégats et animation*/
        canBeDamaged = false;
        //animator.SetTrigger("stagger");//possible ajout d'une force avec le rigidbody pour la direction vers laquelle on est projeté
        StartCoroutine(resetCanBeDamaged());
        moveControl.Hit();
    }

    IEnumerator resetCanBeDamaged()
    {
        yield return new WaitForSeconds(staggerTime);
        canBeDamaged = true;
    }
    
}
