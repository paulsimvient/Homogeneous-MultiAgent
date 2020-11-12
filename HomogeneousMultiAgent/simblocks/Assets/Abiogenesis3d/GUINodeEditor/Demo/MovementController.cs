using UnityEngine;

[RequireComponent (typeof (GetMovementData))]
[RequireComponent (typeof (Rigidbody))]

public class MovementController : MonoBehaviour {
    Camera cam;
    Rigidbody rb;

    GetMovementData getMovementData;

    void Start () {
        cam = Camera.main;
        rb = GetComponent <Rigidbody> ();
    }

    void FixedUpdate () {
        cam.transform.rotation = Quaternion.Slerp(
            cam.transform.rotation,
            Quaternion.LookRotation(
                transform.position - cam.transform.position)
            , Time.deltaTime);

        if (transform.position.y < 0)
            transform.position = new Vector3 (0, 1, 0);

        GetMovementData getMovementData = GetComponent<GetMovementData> ();
        rb.AddTorque(Quaternion.Euler (0, 90, 0) * getMovementData.movementVector.normalized * 350);
    }
}
