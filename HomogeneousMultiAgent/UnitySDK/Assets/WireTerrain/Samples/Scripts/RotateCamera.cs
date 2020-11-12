using UnityEngine;
using System.Collections;

namespace WireTerrain
{
    public class RotateCamera : MonoBehaviour
    {
        private float amount;
        [SerializeField]
        private float angleLimit = 10;
        private float currentVelocity = 64f;
        private Vector3 euler;
        void Start()
        {
            euler = transform.eulerAngles;
        }

        void Update()
        {
            float amountTarget = Input.mousePosition.x / Screen.width;
            amount = Mathf.SmoothDamp(amount, amountTarget, ref currentVelocity, 0.75f);
            transform.eulerAngles = euler + new Vector3(0, amount * angleLimit - 0.5f, 0);
        }
    }
}
