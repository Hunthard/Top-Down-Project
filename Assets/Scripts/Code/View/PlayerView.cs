using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private Animator animator;

    private bool _isLookAtRight = true;
    
    private static readonly int Horizontal = Animator.StringToHash("Horizontal");
    private static readonly int Vertical = Animator.StringToHash("Vertical");
    private static readonly int Speed = Animator.StringToHash("Speed");

    public void Move(Vector2 movement, bool needFlip, float dt, float moveSpeed)
    {
        PlayMoveAnim(movement);
        MoveTransform(movement, needFlip, dt, moveSpeed);
    }

    
    private void MoveTransform(Vector2 movement, bool isLookAtRight, float dt, float moveSpeed)
    {
        rb.MovePosition(rb.position + movement * (moveSpeed * dt));
        
        if (_isLookAtRight != isLookAtRight)
        {
            _isLookAtRight = isLookAtRight;
            Flip();
        }
    }

    private void Flip()
    {
        // Отзеркаливание спрайта персонажа по оси X
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
    
    private void PlayMoveAnim(Vector2 movement)
    {
        animator.SetFloat(Horizontal, movement.x);
        animator.SetFloat(Vertical, movement.y);
        animator.SetFloat(Speed, movement.sqrMagnitude);
    }
}
