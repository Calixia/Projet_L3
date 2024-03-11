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


    private bool canDash = true;
    private bool isDashing;
    [SerializeField] private float dashingPower = 24f;
    [SerializeField] private float dashingTime = 0.2f;
    [SerializeField] private float dashingCooldown = 1f;

    private float originalGravity;
    private float fastFallGravity;


    Animator animator;
    private void Start()
    {
        rb= GetComponent<Rigidbody2D>();
        originalGravity = rb.gravityScale;
        fastFallGravity = rb.gravityScale * 3;
        animator= GetComponent<Animator>();
        _collider= GetComponent<Collider2D>();
    }
    /*est effectué a chaque image affiché par le jeu*/
    private void Update()
    {
        isGrounded = IsGrounded();
        if (isDashing)
        {
            return;
        }
        
        if (canMove)
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            Flip();
            HandleAttack();
            HandleDash();
            HandleJump();
            HandleFall();
        }
        animator.SetFloat("Vy", rb.velocity.y);
        Animate();
    }

    /*Peut etre effectuer 0, 1 ou plusieurs fois a chaque image en fonction des parametres tout ce qui a un rapport avec la physique doit etre effectuer dans le fixedUpdate */
    private void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }
        if (canMove)
        {
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
        }
        
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

        if (Input.GetButtonUp("Jump") && rb.velocity.y>0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }
    }
    private void HandleFall()
    {
        if (Input.GetAxis("Vertical") < 0)
        {
            rb.gravityScale = fastFallGravity;
        }
        else
        {
            rb.gravityScale = originalGravity;

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

    private void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash  && isGrounded)
        {
            StartCoroutine(Dash());
        }
    }
    /*
     * controle les animations du joueur
     si il doit etre en train de tomber, courir ou etre a l'arret
    */
    private void Animate()
    {
        if (!isDashing)
        {
            if (isGrounded)
            {
                animator.SetBool("isFalling", false);
                if (horizontal!= 0f)
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
        horizontal = 0f;
        StartCoroutine(ResetCanMove());
    }
    
    public void Death()
    {
        canMove= false;
        horizontal = 0f;
        Time.timeScale = 0f;
    }

    IEnumerator ResetCanMove()
    {
        yield return new WaitForSeconds(staggerTime);
        canMove = true;
    }

    IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = originalGravity * 5f;
        rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        yield return new WaitForSeconds(dashingTime);
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
}
