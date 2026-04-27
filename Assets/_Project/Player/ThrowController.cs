using UnityEngine;

public class ThrowController : MonoBehaviour
{
    [Header("Settings")]
    public float pickupRange = 5f;
    public float throwForce = 15f;

    [Header("References")]
    public Transform holdPoint;
    public Camera cam;

    private ThrowableObject heldObject;

    void Start()
    {
        // 자동 연결 (안전성 확보)
        if (cam == null)
            cam = Camera.main;

        if (holdPoint == null)
        {
            Transform found = transform.Find("HoldPoint");
            if (found != null)
                holdPoint = found;
        }
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // 좌클릭: 집기 / 던지기
        if (Input.GetMouseButtonDown(0))
        {
            if (heldObject == null)
                TryPickup();
            else
                Throw();
        }
    }

    void TryPickup()
    {
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            ThrowableObject obj = hit.collider.GetComponent<ThrowableObject>();
            if (obj != null)
            {
                heldObject = obj;
                heldObject.Pickup(holdPoint);
            }
        }
    }

    void Throw()
    {
        if (heldObject == null) return;

        Vector3 direction = GetThrowDirection();

        heldObject.Throw(direction * throwForce);
        heldObject = null;
    }

    Vector3 GetThrowDirection()
    {
        if (cam != null)
            return cam.transform.forward;

        return transform.forward;
    }
}