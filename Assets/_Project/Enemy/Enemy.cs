using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public float speed = 2f;
    public float maxHp = 20f;

    private float currentHp;

    private Transform player;
    private bool isDead = false;

    void Start()
    {
        currentHp = maxHp;

        // Player 자동 탐색
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (isDead) return;
        if (player == null) return;

        MoveToPlayer();
    }

    void MoveToPlayer()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            speed * Time.deltaTime
        );
    }

    // ★ 인터페이스 구현
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHp -= damage;

        OnHitFeedback();

        if (currentHp <= 0)
        {
            Die();
        }
    }

    void OnHitFeedback()
    {
        // 간단한 피격 효과 (확장 가능)
        transform.localScale *= 0.9f;

        // 이후 확장:
        // - 색상 변경
        // - 파티클
        // - 사운드
    }

    void Die()
    {
        isDead = true;

        // 이후 확장:
        // - 애니메이션
        // - 아이템 드랍
        // - 점수 증가

        Destroy(gameObject);
    }
}