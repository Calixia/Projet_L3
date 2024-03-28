using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class damageHandler : MonoBehaviour
{
    [Header("health settings")]
    [SerializeField] private int maxHealth = 20;
     public int currentHealth { get; private set; }

    [Header("on damage settings")]
    public bool canBeDamaged=true;
    [SerializeField] private int staggerTime;

    [Header("damaging tags settings")]
    [SerializeField] private string ennemyTag;
    [SerializeField] private string ennemyAttackTag;
    [SerializeField] private healthPointUIController hpController;
    MovementController moveControl;
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        moveControl = GetComponent<MovementController>();
        hpController.damage(currentHealth);
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (canBeDamaged && !collision.collider.isTrigger && (collision.gameObject.CompareTag(ennemyTag)) && moveControl.myboxCollider.IsTouching(collision.collider))
        {
            currentHealth--;
            hpController.damage(currentHealth);
            if (currentHealth <= 0)
                death();
            else {
                if (collision.gameObject.CompareTag(ennemyTag))
                    stagger(collision.gameObject.transform.position);
            }

        }
    }
    private void death()
    {
        Debug.Log("GameOver");

        moveControl.Death();
    }
    private void stagger(Vector2 ennemypos) 
    {
        Debug.Log("you got hit");
        /*temps pendant lequel on ne prend pas de dégats et animation*/
        canBeDamaged = false;
        //animator.SetTrigger("stagger");//possible ajout d'une force avec le rigidbody pour la direction vers laquelle on est projeté
        StartCoroutine(resetCanBeDamaged());
        Vector2 force = new Vector2(transform.position.x - ennemypos.x, (transform.position.y - ennemypos.y)/3);
        moveControl.Hit(force);
    }

    IEnumerator resetCanBeDamaged()
    {
        yield return new WaitForSeconds(staggerTime);
        canBeDamaged = true;
    }
    
}
