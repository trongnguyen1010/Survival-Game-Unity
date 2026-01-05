using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] NavMeshAgent agent;

    [Header("Stats")]
    [SerializeField] float damage = 1f;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float health = 10f;
    [SerializeField] int experienceToGive = 1;

    [Header("Knockback")]
    [SerializeField] float knockbackDuration = 0.2f;
    [SerializeField] float knockbackForce = 10f; // Chỉnh to lên (10-20)
    
    private bool isKnockedBack = false;

    [Header("Effects")]
    [SerializeField] GameObject destroyEffect;

    void Start()
    {
        if(agent == null) agent = GetComponent<NavMeshAgent>();
        if(rb == null) rb = GetComponent<Rigidbody2D>();

        // Cấu hình NavMesh
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = moveSpeed;
    }

    void FixedUpdate()
    {
        if (isKnockedBack) return; // Đang bị đẩy thì không làm gì cả

        if (PlayerController.Instance.gameObject.activeSelf)
        {
            // 1. Quay mặt
            if (PlayerController.Instance.transform.position.x > transform.position.x)
                spriteRenderer.flipX = true;
            else
                spriteRenderer.flipX = false;

            // 2. AI Tìm đường (Luôn đảm bảo AI bật khi đi săn)
            if (!agent.enabled)
            {
                agent.enabled = true;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
            
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(PlayerController.Instance.transform.position);
            }
        }
    }

    // --- HÀM BỊ ĐÁNH (PLAYER ĐÁNH QUÁI) ---
    public void TakeDamage(float dmg)
    {
        health -= dmg;
        
        if(DamageNumberController.Instance != null)
             DamageNumberController.Instance.CreateNumber(dmg, transform.position);

        // Kích hoạt đẩy lùi
        StopCoroutine("KnockbackRoutine");
        StartCoroutine("KnockbackRoutine");

        if (health <= 0)
        {
            Die();
        }
    }

    IEnumerator KnockbackRoutine()
    {
        isKnockedBack = true;
        
        // Tắt AI, Bật Vật lý
        agent.velocity = Vector3.zero;
        agent.enabled = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;

        // Đẩy
        // Ép kiểu (Vector2) để bỏ qua trục Z -> Sửa lỗi không đẩy được theo chiều dọc
        Vector2 enemyPos = transform.position;
        Vector2 playerPos = PlayerController.Instance.transform.position;
        Vector2 direction = (enemyPos - playerPos).normalized; 
        
        rb.linearVelocity = direction * knockbackForce;
        yield return new WaitForSeconds(knockbackDuration);

        // Hết giờ đẩy -> Reset
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        agent.enabled = true;
        isKnockedBack = false;
    }

    void Die()
    {
        Destroy(gameObject);
        if(destroyEffect != null) Instantiate(destroyEffect, transform.position, Quaternion.identity);
        if(PlayerController.Instance != null) PlayerController.Instance.GetExperience(experienceToGive);
    }

    // --- SỬA LẠI ĐOẠN NÀY NHƯ CŨ ---
    // Khi chạm vào Player -> Gây damge -> Nổ tung -> Chết
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 1. Trừ máu Player
            PlayerController.Instance.TakeDamge(damage);
            
            // 2. Tạo hiệu ứng nổ
            if (destroyEffect != null)
                Instantiate(destroyEffect, transform.position, transform.rotation);

            // 3. Hủy con quái ngay lập tức (Để nó không kịp đẩy Player)
            Destroy(gameObject);
        }
    }
}