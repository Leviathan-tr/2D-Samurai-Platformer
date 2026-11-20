using UnityEngine;
using System.Collections;



public class EnemyAI : MonoBehaviour
{
    public float hp = 200f;
    public float speed = 2f;
    public float detectionRange = 5f;
    public float attackRange = 1f;
    public float attackDamage = 5f;
    public float attackCooldown = 0.8f;
    public int point = 1;
    public int coinReward = 10;



    private Transform player;
    private Rigidbody2D rb;
    private bool canAttack = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player").transform;
    }
    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            if (canAttack)
                StartCoroutine(Attack());
        }
        else if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

    }
    void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);
    }
    private IEnumerator Attack()
    {
        canAttack = false;
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
            playerController.TakeDamage(attackDamage);
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    public void TakeDamage(float damage)
    {
        hp -= damage;

        if (hp <= 0)
        {
            ScoreManager scoreManager =
               FindAnyObjectByType<ScoreManager>();

            if (scoreManager != null)
            {
                scoreManager.AddCoins(coinReward);
                scoreManager.AddScore(point);
            }

            Destroy(gameObject);
        }
    }

}







