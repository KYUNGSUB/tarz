using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform player;
    public float speed = 2f;
    public float hp = 20f;

    void Update()
    {
        if (player == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            speed * Time.deltaTime
        );
    }

    public void TakeDamage(float damage)
    {
        hp -= damage;

        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}