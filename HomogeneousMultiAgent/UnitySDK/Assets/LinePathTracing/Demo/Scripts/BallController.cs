using UnityEngine;
using System.Collections;

public class BallController : MonoBehaviour
{
    [SerializeField]
    private Transform _camPivotTransform;

    [SerializeField]
    private Transform _camTransform;

    private Rigidbody _rb;

    private float _moveForce = 300f;
    private float _jumpForce = 300f;
    private float _translationSpeed = 50f;

    private bool _inAir = false;


    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter()
    {
        _inAir = false;
    }

    private void OnTriggerExit()
    {
        _inAir = true;
    }

    private void FixedUpdate()
    {
        _camPivotTransform.position = Vector3.Lerp(_camPivotTransform.position,
            transform.position, Time.fixedDeltaTime * 10f);

        float moveForce = _moveForce;

        if (_inAir)
            moveForce *= 0.5f;

        if (Input.GetKey(KeyCode.Space) && !_inAir)
        {
            Vector3 vel = _rb.velocity;
            vel.y = 0;
            _rb.velocity = vel;

            _rb.AddForce(Vector3.up * _jumpForce);

            _inAir = true;
        }

        if (Input.GetKey(KeyCode.W))
            _rb.AddForce(Vector3.forward * moveForce * Time.fixedDeltaTime);

        if (Input.GetKey(KeyCode.S))
            _rb.AddForce(Vector3.back * moveForce * Time.fixedDeltaTime);

        if (Input.GetKey(KeyCode.A))
            _rb.AddForce(Vector3.left * moveForce * Time.fixedDeltaTime);

        if (Input.GetKey(KeyCode.D))
            _rb.AddForce(Vector3.right * moveForce * Time.fixedDeltaTime);
    }

    private void Update()
    {
        _camTransform.Translate(Vector3.forward * Input.mouseScrollDelta.y * _translationSpeed * Time.deltaTime);
    }
}
