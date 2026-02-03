// 2D 角色控制器示例：根据输入切换动画（idle / walk / run / jump / death）
using System.Collections;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    [Header("引用")]
    public SpineAnimationManager animationManager;

    [Header("移动")]
    public float moveSpeed = 5f;

    private Vector2 movement;
    private bool isGrounded = true;
    private bool isDead = false;

    void Update()
    {
        if (isDead) return;
        HandleInput();
        UpdateAnimationState();
    }

    void HandleInput()
    {
        movement.x = Input.GetAxis("Horizontal");
        movement.y = Input.GetAxis("Vertical");

        if (Input.GetButtonDown("Jump") && isGrounded)
            PlayJumpAnimation();
    }

    void UpdateAnimationState()
    {
        if (animationManager == null) return;

        if (movement.magnitude > 0.1f)
        {
            if (Mathf.Abs(movement.x) > 0)
            {
                var ls = transform.localScale;
                transform.localScale = new Vector3(
                    movement.x > 0 ? Mathf.Abs(ls.x) : -Mathf.Abs(ls.x),
                    ls.y,
                    ls.z
                );
            }

            if (Input.GetKey(KeyCode.LeftShift))
                animationManager.CrossFadeAnimation("run", 0.2f);
            else
                animationManager.CrossFadeAnimation("walk", 0.2f);
        }
        else
        {
            animationManager.CrossFadeAnimation("idle", 0.2f);
        }
    }

    void PlayJumpAnimation()
    {
        if (animationManager == null) return;
        animationManager.PlayAnimation("jump", false);
        StartCoroutine(WaitForAnimationComplete("jump", () =>
        {
            if (animationManager != null)
                animationManager.CrossFadeAnimation("idle", 0.2f);
        }));
    }

    /// <summary>播放死亡动画并在结束后隐藏</summary>
    public void PlayDeathAnimation()
    {
        if (animationManager == null) return;
        isDead = true;
        animationManager.PlayAnimation("death", false);
        StartCoroutine(WaitForAnimationComplete("death", () => gameObject.SetActive(false)));
    }

    IEnumerator WaitForAnimationComplete(string animName, System.Action onComplete)
    {
        float duration = animationManager != null ? animationManager.GetAnimationDuration(animName) : 1f;
        if (duration <= 0f) duration = 1f;
        yield return new WaitForSeconds(duration);
        onComplete?.Invoke();
    }
}
