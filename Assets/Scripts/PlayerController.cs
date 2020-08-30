using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Vector2 _movement;
    
    public float moveSpeed = 2f;

    public Rigidbody2D rb;
    public Animator animator;

    public bl_Joystick BLJoystick;

    private bool m_FacingRight = true;  // Флаг для определения направления взгляда персонажа

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // _movement.x = Input.GetAxisRaw("Horizontal");
        // _movement.y = Input.GetAxisRaw("Vertical");

        _movement.x = BLJoystick.Horizontal;
        _movement.y = BLJoystick.Vertical;
        
        animator.SetFloat("Horizontal", _movement.x);
        animator.SetFloat("Vertical", _movement.y);
        animator.SetFloat("Speed", _movement.sqrMagnitude);
    }

    void FixedUpdate()
    {
        Move(_movement.x);
    }

    public void Move(float move)
    {
        rb.MovePosition(rb.position + _movement * (moveSpeed * Time.deltaTime));

        // Если персонаж идёт вправо, а смотрит влево - поворот спрайта
        if (move > 0 && !m_FacingRight)
        {           
            Flip();
        }
        // Также если персонаж идёт влево, а смотрит вправо - поворот спрайта
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
