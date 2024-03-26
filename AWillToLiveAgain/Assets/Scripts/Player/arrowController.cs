using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class arrowController : MonoBehaviour
{
    public float speed=2f;
    public Vector2 playerPos;
    public float distanceMax;
    private float distance;
    public Vector2 direction = Vector2.right;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(direction*speed*Time.deltaTime);
        if (Vector2.Distance(playerPos, transform.position) >= distanceMax)
        {
            Destroy(this.gameObject);
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("touch!");
        if (collision.gameObject.CompareTag("Enm1") || collision.gameObject.CompareTag("Ground"))
            Destroy(this.gameObject,0.05f);

    }
}
