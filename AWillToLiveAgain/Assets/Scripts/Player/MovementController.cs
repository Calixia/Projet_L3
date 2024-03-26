using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [Header("Movement settings")]
    [SerializeField] private float speed = 8f;
    private float horizontal;
    [HideInInspector] public bool isFacingRight = true;

    [Header("jump settings")]
    [SerializeField] private float jumpPower = 16f;
    private bool canJump = true;
    private bool isCrouched = false;
    

    private Rigidbody2D rb;
    private Collider2D _collider;

    [Header("Bow settings")]
    [SerializeField] GameObject arrowPrefab;
    [SerializeField] private float arrowTime;

    [SerializeField] private float arrowCooldown;
    [SerializeField] private float arrowMaxDistance;

    private bool isShootingArrow = false;

    private bool canShootArrow = true;

    [Header("Stagger settings")]
    [SerializeField] private int staggerTime;
    [SerializeField] private float staggerPower;
    private bool canMove = true;

    [Header("Grounded settings")]
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;

    private int idleCheck=5;

    [Header("dash settings")]
    [SerializeField] private float dashingPower = 24f;
    [SerializeField] private float dashingTime = 0.2f;
    [SerializeField] private float dashingCooldown = 1f;
    private bool canDash = true;
    private bool isDashing;

    [Header("fall settings")]
    [SerializeField] float fallGravityMultiplier=2;
    [SerializeField] float fastFallGravityMultiplier=3;
    private float originalGravity;
    private float fallGravity;
    private float fastFallGravity;

    [Header("Camera settings")]
    [SerializeField] private GameObject cameraFollowObject;
    private CameraFollowObject cameraFollowObjectScript;
    private float fallSpeedDampingChangeThreshold;

    Animator animator;
    private void Start()
    {

        rb = GetComponent<Rigidbody2D>();
        originalGravity = rb.gravityScale;
        fallGravity = rb.gravityScale * fallGravityMultiplier;

        fastFallGravity = rb.gravityScale * fastFallGravityMultiplier;
        animator= GetComponent<Animator>();
        _collider= GetComponent<Collider2D>();

        cameraFollowObjectScript=cameraFollowObject.GetComponent<CameraFollowObject>();

        fallSpeedDampingChangeThreshold = CameraManager.instance.fallSpeedYDampingChangeThreshold;
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
        //if player is falling past a certain threshold
        if(rb.velocity.y<fallSpeedDampingChangeThreshold && !CameraManager.instance.IsLerpingYDamping && !CameraManager.instance.LerpedFromPlayerFalling) 
        {
            CameraManager.instance.LerpYDamping(true);
        }
        if (rb.velocity.y >= 0f && !CameraManager.instance.IsLerpingYDamping && CameraManager.instance.LerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpedFromPlayerFalling = false;

            CameraManager.instance.LerpYDamping(false); 
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
        if (canJump)
        {
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpPower);
                animator.SetTrigger("jump");
            }
            if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
                animator.ResetTrigger("jump");
            }
            if (rb.velocity.y < 0f)
            {
                animator.ResetTrigger("jump");
            }
        }
        
    }
    private void HandleFall()
    {
        
        if (Input.GetAxis("Vertical") < 0)
        {
            canJump = false;
            isCrouched = true;
            if (rb.velocity.y > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x,0f) ;
            }
            rb.gravityScale = fastFallGravity;
            
            //transform.localScale = fastfallScale; AVEC LERP!!! réduis la taille pour rendre le fait qu'on tombe plus vite plus visible (pas extremement important)
        }
        else
        {
            canJump = true;
            if (rb.velocity.y < 0f)
            {
                rb.gravityScale = fallGravity;
            }
            else
            {
                rb.gravityScale = originalGravity;
            }
            isCrouched = false;
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
            cameraFollowObjectScript.CallTurn();
        }
    }

    /*
    Controle les attaques du joueur
    si il y a un clic gauche (peut etre changer) effectuer une attaque
     */
    private void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0) && !isShootingArrow)
        {
            animator.SetTrigger("Attack");

        }
        if (Input.GetMouseButtonDown(1) && canShootArrow)
        {
            StartCoroutine(Arrow());
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
            animator.SetBool("crouch", isCrouched);
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

    public void Hit(Vector2 force) {
        if (force.x >= 0)
            force.x = 0.5f;
        else
            force.x = -0.5f;
        if (force.y >= 0)
            force.y = 0.5f;
        else
            force.y = -0.5f;
        rb.velocity = Vector2.zero;
        rb.AddForce(force*staggerPower);
        canMove = false;
        horizontal = 0f;
        animator.SetTrigger("hurt");
        StartCoroutine(ResetCanMove());
    }

    public void Death()
    {
        canMove = false;
        horizontal = 0f;
        Time.timeScale = 0f;
    }
    IEnumerator ResetCanMove()
    {
        yield return new WaitForSeconds(staggerTime);
        animator.ResetTrigger("hurt");
        canMove = true;
    }

    IEnumerator Dash()
    {
        animator.SetBool("isDashing", true);
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = originalGravity * 5f;
        rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        yield return new WaitForSeconds(dashingTime);
        animator.SetBool("isDashing", false);
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    IEnumerator Arrow()
    {
        animator.SetBool("isShooting",true);
        Debug.Log("shoot");
        canShootArrow = false;
        isShootingArrow = true;
        yield return new WaitForSeconds(arrowTime);

        
        GameObject arrow = Instantiate(arrowPrefab, new Vector2(transform.position.x, transform.position.y),isFacingRight?Quaternion.identity:Quaternion.Euler(0f,0f,180f));
        arrowController ac=arrow.GetComponent<arrowController>();
        ac.direction =Vector2.right;
        ac.playerPos = transform.position;
        ac.distanceMax = arrowMaxDistance;
        isShootingArrow = false;
        animator.SetBool("isShooting", false);
        yield return new WaitForSeconds(arrowCooldown);
        canShootArrow = true;
    }
}
