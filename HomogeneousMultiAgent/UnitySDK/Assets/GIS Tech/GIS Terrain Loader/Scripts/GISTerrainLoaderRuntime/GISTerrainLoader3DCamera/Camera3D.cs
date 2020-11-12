/*     Unity GIS Tech 2019-2020      */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class Camera3D : MonoBehaviour
    {
        // Public fields
        [Header("Camera Parameters")]
        [Space(2)]
        public float panSpeed = 15.0f;
        public float zoomSpeed = 100.0f;
        public float rotationSpeed = 50.0f;

        public float mousePanMultiplier = 0.1f;
        public float mouseRotationMultiplier = 0.2f;
        public float mouseZoomMultiplier;

        public float minZoomDistance = 20.0f;
        public float maxZoomDistance = 2.0f;

        public float smoothingFactor = 0.1f;
        public float goToSpeed = 0.1f;
        [Header("Enable/Disbale Parameters")]
        [Space(2)]
        public bool useKeyboardInput = true;
        public bool useMouseInput = true;
        public bool adaptToTerrainHeight = true;
        public bool increaseSpeedWhenZoomedOut = true;
        public bool correctZoomingOutRatio = true;
        public bool smoothing = true;

        public GameObject objectToFollow;
        [HideInInspector]
        public Vector3 cameraTarget;
        private float currentCameraDistance;
        private Vector3 lastMousePos;
        private Vector3 lastPanSpeed = Vector3.zero;
        private Vector3 goingToCameraTarget = Vector3.zero;
        private bool doingAutoMovement = false;
        private Vector3 lastpos;
        // Use this for initialization
        public void Start()
        {
            lastpos = this.transform.position;
            currentCameraDistance = minZoomDistance + ((maxZoomDistance - minZoomDistance) / 2.0f);
            lastMousePos = new Vector3(123.8f, 23.1f, 49);
            transform.position = new Vector3(123.8f, 23.1f, 49);
            cameraTarget = new Vector3(123.8f, 23.1f, 49);
            this.transform.position = lastpos;
        }

        // Update is called once per frame
        public void Update()
        {

            UpdatePanning();
            UpdateRotation();
            UpdateZooming();
            UpdatePosition();
            UpdateAutoMovement();
            lastMousePos = Input.mousePosition;
        }

        public void GoTo(Vector3 position)
        {
            doingAutoMovement = true;
            goingToCameraTarget = position;
            objectToFollow = null;
        }

        public void Follow(GameObject gameObjectToFollow)
        {
            objectToFollow = gameObjectToFollow;
        }
        private void UpdatePanning()
        {
            Vector3 moveVector = new Vector3(0, 0, 0);
            if (useKeyboardInput)
            {

                //! rewrite to adress xyz seperatly
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    moveVector.x -= 1;

                }
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    moveVector.z -= 1;

                }
                if (Input.GetKey(KeyCode.RightArrow))
                {
                    moveVector.x += 1;
                }
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    moveVector.z += 1;
                }

            }
            if (useMouseInput)
            {
                if (Input.GetMouseButton(0))
                {
                    Vector3 deltaMousePos = (Input.mousePosition - lastMousePos);
                    moveVector += new Vector3(-deltaMousePos.x, 0, -deltaMousePos.y) * mousePanMultiplier;
                }
                if (Input.GetMouseButton(2))
                {
                    Vector3 deltaMousePos = (Input.mousePosition - lastMousePos);
                    moveVector += new Vector3(0, -deltaMousePos.y, 0) * mousePanMultiplier;
                }
            }
            if (moveVector != Vector3.zero)
            {
                objectToFollow = null;
                doingAutoMovement = false;
            }
            var effectivePanSpeed = moveVector;
            if (smoothing)
            {
                effectivePanSpeed = Vector3.Lerp(lastPanSpeed, moveVector, smoothingFactor);
                lastPanSpeed = effectivePanSpeed;
            }
            var oldXRotation = transform.localEulerAngles.x;
            // Set the local X rotation to 0;
            transform.SetLocalEulerAngles(0.0f);
            float panMultiplier = increaseSpeedWhenZoomedOut ? (Mathf.Sqrt(currentCameraDistance)) : 1.0f;
            cameraTarget = cameraTarget + transform.TransformDirection(effectivePanSpeed) * panSpeed * panMultiplier * Time.deltaTime;
            // Set the old x rotation.
            transform.SetLocalEulerAngles(oldXRotation);
        }

        private void UpdateRotation()
        {
            float deltaAngleH = 0.0f;
            float deltaAngleV = 0.0f;

            if (useKeyboardInput)
            {
                if (Input.GetKey(KeyCode.Q))
                {
                    deltaAngleH = 1.0f;
                }
                if (Input.GetKey(KeyCode.E))
                {
                    deltaAngleH = -1.0f;
                }
            }

            if (useMouseInput)
            {
                if (Input.GetMouseButton(1))
                {
                    var deltaMousePos = (Input.mousePosition - lastMousePos);
                    deltaAngleH += deltaMousePos.x * mouseRotationMultiplier;
                    deltaAngleV -= deltaMousePos.y * mouseRotationMultiplier;
                }
            }

            transform.SetLocalEulerAngles(
                Mathf.Min(80.0f, Mathf.Max(5.0f, transform.localEulerAngles.x + deltaAngleV * Time.deltaTime * rotationSpeed)),
                transform.localEulerAngles.y + deltaAngleH * Time.deltaTime * rotationSpeed
            );
        }

        private void UpdateZooming()
        {
            float deltaZoom = 0.0f;
            if (useKeyboardInput)
            {
                if (Input.GetKey(KeyCode.F))
                {
                    deltaZoom = 1.0f;
                }
                if (Input.GetKey(KeyCode.R))
                {
                    deltaZoom = -1.0f;
                }
            }
            if (useMouseInput)
            {
                var scroll = Input.GetAxis("Mouse ScrollWheel");

                deltaZoom -= scroll * mouseZoomMultiplier;

                var zoomedOutRatio = correctZoomingOutRatio ? (currentCameraDistance - minZoomDistance) / (maxZoomDistance - minZoomDistance) : 0.0f;

                if (scroll > 0 || scroll < 0)
                {
                    minZoomDistance -= minZoomDistance * scroll * mouseZoomMultiplier * Time.deltaTime;
                    currentCameraDistance = Mathf.Max(minZoomDistance, Mathf.Min(maxZoomDistance, currentCameraDistance + deltaZoom * Time.deltaTime * zoomSpeed * (zoomedOutRatio * 2.0f + 1.0f)));
                }
            }

        }

        private void UpdatePosition()
        {
            if (objectToFollow != null)
            {
                cameraTarget = Vector3.Lerp(cameraTarget, objectToFollow.transform.position, goToSpeed);
            }
            if (transform.position != Vector3.zero)
            {
                transform.position = cameraTarget;
                transform.Translate(Vector3.back * currentCameraDistance);

            }

        }

        private void UpdateAutoMovement()
        {
            if (doingAutoMovement)
            {
                cameraTarget = Vector3.Lerp(cameraTarget, goingToCameraTarget, goToSpeed);
                if (Vector3.Distance(goingToCameraTarget, cameraTarget) < 1.0f)
                {
                    doingAutoMovement = false;
                }
            }

        }

    }
}
