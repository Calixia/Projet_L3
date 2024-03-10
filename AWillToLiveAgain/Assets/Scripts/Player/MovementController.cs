using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    private float horizontal;
    [SerializeField] private float speed=8f;
    [SerializeField] private float jumpPower = 16f;
    private bool isFacingRight = true;

    private Rigidbody2D rb;
    private Collider2D _collider;


    [SerializeField] private int staggerTime;
    private bool canMove = true;

    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;

    private int idleCheck=5;


    Animator animator;
    private void Start()
    {
        rb= GetComponent<Rigidbody2D>();
        animator= GetComponent<Animator>();
        _collider= GetComponent<Collider2D>();
    }
    /*est effectué a chaque image affiché par le jeu*/
    private void Update()
    {
        if (canMove)
        {
            isGrounded=IsGrounded();
            horizontal = Input.GetAxisRaw("Horizontal");
            Flip();
            HandleAttack();
            Animate();
        }
    }

    /*Peut etre effectuer 0, 1 ou plusieurs fois a chaque image en fonction des parametres tout ce qui a un rapport avec la physique doit etre effectuer dans le fixedUpdate */
    private void FixedUpdate()
    {
        rb.velocity=new Vector2(horizontal*speed,rb.velocity.y);
        animator.SetFloat("Vy", rb.velocity.y);

        HandleJump();
        
    }

    /*
     Controle les sauts du joueur
    si le bouton saut est appuyer(espace) alors on ajoute une force vers le haut au déplacement du joueur, si il est relaché la hauteur du saut est diminuée
     */
    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpPower);
            animator.SetTrigger("jump");
        }

        if (Input.GetButtonUp("Jump"))
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }
    }

    /*
     Change le coté vers lequel le sprite du joueur est tourné
     */
    private void Flip()
    {
        if(isFacingRight && horizontal < 0f || !isFacingRight && horizontal>0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x*=-1f;
            transform.localScale = localScale;
        }
    }

    /*
    Controle les attaques du joueur
    si il y a un clic gauche (peut etre changer) effectuer une attaque
     */
    private void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("Attack");

        }
    }
    /*
     * controle les animations du joueur
     si il doit etre en train de tomber, courir ou etre a l'arret
    */
    private void Animate()
    {
        if (isGrounded)
        {
            animator.SetBool("isFalling", false);
            if (rb.velocity.x != 0f)
            {
                animator.SetBool("isRunning", true);
                idleCheck = 5;
            }
            else
            {
                if (idleCheck > 0)
                {
                    idleCheck--;
                }
                else
                {
                    animator.SetBool("isRunning", false);
                }
            }
        }
        else
        {
            animator.SetBool("isFalling", true);
        }
    }
    //TODO: dash, attaque, détails:delai entre chute et fin de possibilité de saut

    private bool IsGrounded()
    {
        //crée trois sorte de lasers qui détectent si le sol se trouve sur le trajet du laser renvoie vraie si sur l'un des laser le sol est trouvé faux sinon
        //bas gauche
        if ((Physics2D.Raycast(new Vector2(_collider.bounds.min.x, _collider.bounds.min.y), Vector2.down, 0.15f, groundLayer))/*gauche*/ ||
            (Physics2D.Raycast(new Vector2(_collider.bounds.center.x, _collider.bounds.min.y), Vector2.down, 0.15f, groundLayer))/*milieu*/ ||
            (Physics2D.Raycast(new Vector2(_collider.bounds.max.x, _collider.bounds.min.y), Vector2.down, 0.15f, groundLayer))/*droite*/)
        { 
            return true; 
        }

        return false;
    }

    public void Hit() {
        canMove = false;
        StartCoroutine(ResetCanMove());
    }
    
    IEnumerator ResetCanMove()
    {
        yield return null;
    }
}
