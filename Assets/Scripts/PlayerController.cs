using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float moveSpeed = 2f;

    public Rigidbody2D rb;
    public Animator animator;

    Vector2 movement;

    private bool m_FacingRight = true;  // Флаг для определения направления взгляда персонажа

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);
    }

    void FixedUpdate()
    {
        Move(movement.x);
    }

    public void Move(float move)
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.deltaTime);

        // Если персонаж идёт вправо, а смотрит влево - поворот спрайта
        if (move > 0 && !m_FacingRight)
        {           
            Flip();
        }
        // Также если персонаж идёт влево, а смотри вправо - поворот спрайта
        else if (move < 0 && m_FacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        // Поменять флаг направления взгляда персонажа
        m_FacingRight = !m_FacingRight;

        // Отзеркаливание спрайта персонажа по оси X
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}
