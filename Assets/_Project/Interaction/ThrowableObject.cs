using UnityEngine;

public class ThrowableObject : MonoBehaviour
{
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

        var enemy = collision.collider.GetComponent<Enemy>();
        if (enemy != null)
        {
            float damage = rb.velocity.magnitude * 2f;
            enemy.TakeDamage(damage);
        }
    }
}