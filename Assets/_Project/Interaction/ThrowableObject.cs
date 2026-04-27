using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ThrowableObject : MonoBehaviour
{
    [Header("Settings")]
    public float damageMultiplier = 2f;

    private Rigidbody rb;
    private bool isHeld = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Pickup(Transform holdPoint)
    {
        isHeld = true;

        rb.isKinematic = true;
        rb.velocity = Vector3.zero;

        transform.parent = holdPoint;
        transform.localPosition = Vector3.zero;
    }

    public void Throw(Vector3 force)
    {
        isHeld = false;

        transform.parent = null;
        rb.isKinematic = false;

        rb.AddForce(force, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isHeld) return;

        // 인터페이스 기반 공격 처리
        var target = collision.collider.GetComponent<IDamageable>();

        if (target != null)
        {
            float damage = CalculateDamage();
            target.TakeDamage(damage);
        }
    }

    float CalculateDamage()
    {
        return rb.velocity.magnitude * damageMultiplier;
    }
}