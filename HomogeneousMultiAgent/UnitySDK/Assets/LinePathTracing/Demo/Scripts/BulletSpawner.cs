using UnityEngine;
using System.Collections;

public class BulletSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject _bullet;

    [SerializeField]
    private float _force = 5f;
    

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject go = GameObject.Instantiate(_bullet);
            go.GetComponent<Rigidbody>().AddForce(transform.forward * _force);
            go.transform.position = transform.position;
            go.transform.rotation = transform.rotation;
        }
    }
}
