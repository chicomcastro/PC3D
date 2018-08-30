using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PC3D
{
    public class CameraController : MonoBehaviour
    {
        // Camera movement variables
        public GameObject player;
        public float yLowClampAngle = -30.0f;
        public float yClampRange = 60.0f;
        public float mouseSensitivity = 50.0f;
        public float smoothing = 3.0f;

        private Vector2 _smoothMouse;
        private bool isCursorVisible = false;
        private Vector2 _mouseAbsolute;

        // Camera switcher variables
        public bool isFirstPerson = false;

        public Transform firstPerson;
        public Transform thirdPerson;

        private Vector3 firstCamOffset;
        private Vector3 thirdCamOffset;

        private Camera cam;

        public float camTransitionSpeed = 5.0f;

        void Start()
        {
            cam = Camera.main;

            firstCamOffset = firstPerson.position - transform.position;
            thirdCamOffset = thirdPerson.position - transform.position;

            if (isFirstPerson) // MELHORAR
            {
                cam.transform.position = firstPerson.position;
                return;
            }

            cam.transform.position = thirdPerson.position;
        }

        void Update()
        {
            // Camera movement controller
            Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            mouseDelta = Vector2.Scale(mouseDelta, new Vector2(mouseSensitivity, mouseSensitivity));

            _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing);
            _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing);

            _mouseAbsolute += _smoothMouse;

            _mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, yLowClampAngle, yLowClampAngle + yClampRange);

            // Rotation applying
            transform.localRotation = Quaternion.Euler(-_mouseAbsolute.y, _mouseAbsolute.x, 0f);

            // Camera switcher controller
            transform.position = player.transform.position;

            if (Input.GetKeyDown(KeyCode.F))
            {
                isFirstPerson = !isFirstPerson;
            }

            if (isFirstPerson)
            {
                cam.transform.position = Vector3.Lerp(cam.transform.position, firstPerson.position, camTransitionSpeed * Time.deltaTime);
                return;
            }

            cam.transform.position = Vector3.Lerp(cam.transform.position, thirdPerson.position, camTransitionSpeed * Time.deltaTime);

            cam.transform.LookAt(player.transform.position);
        }

        private void FixedUpdate()
        {
            if (!isCursorVisible)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                isCursorVisible = !isCursorVisible;
            }
        }
    }
}
