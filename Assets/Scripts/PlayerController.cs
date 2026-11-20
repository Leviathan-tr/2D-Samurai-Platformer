using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float ultimateDuration = 3f;
    public float ultimateRange = 3f;
    public float ultimateHitInterval = 0.6f;
    public float ultimateDamagePerHit = 2.5f;
    public float maxHp = 100f;
    public float currentHp = 100f;
    public float speed = 5f;
    public float jumpForce = 3f;
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;
    public float attackDamage = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 0.2f;
    public float heavyDamage = 25f;
    public float holdThreshold = 0.8f;
    public float comboResetTime = 1.2f;
    public float skillCooldown = 5f;
    public float ultimateCooldown = 15f;
    public GameObject bulletPrefab;
    public Transform firePoint;
    private bool canUseSkill = true;
    private bool canUseUltimate = true;
    private bool isGrounded = true;
    private bool isDashing = false;
    private bool isKnockback = false;
    private bool canDash = true;
    private bool canAttack = true;
    private bool heavyTriggered = false;
    private bool isAlive = true;
    private bool isInvulnerable = false;
    private bool isUltimateActive = false;
    private bool cancelUltimateRequested = false;
    private bool secondHeartActive = false;
    private int comboCount = 0;
    private float lastAttackTime = 0f;
    private float mouseDownTime = 0f;
    private Rigidbody2D rb;
    private float horizontal;
    private float facingDirection = 1f;
    private int characterType;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector3 checkpointPosition;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        checkpointPosition = transform.position;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        characterType = PlayerPrefs.GetInt("CharacterType", 0);
        if (characterType == 0) { maxHp = 100f; currentHp = 100f; speed = 5f; attackDamage = 15f; }
    }

    void Update()
    {   if (!isAlive) return;
        horizontal = 0f;
        if (Keyboard.current.aKey.isPressed) horizontal = -1f;
        if (Keyboard.current.dKey.isPressed) horizontal = 1f;
        if (horizontal != 0f)
        {
            facingDirection = Mathf.Sign(horizontal);

            if (spriteRenderer != null)
                spriteRenderer.flipX = facingDirection < 0f;
        }
        // Velocity tabanlı speed: fizik hızını okuyarak animasyonu sürüyoruz
        // Dash veya knockback sırasında rb.linearVelocity.x zaten set edilmiş olduğundan doğru çalışır
        float absVelX = Mathf.Abs(rb.linearVelocity.x);
        float speedValue = absVelX > 0.1f ? 1f : 0f;
        animator.SetFloat("speed", speedValue);
        animator.SetBool("isGrounded", isGrounded);

        if (Keyboard.current.leftShiftKey.wasPressedThisFrame && canDash && !isDashing)
            StartCoroutine(Dash());
            

        if (Keyboard.current.wKey.wasPressedThisFrame && isGrounded)
        { isGrounded = false; rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); }

        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        { isGrounded = false; rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); }

        HandleAttackInput(
            Mouse.current.leftButton.wasPressedThisFrame,
            Mouse.current.leftButton.isPressed,
            Mouse.current.leftButton.wasReleasedThisFrame
        );

        HandleAttackInput(
            Keyboard.current.rightShiftKey.wasPressedThisFrame,
            Keyboard.current.rightShiftKey.isPressed,
            Keyboard.current.rightShiftKey.wasReleasedThisFrame
        );
           if (Keyboard.current.eKey.wasPressedThisFrame && canUseSkill && isAlive)
            StartCoroutine(UseSkill());
        if (Keyboard.current.rKey.wasPressedThisFrame && isAlive)
            if (isUltimateActive)
            {
                cancelUltimateRequested = true;
                Debug.Log("utli iptal istendi");
            }
            else if (canUseUltimate)
                StartCoroutine(UseUltimate());
        
    }

    void HandleAttackInput(bool pressed, bool held, bool released)
    {
        if (pressed) { mouseDownTime = Time.time; heavyTriggered = false; }

        if (held && !heavyTriggered && canAttack)
        {
            if (Time.time - mouseDownTime >= holdThreshold)
            { heavyTriggered = true; StartCoroutine(HeavyAttack()); }
        }

        if (released && canAttack && !heavyTriggered)
        {
            if (Time.time - lastAttackTime > comboResetTime)
                comboCount = 0;

            comboCount++;

            if (characterType == 0) // Melee
            {
                animator.SetInteger("comboStep", comboCount);
                StartCoroutine(LightAttack());

                if (comboCount >= 5)
                    comboCount = 0;
            }/*
            else if (characterType == 1) // Ranged
            {
                if (comboCount >= 5)
                {
                    comboCount = 0;
                    StartCoroutine(HeavyAttack());
                }
               
        }
            */

            lastAttackTime = Time.time;
        }
    }

    void FixedUpdate()
    {
        if (!isAlive || isDashing || isKnockback) return;
        if (isDashing || isKnockback) return;
        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) isGrounded = false;
    }

    public void TakeDamage(float damage)
    {
        if (!isAlive || isInvulnerable || isDashing)
        {
            return;
        }

        currentHp -= damage;

        if (currentHp <= 0)
        {
            isAlive = false;
            rb.linearVelocity = Vector2.zero;
            StartCoroutine(Respawn());
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;
        rb.linearVelocity = new Vector2(facingDirection * dashSpeed, 0f);
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private IEnumerator LightAttack()
    {
        canAttack = false;
        animator.SetTrigger("attack");
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (Collider2D enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                EnemyAI e = enemy.GetComponent<EnemyAI>();
                if (e != null) e.TakeDamage(attackDamage);
            }
        }
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private IEnumerator HeavyAttack()
    {
        canAttack = false;
        animator.SetTrigger("heavy");
       /* if (characterType == 1)
        { 
            isKnockback = true;
            Vector2 knockback = horizontal >= 0 ? Vector2.left : Vector2.right;
            rb.linearVelocity = new Vector2(knockback.x * dashSpeed * 0.6f, rb.linearVelocity.y);
            yield return new WaitForSeconds(0.2f);
            isKnockback = false;
        }*/
        if (characterType == 0)
        {
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, attackRange * 1.5f);
            foreach (Collider2D enemy in enemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    EnemyAI e = enemy.GetComponent<EnemyAI>();
                    if (e != null) e.TakeDamage(heavyDamage);
                }
            }
        }
        yield return new WaitForSeconds(attackCooldown * 2f);
        canAttack = true;
    }

    private IEnumerator UseSkill() // E hızlı kesme
    {
        canUseSkill = false;
        Debug.Log("e skilli tetiklendi");
        canAttack = false;
        yield return new WaitForSeconds(0.1f);
        float facing = facingDirection;
        Vector2 boxCenter = (Vector2)transform.position + new Vector2(facing * attackRange, 0f);
        Vector2 boxSize = new Vector2(attackRange *2f, 1.5f);
        Collider2D[] enemies = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f);
        foreach (Collider2D enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();

                if (enemyAI != null)
                {
                    enemyAI.TakeDamage(attackDamage * 2.5f);
                    Debug.Log("E skill düşmana vurdu: " + enemy.name);
                }
            }
        }
        canAttack = true;
        yield return new WaitForSeconds(skillCooldown);
        canUseSkill = true;
    }
    private IEnumerator UseUltimate()
    {
        canUseUltimate = false;
        isUltimateActive = true;
        cancelUltimateRequested = false;
        isInvulnerable = true;
        Debug.Log("ulti aktif edildi");

        float elapsed = 0f;

        while (elapsed < ultimateDuration && !cancelUltimateRequested)
        {
            float facing = facingDirection;

            Vector2 boxCenter =
                (Vector2)transform.position
                + new Vector2(facing * ultimateRange / 2f, 0f);

            Vector2 boxSize = new Vector2(ultimateRange, 1.5f);

            Collider2D[] enemies =
                Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f);

            foreach (Collider2D enemy in enemies)
            {
                EnemyAI enemyAI = enemy.GetComponentInParent<EnemyAI>();

                if (enemyAI != null)
                    enemyAI.TakeDamage(ultimateDamagePerHit);
            }

            float waitDuration =
                Mathf.Min(ultimateHitInterval, ultimateDuration - elapsed);

            float waited = 0f;

            while (waited < waitDuration && !cancelUltimateRequested)
            {
                yield return null;
                waited += Time.deltaTime;
            }

            elapsed += Mathf.Min(waited, waitDuration);
        }

        isInvulnerable = false;
        isUltimateActive = false;

        float remainingCooldown =
            Mathf.Max(0f, ultimateCooldown - elapsed);

        yield return new WaitForSeconds(remainingCooldown);
        canUseUltimate = true;
    }
    public void SetCheckpoint(Vector3 position)
    {
        checkpointPosition = position;
    }
    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(0.5f);
        transform.position = checkpointPosition;
        rb.linearVelocity = Vector2.zero;
        currentHp = maxHp;
        isAlive = true;
    }
}
