using UnityEngine;

public class ThrowController : MonoBehaviour
{
    public float pickupRange = 5f;
    public float throwForce = 15f;
    public Transform holdPoint;
    public Camera cam;

    private ThrowableObject heldObject;

    void Update()
    {
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
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            var obj = hit.collider.GetComponent<ThrowableObject>();
            if (obj != null)
            {
                heldObject = obj;
                heldObject.Pickup(holdPoint);
            }
        }
    }

    void Throw()
    {
        heldObject.Throw(cam.transform.forward * throwForce);
        heldObject = null;
    }
}